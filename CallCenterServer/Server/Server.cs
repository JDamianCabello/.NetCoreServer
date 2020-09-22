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

namespace CallCenterServer
{
    class Server
    {
        SqLiteOperator sqLite; //Sqlite operator for check users in database.

        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private readonly int BUFFERSIZE = 1024; //Max buffer size. In big list (>= 50 users) will do out of size exception
        private readonly int MAXCLIENTSCONNECTED = 200;
        private readonly string SERVER_IP = string.IsNullOrWhiteSpace(config["SERVER_IP"]) ? "127.0.0.1" : config["SERVER_IP"];
        private readonly string SERVER_PORT = string.IsNullOrWhiteSpace(config["SERVER_PORT"]) ? "4404" : config["SERVER_PORT"];

        private static readonly bool DEBUG_MODE = config["DEBUG_MODE"].ToLower() == "true";


        Socket socket;
        Thread listenThread;
        ConcurrentDictionary<User, Socket> agentsOnline; //thread-safe collection class to store key/value pairs.
        ConcurrentDictionary<User, Socket> adminsOnline; //thread-safe collection class to store key/value pairs.
        public bool IsServerRunning; //To check if server is running


        /// <summary>
        /// Delegates that show server errors in console when they appears
        /// </summary>
        /// <param name="error">Error id</param>
        public delegate void ErrorDelegate(int error);
        private ErrorDelegate onErrorDelegate;

        //Singleton pattern
        private static Server server;

        private Server() { }
        public static Server GetInstance()
        {
            if (server is null)
                server = new Server();

            return server;
        }

        /// <summary>
        /// Server start point
        /// </summary>
        /// <param name="errorDelegate">Method that will handle the errors in other side (Program.cs)</param>
        public void StartServer(ErrorDelegate errorDelegate)
        {
            onErrorDelegate = errorDelegate;
            try
            {
                IPAddress addr = IPAddress.Parse(SERVER_IP);
                if (!int.TryParse(SERVER_PORT, out int parsedServerPort))
                {
                    //Server port unable to convert to int
                    onErrorDelegate(101);
                    return;
                }
                IPEndPoint endPoint = new IPEndPoint(addr, parsedServerPort);
                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(MAXCLIENTSCONNECTED);
            }
            catch (FormatException e)
            {
                onErrorDelegate(100);
                ServerLog.GetInstance().WriteToExceptions(e);
                return;
            }
            catch(SocketException e)
            {
                ServerLog.GetInstance().WriteToExceptions(e);
            }
            sqLite = new SqLiteOperator();


            listenThread = new Thread(Listen)
            {
                Priority = ThreadPriority.Highest
            };

            listenThread.Start();
            agentsOnline = new ConcurrentDictionary<User, Socket>();
            adminsOnline = new ConcurrentDictionary<User, Socket>();
            IsServerRunning = true;
        }


        /// <summary>
        /// Not totally implemented at the moment. Just for test purposes
        /// </summary>
        public void StopServer()
        {
            BroadCastMessageAllClients(new Message("Server shutdown, disconnecting all clients."));

            Parallel.ForEach(agentsOnline, u =>
            {
                Logout(new Logout(u.Key));
                u.Value.Close();
            });
            socket.Close();
            IsServerRunning = false;
        }

        /// <summary>
        /// Debbug method that show online users in server
        /// </summary>
        /// <returns>A users online list</returns>
        public List<User> ConnectecUsers()
        {   
            return agentsOnline.IsEmpty ? null : agentsOnline.Keys.ToList();
        }

        /// <summary>
        /// Ready to accept connection from clients
        /// </summary>
        private void Listen()
        {
            Socket client;
            do
            {
                try
                {
                    client = socket.Accept();
                    Thread listenClient = new Thread(ListenClient);
                    listenClient.Start(client);
                }
                catch
                {
                }
            } while (IsServerRunning);
        }

