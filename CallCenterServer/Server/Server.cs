using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using SharedNameSpace;
using System.Linq;

namespace CallCenterServer {
    class Server {
        SqLiteOperator sqLite; //Sqlite operator for check users in database.

        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private readonly int BUFFERSIZE = 1024; //Max buffer size. In big list (>= 50 users) will do out of size exception
        private readonly int MAXCLIENTSCONNECTED = 200;
        private readonly bool LOCALHOST_MODE = config["LOCALHOST"].ToLower() == "true";
        private readonly string SERVER_PORT = string.IsNullOrWhiteSpace(config["SERVER_PORT"]) ? "4404" : config["SERVER_PORT"];

        private static readonly bool DEBUG_MODE = config["DEBUG_MODE"].ToLower() == "true";
        private readonly bool SAVE_CONSOLE_LOG_TO_FILE = config["CONSOLE_LOG_SAVE_FILE"].ToLower() == "true";



        Socket socket;
        Thread listenThread;
        ConcurrentDictionary<User, Socket> agentsOnline; //thread-safe collection class to store key/value pairs.
        ConcurrentDictionary<User, Socket> adminsOnline; //thread-safe collection class to store key/value pairs.
        public bool IsServerRunning; //To check if server is running
        public bool SHOW_CONSOLE_LOG = (config["CONSOLE_LOG"].ToLower() == "true" && !DEBUG_MODE);//Show console debug when not in debug mode



        /// <summary>
        /// Delegates that show server errors in console when they appears
        /// </summary>
        /// <param name="error">Error id</param>
        public delegate void ErrorDelegate(int error);
        private ErrorDelegate onErrorDelegate;

        public delegate void ShowConsoleLog(string logMessage);
        private ShowConsoleLog onShowConsoleLog;

        //Singleton pattern
        private static Server server;

        private Server() { }
        public static Server GetInstance() {
            if (server is null)
                server = new Server();

            return server;
        }

        /// <summary>
        /// Server start point
        /// </summary>
        /// <param name="errorDelegate">Method that will handle the errors in other side (Program.cs)</param>
        /// <param name="showConsoleLog">Method that will show the console log in other side (Program.cs)</param>
        public void StartServer(ErrorDelegate errorDelegate, ShowConsoleLog showConsoleLog) {
            onErrorDelegate = errorDelegate;
            onShowConsoleLog = showConsoleLog;
            try {

                ConsoleLogs(string.Format("Server starting on IP: {0}:{1}", GetMachineIP(), SERVER_PORT), "SYSTEM");


                IPAddress addr = IPAddress.Parse(GetMachineIP());
                if (!int.TryParse(SERVER_PORT, out int parsedServerPort)) {
                    //Server port unable to convert to int
                    onErrorDelegate(101);
                    return;
                }
                IPEndPoint endPoint = new IPEndPoint(addr, parsedServerPort);
                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);


                ConsoleLogs(string.Format("Server socket bind"), "SYSTEM");

                socket.Listen(MAXCLIENTSCONNECTED);

                ConsoleLogs(string.Format("Server max clients is {0}", MAXCLIENTSCONNECTED), "SYSTEM");
            } catch (FormatException e) {
                onErrorDelegate(100);
                ServerLog.GetInstance().WriteToExceptions(e);
                return;
            } catch (SocketException e) {
                ServerLog.GetInstance().WriteToExceptions(e);
            }
            sqLite = new SqLiteOperator();

            ConsoleLogs(string.Format("Creating new thread to listen clients"), "SYSTEM");

            listenThread = new Thread(Listen) {
                Priority = ThreadPriority.Highest
            };

            ConsoleLogs(string.Format("Setting thread priority to HIGH"), "SYSTEM");

            listenThread.Start();

            ConsoleLogs(string.Format("Thread started"), "SYSTEM");

            agentsOnline = new ConcurrentDictionary<User, Socket>();
            adminsOnline = new ConcurrentDictionary<User, Socket>();
            IsServerRunning = true;

            ConsoleLogs(string.Format("SYSTEM RUNNING CORRECTLY"), "SYSTEM");
        }


