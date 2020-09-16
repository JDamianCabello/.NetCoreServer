# Welcome to .NetCoreServer!

.NetCoreServer is a **dotnet** server solution to use where you want. Server have config.json file to specific some parameters and 2 clients to use the server as [normal dummy client](#) and as [admin client](#).

How to start: 


# Config file ([config.json](#))

The server need a minimum of configurations to start. All configurations are includes this file.

The following lines shows the configurations file options:

    {
      "SERVER_IP": "YOUR SERVER IP HERE. DEFAULT localhost",
      "SERVER_PORT": "YOUR SERVER PORT HERE. DEFAULT 4044",
      "DEBBUG_MODE": "SHOW OR HIDE THE SERVER MENU {true: false}"
      "PATH_LOG_FOLDER": "PATH TO YOUR LOGS FILES DIRECTORY ( DEFAULT SERVER folder/serverlogs)"
    }

In the configurations if want default just set the value to empty valor.

    {
      "SERVER_IP": "",
      "SERVER_PORT": "",
      "DEBBUG_MODE": "true"
      "PATH_LOG_FOLDER": ""
    }
This is the minimum configuration to server (To work in localhost).

If some options are wrong, server will thrown a exception.

## Server models

The Server.cs class need some models to work. All models are stored in Models folder. If want some extra function or include a new model just put here and call in in the ListenClient(object o) method from Server.cs (line 203).

Example if you want to receive a option class make your model and at the second do while include it like this: 


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
**All models you makes must be in the same namespace and include the flag [Serializable]** Example below.

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
To build the database can make a new SQLite database with this structure: 

    -- TABLE
    CREATE TABLE "Users" (
        "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
        "Name" TEXT NOT NULL,
        "IsAdmin" INTEGER NOT NULL
    );
    CREATE TABLE "__EFMigrationsHistory" (
        "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
        "ProductVersion" TEXT NOT NULL
    );

Or use the Migration class. In Visual Studio open the console and run the following command: 
> update-Database

For delete the migration use: 
> remove-migration

To update the migration go to /Migrations/20200911090205_InitialCreate.cs and write your tables in the Up method.

We have a SqLiteOperator in the server (Server/Database/SqLiteOperator.cs) to manage SQLite databases. U can do all u want with databases here, I just include a Users CRUD. If need more info about program new functionalitys check [this link](https://www.learnentityframeworkcore.com/).

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

    +-------------------------------------------+
    |      | ID        | NAME        | IS ADMIN |
    +-------------------------------------------+
    | 001  | ADMIN     | ADMIN       | True     |
    +-------------------------------------------+
    | 002  | admin     | admin       | True     |
    +-------------------------------------------+
    | 003  | asd       | asd         | False    |
    +-------------------------------------------+
    | 004  | noadmin   | noadmin    | False     |
    +-------------------------------------------+
    | 005  | test      | test       | True      |
    +-------------------------------------------+


