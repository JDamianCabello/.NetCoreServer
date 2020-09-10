using System;
using System.Collections.Generic;
using System.Text;

namespace CallCenterServer.Utils
{
    public static class Constants
    {
        public const string CONFIG_FILE_DATA = @"
                {\n
                    ""SERVER_IP"": ""YOUR SERVER IP HERE DEFAULT IS Localhost"",\n
                    ""SERVER_PORT"": ""YOUR SERVER PORT HERE DEFAULT IS 4044"",n
                    ""DEBBUG_MODE"": ""SHOW OR HIDE THE SERVER MENU {TRUE : FALSE }"",\n
                    ""ONLY_ADMIT_ADMIN"": ""DETERMINES IF ONLY ADMINS ARE ALLOWED IN THIS SESSION"",\n
                    ""MESSAGE_TO_CLIENTS_IN_ONLY_ADMINS_MODE"": ""DEFAULT MESSAGE WHEN CLIENT TRY CONNECT TO SERVER IN ONLY ADMIN MODE"",\n,
                    ""PATH_LOG_FILE"": ""PATCH TO YOUR LOGS FILES DIRECTORY ( DEFAULT IS SERVER FOLDER/Serverlogs)""\n
                }";
    }
}