        private void ConsoleLogs(string msg, string msgType) {
            if (SHOW_CONSOLE_LOG || SAVE_CONSOLE_LOG_TO_FILE) {
                string message = "[" + DateTime.Now.ToString("HH:mm:ss") + "][" + msgType + "]: " + msg;
                if (SHOW_CONSOLE_LOG)
                    onShowConsoleLog(message);

                if (SAVE_CONSOLE_LOG_TO_FILE)
                    ServerLog.GetInstance().WriteToConsole(message);
            }
        }

        /// <summary>
        /// Not totally implemented at the moment. Just for test purposes
        /// </summary>
        public void StopServer() {
            BroadCastMessageAllClients(new ServerResponse(ResponseContext.ServerMessage, "Server shutdown, disconnecting all clients."));

            Parallel.ForEach(agentsOnline, u => {
                Logout(new AdminOrder(OrderType.DisconnectUser, u.Key));
                u.Value.Close();
            });
            socket.Close();
            IsServerRunning = false;
        }

        /// <summary>
        /// Debbug method that show online users in server
        /// </summary>
        /// <returns>A users online list</returns>
        public List<User> ConnectecUsers() {
            return agentsOnline.IsEmpty ? null : agentsOnline.Keys.ToList();
        }

        private string GetMachineIP() {
            string localIP = string.Empty;

            if (LOCALHOST_MODE)
                localIP = "127.0.0.1";
            else {
#pragma warning disable IDE0063 // Use la instrucción "using" simple
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
#pragma warning restore IDE0063 // Use la instrucción "using" simple
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
            }

            return localIP;
        }

        /// <summary>
        /// Ready to accept connection from clients
        /// </summary>
        private void Listen() {
            Socket client;
            do {
                try {
                    client = socket.Accept();

                    ConsoleLogs(string.Format("New client connection from {0}\nTOTAL CURRENT USERS CONNECTED: {1}", client.RemoteEndPoint, agentsOnline.Count()+1), "SYSTEM");

                    Thread listenClient = new Thread(ListenClient);
                    listenClient.Start(client);

                } catch {
                }
            } while (IsServerRunning);
        }

