using SharedNameSpace;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using CallCenterServer.Utils;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace CallCenterServer
{
    class Program
    {
        static string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "config.json";
        //startup configuration
        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private static readonly bool DEBBUG_MODE = config["DEBBUG_MODE"].ToLower() == "true" ? true : false;
        static void Main()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + "/config.json"))
            {
                Console.WriteLine("El fichero config no existe. Por favor añada el fichero config.json en la ruta: "+ Path.GetFullPath(Directory.GetCurrentDirectory() + "/config.json"));
                Console.WriteLine("Pulse la tecla Enter para crear un fichero config o pulse una tecla para salir...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                if(consoleKeyInfo.Key == ConsoleKey.Enter)
                {

                    File.CreateText(filePath);
                    Console.WriteLine("Config file created!.");
                    using (var tw = new StreamWriter(filePath,true))
                    {
                        tw.Write(Constants.CONFIG_FILE_DATA);
                    }
                    Console.WriteLine("Setting example options.");


                }
                return;
            }
            StartServer();
            while (true)
            {
                if(DEBBUG_MODE)
                    Menu();
            }
        }


        private static void ClientsOnline()
        {
            Console.Clear();
            Console.WriteLine("         Usuarios conectados");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            List<User> users = Server.GetInstance().ConnectecUsers();
            foreach (var item in users)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("\n\nFin de usuarios conectados. Pulse una tecla para volver al menu");
            Console.ReadKey();
        }

        private static void Menu()
        {
            Console.Clear();
            ConsoleKeyInfo aux;
            Console.WriteLine("                Menu                 ");
            Console.WriteLine("=====================================");
            Console.WriteLine("1. Ver agentes conectados");
            Console.WriteLine("2. Enviar nueva llamada entrante");
            Console.WriteLine("3. Enviar señal de fin de llamada");
            Console.WriteLine("4. Enviar un mensaje a todos los agentes");
            Console.WriteLine("5. Desconectar agente");
            Console.WriteLine("\n0. {0}", Server.GetInstance().IsServerRunning ? "Apagar el servidor":"Encender el servidor");

            Console.Write("Seleccione una opción: ");
            aux = Console.ReadKey();
            Console.Clear();
            switch (aux.Key)
            {
                case ConsoleKey.D1:
                    ClientsOnline();
                    break;
                case ConsoleKey.D2:
                    SendCallToClient();
                    break;
                case ConsoleKey.D3:
                    SendEndCallToClient();
                    break;
                case ConsoleKey.D4:
                    SendMessageToAll();
                    break;
                case ConsoleKey.D5:
                    DisconnectClient();
                    break;
                case ConsoleKey.D0:
                    if (Server.GetInstance().IsServerRunning)
                        StopServer();
                    else
                        StartServer();
                    break;
            }
        }

        private static void StopServer()
        {
            Console.WriteLine("Apagando el servidor...");
            Server.GetInstance().StopServer();
            Console.WriteLine("Servidor apagado.");
        }

        private static void StartServer()
        {
            Console.WriteLine("Encendiendo el servidor...");
            Server.GetInstance().StartServer(GetErrors);
            Console.WriteLine("Servidor encendido y esperando clientes.");
        }

        private static void GetErrors(int error)
        {
            string auxError = string.Empty;

            switch (error)
            {
                case 1: //El cliente no está disponible al enviarle algo
                    auxError = "El cliente está desconectado no se pudo enviar nada.";
                    break;
                default:
                    break;
            }

            Console.WriteLine(auxError);
        }

        private static void DisconnectClient()
        {
            Console.WriteLine("        DESCONECTAR CLIENTE");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Seleccione el cliente para desconectar: ");
            List<User> usersList = Server.GetInstance().ConnectecUsers();
            for (int i = 0; i < usersList.Count; i++)
            {
                Console.WriteLine(string.Format("[{0:00}]: ", i + 1) + usersList[i].ToString()); //+ 1 Cause natural users count start 1
            }
            Console.Write("\n\nCliente: ");
            int selectedUser = int.Parse(Console.ReadLine()) - 1; //-1 To get the real array position
            ClienLogout logout = new ClienLogout(usersList[selectedUser - 1]);
            Server.GetInstance().ClientLogout(logout);
            Console.WriteLine("Desconectando el cliente : {0}", usersList[selectedUser - 1].ToString());
            Console.WriteLine("Cliente desconectado");
            Console.ReadKey();
        }

        private static void SendMessageToAll()
        {
            Console.WriteLine("       ENVIAR MENSAJE A TODOS");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Escriba el mensaje: ");
            string msg = Console.ReadLine();
            Server.GetInstance().BroadCastMessageAllClients(new Message(msg));
            Console.WriteLine("Mensaje enviado, pulsa una tecla para volver al menu");
            Console.ReadKey();
        }

        private static void SendEndCallToClient()
        {
            Console.WriteLine("     FINALIZAR LLAMADA A CLIENTE");
            Console.WriteLine("=====================================");
            Console.WriteLine();
             Console.WriteLine("Seleccione el cliente de la lista: ");
            List<User> users = Server.GetInstance().ConnectecUsers();
            for (int i = 0; i < users.Count; i++)
            {
                Console.WriteLine(string.Format("[{0:00}]: ", i + 1) + users[i].ToString()); //+ 1 Cause natural users count start 1
            }
            Console.Write("\n\nCliente: ");
            int selectedUser = int.Parse(Console.ReadLine()) -1;//-1 To get the real array position

            Console.WriteLine("Seleccionado el cliente : {0}", users[selectedUser].ToString());
            Console.WriteLine("Enviando fin de llamada...");
            EndCall endCall = new EndCall(users[selectedUser]);
            Server.GetInstance().SendEndCall(endCall);
            Console.WriteLine("Llamada en cliente terminada, pulsa una tecla para volver al menu");
            Console.ReadKey();
        }

        private static void SendCallToClient()
        {
            Console.WriteLine("       ENVIAR LLAMADA A CLIENTE");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Seleccione el cliente de la lista: ");
            List<User> usersList = Server.GetInstance().ConnectecUsers();
            for (int i = 0; i < usersList.Count; i++)
            {
                Console.WriteLine(string.Format("[{0:00}]: ", i + 1) + usersList[i].ToString()); //+ 1 Cause natural users count start 1
            }
            Console.Write("\n\nCliente: ");
            int selectedUser = int.Parse(Console.ReadLine()) - 1;//-1 To get the real array position

            Console.WriteLine("Seleccionado el cliente : {0}", usersList[selectedUser].ToString());
            Console.Write("\n\nIndica el número de teléfono: ");
            string telf = Console.ReadLine();
            Console.Write("\n\nIndica la campaña: ");
            string campania = Console.ReadLine();
            Call call = new Call(usersList[selectedUser], telf, campania);
            Server.GetInstance().SendCall(call);
            Console.WriteLine("Llamada enviada, pulsa una tecla para volver al menu");
            Console.ReadKey();
        }
    }
}
