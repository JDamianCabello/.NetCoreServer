# Welcome to .NetCoreServer!

.NetCoreServer is a **dotnet** server solution to use where you want. Server have config.json file to specific some parameters and 2 clients to use the server as [normal dummy client](#) and as [admin client](#).

How to start: 


# Config file ([config.json](https://github.com/JDamianCabello/.NetCoreServer/blob/master/CallCenterServer/config.json.example))

The server need a minimum of configurations to start. All configurations are includes this file.

The following lines shows the configurations file options:

    {
      "SERVER_IP": "YOUR SERVER IP HERE. DEFAULT localhost",
      "SERVER_PORT": "YOUR SERVER PORT HERE. DEFAULT 4044",
      "DEBUG_MODE": "SHOW OR HIDE THE SERVER MENU {true: false}"
      "PATH_LOG_FOLDER": "PATH TO YOUR LOGS FILES DIRECTORY ( DEFAULT SERVER folder/serverlogs)"
    }

In the configurations if want default just set the value to empty valor.

    {
      "SERVER_IP": "",
      "SERVER_PORT": "",
      "DEBUG_MODE": "true"
      "PATH_LOG_FOLDER": ""
    }
This is the minimum configuration to server (To work in localhost).

If some options are wrong, server will thrown a exception.

## Server models

The `Server.cs` class need some models to work. All models are stored in Models folder. If want some extra function or include a new model just put here and call in in the `ListenClient(object o)` method from `Server.cs`.

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
**All models you makes must be in the same namespace (SharedNameSpace) and include the flag [Serializable]** Example below.

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

To update the migration go to `/Migrations/20200911090205_InitialCreate.cs` and write your tables in the Up method.

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

The SqLiteOperator is a [Singleton](https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/). 

> Should be called SqLiteOperator.GetInstance().method();


# Utils

## Serialization class

Server communications are based on serialized objects. For this purpose in `/Utils/Serialization.cs` have a Serialization class. The class should serialize/deserializate all send/received objects.

When u deserializate a received item need be in the same executing assembly. If don't, this class will throw a SerializationError. This class make the same executing assembly:

    public class CurrentAssemblyDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(string.Format("{0}, {1}", typeName, Assembly.GetExecutingAssembly().FullName));
        }
    }

And in the `public static object Deserializate(byte[] data)` method when create the [BinaryFormatter](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.formatters.binary.binaryformatter?view=netcore-3.1), set the [Binder](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.formatters.binary.binaryformatter.binder?view=netcore-3.1) like this:

    BinaryFormatter formatter = new BinaryFormatter
    {
        Binder = new CurrentAssemblyDeserializationBinder() 
    };




# Server.cs
This class implements the [Singleton](https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/) pattern.

The `Server.cs` use a external configuration, for read this settings use the interface [IConfiguration](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfiguration?view=dotnet-plat-ext-3.1) and [IConfigurationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfigurationbuilder?view=dotnet-plat-ext-3.1). This 2 dependencies are includes in nuget package `Microsoft.Extensions.Configuration` . Link [here](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/).

As the configuration is in json format we need install other nuget package : `Microsoft.Extensions.Configuration.Json` for extend the class `ConfigurationBuilder` : `AddJsonFile` and `AddJsonStream`. Link [here](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/).

From this configuration file this class use the key/value collections: SERVER_IP, SERVER_PORT and DEBUG_MODE.