        /// <summary>
        /// Listen to client
        /// </summary>
        /// <param name="o">Socket client</param>
        private void ListenClient(object o) {
            Socket socket = (Socket)o;
            object received;
            do {
                received = Receive(socket);

                if (received is User u) {
                    User loggedUser;
                    if (DEBUG_MODE) //Don`t match users to database in debbug mode, only for test porpuses
                    {
                        loggedUser = u;
                        loggedUser.IsAdmin = true; //In debug mode all users are admins only for test
                    } else {
                        if (!sqLite.ValidateLogin(u)) //If user arent in database will get out of the thread poll. In debugg mode will accept all
                        {
                            Send(socket, new ServerResponse(ResponseContext.Login, null));
                            ServerLog.GetInstance().WriteToConnections(u, socket.RemoteEndPoint.ToString(), ConnectionAction.Wrong_Login);

                            ConsoleLogs(string.Format("Wrong login from {0} with user: {1}", socket.RemoteEndPoint, u.ToString()), "SYSTEM");

                            return;
                        }
                        loggedUser = sqLite.FindUser(u.Id);
                        ConsoleLogs(string.Format("Succes login from {0} with user: {1}", socket.RemoteEndPoint, loggedUser.ToString()), "SYSTEM");
                    }
                    Send(socket, new ServerResponse(ResponseContext.Login, loggedUser));
                    Thread.CurrentThread.Name = u.Name; //Change the thread name for be more friendly to debug/test
                    agentsOnline.TryAdd(loggedUser, socket);
                    ServerLog.GetInstance().WriteToConnections(loggedUser, socket.RemoteEndPoint.ToString(), ConnectionAction.Login);

                    if (loggedUser.IsAdmin)
                        adminsOnline.TryAdd(loggedUser, socket);

                    SendUserUpdateToAdmin(loggedUser);

                }
            } while (!(received is User)); //If client dont send user, can't connect to server

            do {
                received = Receive(socket);
                if (received is null) //Socket close
                    break;


                if (received is Call call) {
                    SendCall(call);
                    ConsoleLogs(string.Format("Incoming call to {0}", call.to.Id), "SYSTEM");
                    continue;
                }

                if (received is EndCall endCall) {
                    SendEndCall(endCall);
                    ConsoleLogs(string.Format("EndCall to {0}", endCall.to.Id), "SYSTEM");
                    continue;
                }

                if (received is AdminOrder aOrder) {
                    ConsoleLogs(string.Format("Received admin order: {0}", aOrder.OrderType.ToString()), "SYSTEM");
                    switch (aOrder.OrderType) {
                        case OrderType.GetUserList:
                            SendUserList(aOrder.UserToDoAction);
                            break;
                        case OrderType.SendMessage:
                            if (aOrder.UserToDoAction is null)
                                BroadCastMessageAllClients(new ServerResponse(ResponseContext.ServerMessage, aOrder.Message));
                            else
                                SendMessageToClient(aOrder);
                            break;
                        case OrderType.DisconnectUser:
                            Logout(aOrder);
                            break;
                        case OrderType.ManageDatabase:
                            ServerResponse serverResponse = new ServerResponse(ResponseContext.DatabaseManager, null);

                            User tmpUser = GetCurrentUserBySocket(socket, true);

                            switch (aOrder.DatabaseAction) {
                                case DatabaseAction.Create:
                                    ConsoleLogs(string.Format("Creating new user in database {0}", aOrder.UserToDoAction.ToString()), tmpUser.Id.ToUpper());
                                    sqLite.CreateNewUser(aOrder.UserToDoAction);
                                    serverResponse.ResponseObject = new DatabaseManagerResponse(true, sqLite.GetAllUsers().ToArray());
                                    break;
                                case DatabaseAction.Read:
                                    serverResponse.ResponseObject = sqLite.GetAllUsers().ToArray();
                                    break;
                                case DatabaseAction.Update:
                                    ConsoleLogs(string.Format("Updating user in database {0}", aOrder.UserToDoAction.ToString()), tmpUser.Id.ToUpper());
                                    sqLite.UpdateUser(aOrder.UserToDoAction);
                                    serverResponse.ResponseObject = new DatabaseManagerResponse(true, sqLite.GetAllUsers().ToArray());
                                    break;
                                case DatabaseAction.Delete:
                                    ConsoleLogs(string.Format("Deleting user in database {0}", aOrder.UserToDoAction.ToString()), tmpUser.Id.ToUpper());
                                    sqLite.DeleteUser(aOrder.UserToDoAction);
                                    serverResponse.ResponseObject = new DatabaseManagerResponse(true, sqLite.GetAllUsers().ToArray());
                                    break;
                            }
                            if (!(serverResponse.ResponseObject is null))
                                if (aOrder.UserToSendResponse is null)
                                    aOrder.UserToSendResponse = aOrder.UserToDoAction;
                            Send(agentsOnline[aOrder.UserToSendResponse], serverResponse);
                            break;
                    }
                }

            } while (true);

            //Close the resources and end the current thread.
            socket.Close();
        }


        /// <summary>
        /// Method to get the current User based on the socket
        /// </summary>
        /// <param name="userSocket">Client socket</param>
        /// <param name="isAdmin">To evaluate what list should use</param>
        /// <returns></returns>
        private User GetCurrentUserBySocket(Socket userSocket, bool isAdmin) {
            if (isAdmin) {
                return adminsOnline.FirstOrDefault(x => x.Value.Equals(userSocket)).Key;
            } else {
                return agentsOnline.FirstOrDefault(x => x.Value.Equals(userSocket)).Key;
            }
        }

        /// <summary>
        /// Send a new connection update to all admins clients
        /// </summary>
        /// <param name="u">New Logged user</param>
        private void SendUserUpdateToAdmin(User u) {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            foreach (KeyValuePair<User, Socket> client in adminsOnline)
                Send(client.Value, new ServerResponse(ResponseContext.ClientsManager, u));
        }

        /// <summary>
        /// Method to update admin users control. Send one ClientLogout to update UI
        /// </summary>
        /// <param name="u">User disconnected</param>
        private void SendLogoutToAdmin(User u) {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            foreach (KeyValuePair<User, Socket> client in adminsOnline) {
                if (client.Key.IsAdmin && u != client.Key)
                    Send(client.Value, new ServerResponse(ResponseContext.ClientsManager, new ClienLogout(u)));
            }
        }

