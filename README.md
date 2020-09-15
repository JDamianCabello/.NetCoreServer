
# Welcome to .NetCoreServer!

.NetCoreServer is a **dotnet** server solution to use where you want. Server have config.json file to specific some parameters and 2 clients to use the server a [normal dummy client](#) and a [admin client](#).

How to start: 


# Config file ([config.json](#))

The server need a minimum of config to start run. All config are in this file and server will do all about this.

The following lines show the config file options:

    {
      "SERVER_IP": "YOUR SERVER IP HERE DEFAULT IS Localhost",
      "SERVER_PORT": "YOUR SERVER PORT HERE DEFAULT IS 4044",
      "DEBBUG_MODE": "SHOW OR HIDE THE SERVER MENU {TRUE : FALSE}"
      "PATH_LOG_FOLDER": "PATCH TO YOUR LOGS FILES DIRECTORY ( DEFAULT IS SERVER FOLDER/Serverlogs)"
    }

In the options if want default just set the value to empty valor.

    {
          "SERVER_IP": "",
          "SERVER_PORT": "",
          "DEBBUG_MODE": "true"
          "PATH_LOG_FOLDER": ""
    }
This are the minimum config to server (To work in localhost).

if some option are bad, server will thrown a exception.

## Server models

The Server.cs class need some models to work all modes are stored in Models folder. If want some extra function or include a new model just put here and call in in the ListenClient(object o) method from Server.cs (line 203).

Example if u wanna receive a option class make your model and in the second do while include it like this: 


        private void ListenClient(object o)
        {
            Socket socket = (Socket)o;
            object received;
            do
            {
                //Wait for a user to client and start socket and threads
            }
        } while (!(received is User));
     
        do
        {
            received = Receive(socket);
            //Other object type stuff 
            
            if (received is Option option)
            {
                //Do Option stuff that u need
                continue;
            }
        } while (true);
        socket.Close();
     }
**All models that u makes need be in the same namespace and include the flag [Serializable]** Example below.

    using System;
    
    namespace SharedNameSpace
    {
    
        /// <summary>
        /// Example Model to option
        /// </summary>
        [Serializable]
        class Option
        {
            public var someVariableThatUNeed;
        
            public Option(Var someVariableThatUNeed)
            {
                this.someVariableThatUNeed= someVariableThatUNeed;
            }
        }
    
    }

## Database
When server aren't in debug mode will check if user exist in the database.

To manage database put server in debug mode (Remember config file) and use the Manage database option in menu.

           GESTIONAR BASE DE DATOS
    =====================================
    1. Listar agentes en la base de datos
    2. Buscar agente
    3. Añadir nuevo agente
    4. Modificar agente
    5. Eliminar agente
    
    ESC. Volver al menú principal
    
    Seleccione una opción.



Translation: 
1. List all users
2. Find user
3. Add user
4. Modify user
5. Delete user

The list is in MariaDB style.
