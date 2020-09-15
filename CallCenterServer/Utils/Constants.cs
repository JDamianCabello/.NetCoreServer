using System;
using System.Collections.Generic;
using System.Text;

namespace CallCenterServer.Utils
{
    /// <summary>
    /// All app constants here
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Config.json.example 
        /// 
        /// if dont exist server sill ask to make for the user so will take from here the template
        /// </summary>
        public const string CONFIG_FILE_DATA = @"{
    ""SERVER_IP"": ""YOUR SERVER IP HERE DEFAULT IS Localhost"",
    ""SERVER_PORT"": ""YOUR SERVER PORT HERE DEFAULT IS 4044"",
    ""DEBBUG_MODE"": ""SHOW OR HIDE THE SERVER MENU {TRUE : FALSE }"",
    ""PATH_LOG_FOLDER"": ""PATCH TO YOUR LOGS FILES DIRECTORY ( DEFAULT IS SERVER FOLDER/Serverlogs)""
}";
    }
}