        /// <summary>
        /// When admin open the tool for first time the method send one array with the actual online users
        /// </summary>
        /// <param name="u">Admin user logged</param>
        private void SendUserList(User u) {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            Send(agentsOnline[u], new ServerResponse(ResponseContext.ClientsManager, agentsOnline.Keys.ToList().ToArray()));
        }

        /// <summary>
        /// Send a object to all users connected
        /// </summary>
        /// <param name="response">Response fron the server that contains the message</param>
        public void BroadCastMessageAllClients(ServerResponse response) {
            ConsoleLogs(string.Format("New message for all: {0}", response.ResponseObject.ToString()), "SYSTEM"); ;
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
                Send(client.Value, response);
        }

        /// <summary>
        /// Send a message to the destinatary
        /// </summary>
        /// <param name="m">Message to send</param>
        private void SendMessageToClient(AdminOrder m) {
            Send(agentsOnline[m.UserToDoAction], m);
        }

        /// <summary>
        /// Send a call to the destinatary
        /// </summary>
        /// <param name="call">The incomming call</param>
        public void SendCall(Call call) {
            if (call.to is null && call.campaigne is null) {
                return;
            } else {
                try {
                    Send(agentsOnline[call.to], call);
                    agentsOnline.Keys.ToList().First(x => x == call.to).InCall = true;
                    call.to.InCall = true;
                    SendUserUpdateToAdmin(call.to);
                } catch {
                    onErrorDelegate(200);
                }
            }


        }

        /// <summary>
        /// Send the endcall to client
        /// </summary>
        /// <param name="endCall">The endcall with the user to send it</param>
        public void SendEndCall(EndCall endCall) {
            try {
                Send(agentsOnline[endCall.to], endCall);
                agentsOnline.Keys.ToList().First(x => x == endCall.to).InCall = false;
                endCall.to.InCall = false;
                SendUserUpdateToAdmin(endCall.to);
            } catch {
                onErrorDelegate(200);
            }

        }

        /// <summary>
        /// Get client Logout signal and dispose the resources
        /// </summary>
        /// <param name="aOrder">AdminOrder</param>
        public void Logout(AdminOrder aOrder) {
            Send(agentsOnline[aOrder.UserToDoAction], new ServerResponse(ResponseContext.Disconnect));
            agentsOnline.TryRemove(aOrder.UserToDoAction, out _);
            SendLogoutToAdmin(aOrder.UserToDoAction);
        }

        /// <summary>
        /// Send a object to the client
        /// </summary>
        /// <param name="s">Socket client</param>
        /// <param name="o">Object to send</param>
        private void Send(Socket s, object o) {
            byte[] buffer = new byte[BUFFERSIZE];
            byte[] obj = BinarySerialization.Serializate(o);
            Array.Copy(obj, buffer, obj.Length);
            try {
                s.Send(buffer);
            } catch (ObjectDisposedException e) {
                ServerLog.GetInstance().WriteToExceptions(e);
                onErrorDelegate(2);
            } catch (SocketException e) {
                ServerLog.GetInstance().WriteToExceptions(e);
            }
        }

        /// <summary>
        /// Receive all the serialized object
        /// </summary>
        /// <param name="s">Socket that receive the object</param>
        /// <returns>Object received from client</returns>
        private object Receive(Socket s) {
            byte[] buffer = new byte[BUFFERSIZE];
            try {
                s.Receive(buffer);
            } catch (SocketException) //If socket was closed, came here
              {
                foreach (var item in agentsOnline)
                    if (item.Value == s) {
                        if (item.Key.IsAdmin)//If user is admin delete from admin list
                            adminsOnline.TryRemove(item.Key, out _);
                        ServerLog.GetInstance().WriteToConnections(item.Key, item.Value.RemoteEndPoint.ToString(), ConnectionAction.Disconnect);
                        agentsOnline.TryRemove(item.Key, out _);
                        SendLogoutToAdmin(item.Key);
                        break;
                    }
                return null;
            }
            return BinarySerialization.Deserializate(buffer);
        }
    }
}
