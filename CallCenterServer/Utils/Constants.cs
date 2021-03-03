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
    ""LOCALHOST"": ""DETERMINE IF SERVER WORK IN LOCALHOST OR GET THE CURRENT MACHINNE IP"",
    ""SERVER_PORT"": ""YOUR SERVER PORT HERE DEFAULT IS 4044"",
    ""DEBUG_MODE"": ""SHOW OR HIDE THE SERVER MENU {TRUE : FALSE }"",
    ""CONSOLE_LOG"": ""DETERMINES IF SHOULD SHOW LOG WHEN RUNNING (DONT WORK WITH DEBUG_MODE)"",
    ""CONSOLE_LOG_SAVE_FILE"": ""SAVE NORMAL LOG TO .LOG FILE"",
    ""PATH_LOG_FOLDER"": ""PATH TO YOUR LOGS FILES DIRECTORY ( DEFAULT IS SERVER FOLDER/Serverlogs)""
}";
    }
}