        /// <summary>
        /// Listen to client
        /// </summary>
        /// <param name="o">Socket client</param>
        private void ListenClient(object o)
        {
            Socket socket = (Socket)o;
            object received;
            do
            {
                received = Receive(socket);
                if (received is User u)
                {
                    User loggedUser;
                    if (DEBUG_MODE) //Don`t match users to database in debbug mode, only for test porpuses
                    {
                        loggedUser = u;
                        loggedUser.IsAdmin = true; //In debug mode all users are admins only for test
                    }
                    else
                    {
                        if (!sqLite.ValidateLogin(u)) //If user arent in database will get out of the thread poll. In debugg mode will accept all
                        {
                            Send(socket, new LoginStatus(false, "Wrong id or name"));
                            ServerLog.GetInstance().WriteToConnections(u, socket.RemoteEndPoint.ToString(), ConnectionAction.Wrong_Login);
                            return;
                        }
                        loggedUser = sqLite.FindUser(u.Id);
                    }
                    Send(socket, new LoginStatus(true, "Succes",loggedUser.IsAdmin));
                    Thread.CurrentThread.Name = u.Name; //Change the thread name for be more friendly to debug/test
                    agentsOnline.TryAdd(loggedUser, socket);
                    ServerLog.GetInstance().WriteToConnections(loggedUser, socket.RemoteEndPoint.ToString(),ConnectionAction.Login);

                    if (loggedUser.IsAdmin)
                        adminsOnline.TryAdd(loggedUser, socket);

                    SendUserUpdateToAdmin(loggedUser);

                }
            } while (!(received is User)); //If client dont send user, can't connect to server

            do
            {
                received = Receive(socket);
                if (received is null) //Socket close
                    break;

                if (received is Logout logout)//Client logout
                {
                    Logout(logout);
                    break;
                }

                if (received is Call call)
                {
                    SendCall(call);
                    continue;
                }

                if (received is EndCall endCall)
                {
                    SendEndCall(endCall);
                    continue;
                }

                if(received is AdminOrder aOrder)
                {
                    switch (aOrder.OrderType)
                    {
                        case OrderType.GetUserList:
                            SendUserList(aOrder.UserToSendResponse);
                            break;
                        case OrderType.SendMessage:
                            break;
                        case OrderType.DisconnectUser:
                            break;
                    }
                }

                if (received is Message message)
                {
                    if (message.userToMessage is null)
                        BroadCastMessageAllClients(message);
                    else
                        SendMessageToClient(message);
                    continue;
                }
            } while (true);

            //Close the resources and end the current thread.
            socket.Close();
        }



        /// <summary>
        /// Send a new connection update to all admins clients
        /// </summary>
        /// <param name="u">New Logged user</param>
        private void SendUserUpdateToAdmin(User u)
        {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            foreach (KeyValuePair<User, Socket> client in adminsOnline)
                Send(client.Value, u);
        }

        /// <summary>
        /// Method to update admin users control. Send one ClientLogout to update UI
        /// </summary>
        /// <param name="u">User disconnected</param>
        private void SendLogoutToAdmin(User u)
        {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            foreach (KeyValuePair<User, Socket> client in adminsOnline)
            {
                if (client.Key.IsAdmin && u != client.Key)
                    Send(client.Value, new ClienLogout(u));
            }
        }

        /// <summary>
        /// When admin open the tool for first time the method send one array with the actual online users
        /// </summary>
        /// <param name="u">Admin user logged</param>
        private void SendUserList(User u)
        {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            User[] users = new User[agentsOnline.Count];
            int counter = 0;
            foreach (var item in agentsOnline)
                users[counter++] = item.Key;

            Send(agentsOnline[u], users);
        }

        /// <summary>
        /// Send a object to all users connected
        /// </summary>
        /// <param name="message">object to send</param>
        public void BroadCastMessageAllClients(Message message)
        {
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
                Send(client.Value, message);
        }

        /// <summary>
        /// Send a message to the destinatary
        /// </summary>
        /// <param name="m">Message to send</param>
        private void SendMessageToClient(Message m)
        {
                Send(agentsOnline[m.userToMessage], m);
        }

        /// <summary>
        /// Send a call to the destinatary
        /// </summary>
        /// <param name="call">The incomming call</param>
        public void SendCall(Call call)
        {
            Send(agentsOnline[call.to], call);
            call.to.InCall = true;
            SendUserUpdateToAdmin(call.to);
        }

        /// <summary>
        /// Send the endcall to a client
        /// </summary>
        /// <param name="endCall">Te endcall with the user to send it</param>
        public void SendEndCall(EndCall endCall)
        {
            Send(agentsOnline[endCall.to], endCall);
            endCall.to.InCall = false;
            SendUserUpdateToAdmin(endCall.to);
        }

        /// <summary>
        /// Get client Logout signal and dispose the resources
        /// </summary>
        /// <param name="logout"></param>
        public void Logout(Logout logout)
        {
            Send(agentsOnline[logout.userToLogout], logout);
            agentsOnline.TryRemove(logout.userToLogout, out _);
            SendLogoutToAdmin(logout.userToLogout);
        }

        /// <summary>
        /// Send a object to the client
        /// </summary>
        /// <param name="s">Socket client</param>
        /// <param name="o">Object to send</param>
        private void Send(Socket s, object o)
        {
            byte[] buffer = new byte[BUFFERSIZE];
            byte[] obj = BinarySerialization.Serializate(o);
            Array.Copy(obj, buffer, obj.Length);
            try
            {
                s.Send(buffer);
            }
            catch (ObjectDisposedException e)
            {
                ServerLog.GetInstance().WriteToExceptions(e);
                onErrorDelegate(2);
            }
            catch (SocketException e)
            {
                ServerLog.GetInstance().WriteToExceptions(e);
            }
        }

        /// <summary>
        /// Receive all the serialized object
        /// </summary>
        /// <param name="s">Socket that receive the object</param>
        /// <returns>Object received from client</returns>
        private object Receive(Socket s)
        {
            byte[] buffer = new byte[BUFFERSIZE];
            try
            {
                s.Receive(buffer);
            }
            catch (SocketException) //If socket was closed, came here
            {
                foreach (var item in agentsOnline)
                    if (item.Value == s)
                    {
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
