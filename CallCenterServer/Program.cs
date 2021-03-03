using SharedNameSpace;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using CallCenterServer.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CallCenterServer {
    class Program {
        //startup configuration
        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private static readonly bool DEBUG_MODE = config["DEBUG_MODE"].ToLower() == "true";

        static readonly bool serverStopped = false;
        static bool exception = false;
        static void Main() {

            #region CheckIfConfigFileExist (and create example)
            if (!File.Exists(Path.GetFullPath(Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar + "config.json")) {
                Console.WriteLine("El fichero config no existe. Por favor añada el fichero config.json en la ruta: " + Path.GetFullPath(Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar + "config.json");
                Console.WriteLine("Pulse la tecla Enter para crear un fichero config o pulse una tecla para salir...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                if (consoleKeyInfo.Key == ConsoleKey.Enter) {
                    Console.WriteLine(Path.GetFullPath(Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar + "config.json");
                    File.AppendAllText(Path.GetFullPath(Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar + "config.json", Constants.CONFIG_FILE_DATA);

                }
                return;
            }
            #endregion

            StartServer();
            while (!serverStopped && !exception) {
                if (DEBUG_MODE)
                    Menu();
            }
            if (exception) {
                Console.WriteLine("El servidor se detuvo a causa de una excepción. Pulse una tecla para cerrar el programa....");
                Console.ReadKey();
            }
        }






        #region UI methods
        private static void Menu() {
            Console.CursorVisible = false;
            Console.Clear();
            ConsoleKeyInfo aux;
            Console.WriteLine("                Menu                 ");
            Console.WriteLine("=====================================");
            if (Server.GetInstance().IsServerRunning) {
                Console.WriteLine("1. Ver agentes conectados");
                Console.WriteLine("2. Enviar nueva llamada entrante");
                Console.WriteLine("3. Enviar señal de fin de llamada");
                Console.WriteLine("4. Enviar un mensaje a todos los agentes");
                Console.WriteLine("5. Desconectar agente");
                Console.WriteLine("6. Administrar base de datos");
                Console.WriteLine("7. Ver log en tiempo real del server");
            }
            Console.WriteLine("\n0. {0}", Server.GetInstance().IsServerRunning ? "Apagar el servidor" : "Encender el servidor");

            Console.WriteLine("\n\nSeleccione una opción");
            aux = Console.ReadKey(true);
            Console.Clear();
            Console.CursorVisible = true;
            switch (aux.Key) {
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
                case ConsoleKey.D6:
                    ManageDatabase();
                    break;
                case ConsoleKey.D7:
                    ShowServerLog();
                    break;
                case ConsoleKey.D0:
                    if (Server.GetInstance().IsServerRunning)
                        StopServer();
                    else
                        StartServer();
                    break;
            }
        }

        private static void ShowServerLog() {
            Console.CursorVisible = false;
            Console.Clear();
            Console.WriteLine("              SERVER LOG");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Server.GetInstance().SHOW_CONSOLE_LOG = true;
            do {

            } while (!Console.KeyAvailable);
            Server.GetInstance().SHOW_CONSOLE_LOG = false;
        }

        private static void ManageDatabase() {
            bool exit = false;
            do {
                Console.CursorVisible = false;
                Console.Clear();
                ConsoleKeyInfo aux;
                Console.WriteLine("       GESTIONAR BASE DE DATOS");
                Console.WriteLine("=====================================");
                Console.WriteLine("1. Listar agentes en la base de datos");
                Console.WriteLine("2. Buscar agente");
                Console.WriteLine("3. Añadir nuevo agente");
                Console.WriteLine("4. Modificar agente");
                Console.WriteLine("5. Eliminar agente");
                Console.WriteLine();
                Console.WriteLine("ESC. Volver al menú principal");
                Console.WriteLine("\nSeleccione una opción.");

                aux = Console.ReadKey(true);
                Console.CursorVisible = true;
                Console.Clear();
                switch (aux.Key) {
                    case ConsoleKey.D1:
                        ListDatabaseUsers();
                        break;
                    case ConsoleKey.D2:
                        FindDatabaseUserByID();
                        break;
                    case ConsoleKey.D3:
                        AddUserInDatabase();
                        break;
                    case ConsoleKey.D4:
                        ModifyUserInDatabase();
                        break;
                    case ConsoleKey.D5:
                        DeleteDatabaseUser();
                        break;
                    case ConsoleKey.D6:
                        ManageDatabase();
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                }
            } while (!exit);
        }

        private static void MakeTable(List<User> userList) {
            string linea = "+" + "".PadRight(63, '-') + "+";
            Console.WriteLine(linea);
            Console.WriteLine(string.Format("| ".PadRight(6) + " | " + "ID".PadRight(20) + " | " + "NAME".PadRight(20) + " | " + "IS ADMIN" + " |"));
            for (int i = 0; i < userList.Count; i++) {
                Console.WriteLine(linea);

                Console.WriteLine(string.Format("| " + "{0:000}", i + 1).PadRight(6) + " | " + userList[i].Name.PadRight(20) + " | " + userList[i].Id.PadRight(20) + " | " + userList[i].IsAdmin.ToString().PadRight(8) + " |");



            }
            Console.WriteLine(linea);
        }

        #endregion

        #region Clients Methods
        private static void ClientsOnline() {
            Console.Clear();
            Console.WriteLine("         Usuarios conectados");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            List<User> users = Server.GetInstance().ConnectecUsers();
            if (users is null)
                return;


            foreach (var item in users)
                Console.WriteLine(item.ToString());



            Console.WriteLine("\n\nFin de usuarios conectados. Pulse una tecla para volver al menu");
            Console.ReadKey();
        }

        private static void DisconnectClient() {
            Console.WriteLine("        DESCONECTAR CLIENTE");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Seleccione el cliente para desconectar: ");
            List<User> usersList = Server.GetInstance().ConnectecUsers();
            if (usersList is null)
                return;
            for (int i = 0; i < usersList.Count; i++)
                Console.WriteLine(string.Format("[{0:00}]: ", i + 1) + usersList[i].ToString()); //+ 1 Cause natural users count start 1

            Console.Write("\n\nCliente: ");
            int selectedUser;
            if (int.TryParse(Console.ReadLine(), out selectedUser)) {
                try {
                    selectedUser -= 1; //-1 To get the real array position
                    AdminOrder logout = new AdminOrder(OrderType.DisconnectUser, usersList[selectedUser]);
                    Server.GetInstance().Logout(logout);
                    Console.WriteLine("Desconectando el cliente : {0}", usersList[selectedUser].ToString());
                    Console.WriteLine("Cliente desconectado");
                    Console.ReadKey();
                } catch (ArgumentOutOfRangeException) {
                    WriteError(string.Format("El número que ha introducido no coincide con ninguno de la lista. VALOR [1-{0}]", usersList.Count() + 1));
                } catch (Exception e) {
                    WriteError(string.Format("Se produjo una excepción del tipo: {0}\n{1}", e.GetType(), e.Message));
                }

            } else {
                WriteError("No se introdujo un valor numerico, pulsa una tecla para volver al menú");
            }
        }
        #endregion

        #region Call Methods

        private static void SendEndCallToClient() {
            Console.WriteLine("     FINALIZAR LLAMADA A CLIENTE");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Seleccione el cliente de la lista: ");
            List<User> users = Server.GetInstance().ConnectecUsers();
            if (users is null)
                return;
            for (int i = 0; i < users.Count; i++) {
                Console.WriteLine(string.Format("[{0:00}]: ", i + 1) + users[i].ToString()); //+ 1 Cause natural users count start 1
            }
            Console.Write("\n\nCliente: ");
            int selectedUser;
            if (int.TryParse(Console.ReadLine(), out selectedUser)) {
                try {
                    selectedUser -= 1;//-1 To get the real array position

                    Console.WriteLine("Seleccionado el cliente : {0}", users[selectedUser].ToString());
                    Console.WriteLine("Enviando fin de llamada...");
                    EndCall endCall = new EndCall(users[selectedUser]);
                    Server.GetInstance().SendEndCall(endCall);
                    Console.WriteLine("Llamada en cliente terminada, pulsa una tecla para volver al menu");
                    Console.ReadKey();
                } catch (ArgumentOutOfRangeException) {
                    WriteError(string.Format("El número que ha introducido no coincide con ninguno de la lista. VALOR [1-{0}]", users.Count() + 1));
                } catch (Exception e) {
                    WriteError(string.Format("Se produjo una excepción del tipo: {0}\n{1}", e.GetType(), e.Message));
                }

            } else {
                WriteError("No se introdujo un valor numerico, pulsa una tecla para volver al menú");
            }
        }

        private static void SendCallToClient()
        {
            Console.WriteLine("       ENVIAR LLAMADA A CLIENTE");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Seleccione el cliente de la lista: ");
            List<User> usersList = Server.GetInstance().ConnectecUsers();
            if (usersList is null)
                return;
            for (int i = 0; i < usersList.Count; i++)
            {
                Console.WriteLine(string.Format("[{0:00}]: ", i + 1) + usersList[i].ToString()); //+ 1 Cause natural users count start 1
            }
            Console.Write("\n\nCliente: ");
            int selectedUser;
            if (int.TryParse(Console.ReadLine(), out selectedUser)) {
                selectedUser -= 1;//-1 To get the real array position
                try {
                    Console.WriteLine("Seleccionado el cliente : {0}", usersList[selectedUser].ToString());
                    Console.Write("\n\nIndica el número de teléfono: ");
                    string telf = Console.ReadLine();
                    Console.Write("\n\nIndica la campaña: ");
                    string campania = Console.ReadLine();
                    Call call = new Call(usersList[selectedUser], telf, campania, "callID", 1);
                    Server.GetInstance().SendCall(call);
                    Console.WriteLine("Llamada enviada, pulsa una tecla para volver al menu");
                } catch (ArgumentOutOfRangeException){
                    WriteError(string.Format("El número que ha introducido no coincide con ninguno de la lista. VALOR [1-{0}]", usersList.Count() + 1));
                } catch (Exception e){
                    WriteError(string.Format("Se produjo una excepción del tipo: {0}\n{1}", e.GetType(), e.Message));
                }
                
            } else {
                WriteError("No se introdujo un valor numerico, pulsa una tecla para volver al menú");
            }


            Console.ReadKey();
        }

        #endregion

        #region Messages methods
        private static void SendMessageToAll()
        {
            Console.WriteLine("       ENVIAR MENSAJE A TODOS");
            Console.WriteLine("=====================================");
            Console.WriteLine();
            Console.WriteLine("Escriba el mensaje: ");
            string msg = Console.ReadLine();
            Server.GetInstance().BroadCastMessageAllClients(new ServerResponse(ResponseContext.ServerMessage, msg));
            Console.WriteLine("Mensaje enviado, pulsa una tecla para volver al menu");
            Console.ReadKey();
        }
        #endregion

        #region Database Methods

        private static void ModifyUserInDatabase()
        {
            User u;
            Console.WriteLine("           MODIFICAR USUARIO");
            Console.WriteLine("=====================================");

            SqLiteOperator liteOperator = new SqLiteOperator();
            List<User> userList = liteOperator.GetAllUsers();
            MakeTable(userList);

            Console.Write("\nIntroduce el número del usuario: ");
            int userID = int.Parse(Console.ReadLine()) - 1; //To get real array position1
            try
            {
                u = liteOperator.FindUser(userList[userID].Id);
            }
            catch (Exception)
            {

                u = null;
            }


            if (!(u is null))
            {
                Console.Write("Introduce el nuevo nombre: ");
                u.Name = Console.ReadLine();
                Console.Write("¿Es admin? S/n");
                u.IsAdmin = Console.ReadKey().Key == ConsoleKey.S;
                liteOperator.UpdateUser(u);
                Console.WriteLine("\n¡Usuario actualizado con éxito!.");
            }
            else
                Console.WriteLine("No existe ese usuario en la base de datos.");

            Console.WriteLine("\nPulsa una tecla para volver...");
            Console.ReadKey(true);
        }

        private static void DeleteDatabaseUser()
        {
            Console.WriteLine("           ELIMINAR USUARIO");
            Console.WriteLine("=====================================");

            SqLiteOperator liteOperator = new SqLiteOperator();
            List<User> userList = liteOperator.GetAllUsers();
            MakeTable(userList);

            Console.Write("\nIntroduce el número del usuario: ");
            int userID = int.Parse(Console.ReadLine()) - 1; //To get real array position1

            User u = liteOperator.FindUser(userList[userID].Id);

            if (!(u is null))
            {

                liteOperator.DeleteUser(u);
                Console.WriteLine("\n¡Usuario eliminado con éxito!.");
            }
            else
                Console.WriteLine("No existe ese usuario en la base de datos.");

            Console.WriteLine("\nPulsa una tecla para volver...");
            Console.ReadKey(true);
        }

        private static void AddUserInDatabase()
        {
            Console.WriteLine("           AÑADIR USUARIO");
            Console.WriteLine("=====================================");

            Console.Write("\nIntroduce el id del usuario: ");
            string userID = Console.ReadLine();

            SqLiteOperator liteOperator = new SqLiteOperator();
            User u = liteOperator.FindUser(userID);

            if (u is null)
            {
                Console.Write("\nIntroduce el nombre del usuario: ");
                string userName = Console.ReadLine();
                Console.Write("\n¿Es admin? S/n");
                bool isAdmin = Console.ReadKey().Key == ConsoleKey.S;

                liteOperator.CreateNewUser(new User(userID, userName, isAdmin));
                Console.WriteLine("\n¡Usuario creado con éxito!.");
            }
            else
                Console.WriteLine("Ya existe un cliente con esa ID en la base de datos.");

            Console.WriteLine("\nPulsa una tecla para volver...");
            Console.ReadKey(true);
        }

        private static void FindDatabaseUserByID()
        {
            Console.WriteLine("           BUSCAR USUARIO");
            Console.WriteLine("=====================================");

            Console.Write("\nIntroduce el id a buscar:");
            string userID = Console.ReadLine();

            SqLiteOperator liteOperator = new SqLiteOperator();
            User u = liteOperator.FindUser(userID);

            if (u is null)
                Console.WriteLine("No existe ese usuario en la base de datos");
            else
                Console.WriteLine(u.ToString());

            Console.WriteLine("\nPulsa una tecla para volver...");
            Console.ReadKey(true);
        }

        private static void ListDatabaseUsers()
        {

            Console.WriteLine("  LISTAR USUARIOS EN BASE DE DATOS");
            Console.WriteLine("====================================");
            SqLiteOperator liteOperator = new SqLiteOperator();
            List<User> userList = liteOperator.GetAllUsers();
            MakeTable(userList);


            Console.WriteLine("\nFIN DEL LISTADO");
            Console.WriteLine("Pulse una tecla para volver atrás");
            Console.ReadKey();
        }
        #endregion

        #region Server Methods
        private static void StopServer()
        {
            Console.WriteLine("Apagando el servidor...");
            Server.GetInstance().StopServer();
            Console.WriteLine("Servidor apagado.");
        }

        private static void StartServer()
        {
            Console.WriteLine("Encendiendo el servidor...");
            Server.GetInstance().StartServer(GetErrors,WriteConsoleLog);
            if (!exception)
                Console.WriteLine("Servidor encendido y esperando clientes.");
        }

        #endregion


        private static void WriteConsoleLog(string message)
        {
            if(Server.GetInstance().SHOW_CONSOLE_LOG)
                Console.WriteLine(message);
        }

        #region Erros method
        private static void GetErrors(int error)
        {
            exception = true;
            string auxError = "[EXCEPCION]: ";

            switch (error)
            {
                case 1: //El cliente no está disponible al enviarle algo
                    auxError += "El cliente está desconectado no se pudo enviar nada.";
                    exception = false; //Dont should stop the program
                    break;
                case 2: //El socket esta desechado
                    auxError += "El socket al que se intentó enviar algo está eliminado (.Dispose()).";
                    exception = false; //Dont should stop the program
                    break;

                // config file errors
                case 100: //La ip que se dió en el fichero no es válida
                    auxError += "La dirección IP del servidor del fichero config fue inválida";
                    break;

                case 101: //El puerto que se dió en el fichero no es válido
                    auxError += "El puerto del servidor del fichero config fue inválido";
                    break;


                //Clients errors
                case 200: //La clave por la que se buscó no se encuentra en el diccionario
                    auxError += "No existe esa clave en el diccionario.";
                    exception = false; //Dont should stop the program
                    break;
                default:
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(auxError);
            Console.ResetColor();
        }

        #endregion

        private static void WriteError(string errorMessage) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }
    }
}
