using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using SharedNameSpace;
using Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace CallCenterServer
{
    class Server
    {
        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private string SERVER_IP = string.IsNullOrWhiteSpace(config["SERVER_IP"]) ? "127.0.0.1" : config["SERVER_IP"];
        private readonly int SERVER_PORT = string.IsNullOrWhiteSpace(config["SERVER_PORT"].ToString()) ? 4404 : int.Parse(config["SERVER_PORT"]);
        private readonly bool ONLY_ADMIT_ADMIN = config["ONLY_ADMIT_ADMIN"].ToLower() == "true" ? true : false;
        private readonly string MESSAGE_TO_CLIENTS_IN_ONLY_ADMINS_MODE = config["MESSAGE_TO_CLIENTS_IN_ONLY_ADMINS_MODE"];
        private readonly string PATH_LOG_FILE = config["PATH_LOG_FILE"];

        static Socket socket;
        static Thread listenThread;
        static ConcurrentDictionary<User, Socket> agentsOnline; //thread-safe collection class to store key/value pairs.
        static ConcurrentDictionary<User, Socket> adminsOnline; //thread-safe collection class to store key/value pairs.
        public bool IsServerRunning;

        //Time in minutes to timeout the socket when dont do nothing
        private const int TIMEOUT = 20;

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
        /// <param name="errorDelegate">Method that will handle the errors in other side</param>
        public void StartServer(ErrorDelegate errorDelegate)
        {
            try
            {
                onErrorDelegate = errorDelegate;

                IPAddress addr = IPAddress.Parse(SERVER_IP);
                IPEndPoint endPoint = new IPEndPoint(addr, SERVER_PORT);

                socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(1000);

                listenThread = new Thread(Listen);
                listenThread.Priority = ThreadPriority.Highest;
                listenThread.Start();
                agentsOnline = new ConcurrentDictionary<User, Socket>();
                adminsOnline = new ConcurrentDictionary<User, Socket>();
                IsServerRunning = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
            }
        }
        public void StopServer()
        {
            listenThread.Interrupt(); 
            BroadCastMessageAllClients(new Message("Servidor apagandose, desconectando a todos los agentes."));
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
            {
                ClientLogout(new ClienLogout(client.Key));
            }
            Parallel.ForEach(agentsOnline, u =>
            {
                u.Value.Dispose();
                u.Value.Close();
            });
            socket.Dispose();
            socket.Close();
            IsServerRunning = false;
        }

        /// <summary>
        /// Debbug method that show online users in server
        /// </summary>
        /// <returns>A users online list</returns>
        public List<User> ConnectecUsers()
        {
            if (agentsOnline.IsEmpty)
                return null;
            List<User> userAux = new List<User>();
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
            {
                userAux.Add(client.Key);
            }
            return userAux;
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
                    //client.ReceiveTimeout = TIMEOUT * 60 * 1000; //intValueInMins * 60 secs * 1000 ms/sec
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
                    Thread.CurrentThread.Name = u.Name; //Change this later
                    agentsOnline.TryAdd((User)received, socket);

                    if (u.IsAdmin)
                    {
                        adminsOnline.TryAdd((User)received, socket);
                        SendUserList(u);
                    }
                    else
                        SendUserUpdateToAdmin(u);

                }
            } while (!(received is User));

            do
            {
                received = Receive(socket);
                if (received is Logout logout)
                {
                    Logout(logout);
                    break;
                }

                if (received is Call call)
                {
                    SendCall(call);
                }

                if (received is EndCall endCall)
                {
                    SendEndCall(endCall);
                }

                if (received is ClienLogout clienLogout)
                {
                    ClientLogout(clienLogout);
                }

                if (received is Message message)
                {
                    if (message.userToMessage is null)
                        BroadCastMessageAllClients(message);
                    else
                        SendMessageToClient(message);
                }
            } while (true);

            socket.Dispose();
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
            {
                if (client.Key.IsAdmin && u != client.Key)
                    Send(client.Value, u);
            }
        }

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


        private void SendUserList(User u)
        {
            if (adminsOnline.IsEmpty) //No admin to send data
                return;
            if (agentsOnline.Count == 1) //only admin user was online (one user)
                return;

            User[] userAux = new User[agentsOnline.Count - 1]; //Send all user less the admin that receive the list
            int count = 0;
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
            {
                if (u == client.Key)
                    continue;
                userAux[count++] = client.Key;
            }

            Send(agentsOnline[u], userAux);
        }

        /// <summary>
        /// Send a object to all users connected
        /// </summary>
        /// <param name="o">object to send</param>
        public void BroadCastMessageAllClients(Message message)
        {
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
            {
                Send(client.Value, message);
            }
        }

        /// <summary>
        /// Send a message to the destinatary
        /// </summary>
        /// <param name="m">Message to send</param>
        private void SendMessageToClient(Message m)
        {
            User tmpUser = FindUser(m.userToMessage);
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
            {
                if (client.Key == tmpUser)
                    Send(client.Value, m);
            }
        }

        /// <summary>
        /// Send a call to the destinatary
        /// </summary>
        /// <param name="call">The incomming call</param>
        public void SendCall(Call call)
        {
            User userAux = FindUser(call.to);
            Send(agentsOnline[userAux], call);
            userAux.InCall = true;
            SendUserUpdateToAdmin(userAux);
        }

        /// <summary>
        /// Send the endcall to a client
        /// </summary>
        /// <param name="endCall">Te endcall with the user to send it</param>
        public void SendEndCall(EndCall endCall)
        {
            User userAux = FindUser(endCall.to);
            Send(agentsOnline[userAux], endCall);
            userAux.InCall = false;
            SendUserUpdateToAdmin(userAux);
        }

        /// <summary>
        /// Get client Logout signal and dispose the resources
        /// </summary>
        /// <param name="logout"></param>
        public void Logout(Logout logout)
        {
            User user = FindUser(logout.userToLogout);
            agentsOnline.TryRemove(user, out Socket socket);
            SendLogoutToAdmin(user);
        }

        /// <summary>
        /// Method that receive a admin order to logout some client
        /// </summary>
        /// <param name="logout">Client who admin send a logout signal</param>
        public void ClientLogout(ClienLogout logout)
        {
            User user = FindUser(logout.clientToLogout);
            Send(agentsOnline[user], new Logout(user));
        }

        /// <summary>
        /// Find a received user in agentsOnline
        /// </summary>
        /// <param name="u">User to find</param>
        /// <returns></returns>
        private User FindUser(User u)
        {
            foreach (KeyValuePair<User, Socket> client in agentsOnline)
            {
                if (client.Key.Id == u.Id)
                    return client.Key;
            }
            return null;
        }

        /// <summary>
        /// Send a object to the client
        /// </summary>
        /// <param name="s">Socket client</param>
        /// <param name="o">Object to send</param>
        private void Send(Socket s, object o)
        {
            byte[] buffer = new byte[1024];
            byte[] obj = BinarySerialization.Serializate(o);
            Array.Copy(obj, buffer, obj.Length);
            try
            {
                s.Send(buffer); 
            }
            catch (Exception)
            {
            }
            
        }

        /// <summary>
        /// Receive all the serialized object
        /// </summary>
        /// <param name="s">Socket that receive the object</param>
        /// <returns>Object received from client</returns>
        private object Receive(Socket s)
        {
            byte[] buffer = new byte[1024];
            try
            {
                    s.Receive(buffer);
            }

            catch
            {
            }

            return BinarySerialization.Deserializate(buffer);
        }


    }
}
