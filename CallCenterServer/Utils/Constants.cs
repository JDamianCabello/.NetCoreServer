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
        /// if dont exist server will ask to make it and will take from the template
        /// </summary>
        public const string CONFIG_FILE_DATA = @"{
    ""SERVER_IP"": ""YOUR SERVER IP HERE DEFAULT IS Localhost"",
    ""SERVER_PORT"": ""YOUR SERVER PORT HERE DEFAULT IS 4044"",
    ""DEBUG_MODE"": ""SHOW OR HIDE THE SERVER MENU {TRUE : FALSE }"",
    ""PATH_LOG_FOLDER"": ""PATH TO YOUR LOGS FILES DIRECTORY ( DEFAULT IS SERVER FOLDER/Serverlogs)""
}";
    }
}
