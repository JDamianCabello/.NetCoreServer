using Microsoft.Extensions.Configuration;
using SharedNameSpace;
using System;
using System.IO;
using System.Net.Sockets;

namespace CallCenterServer
{
    public enum ConnectionAction { Login, Wrong_Login, Disconnect};
    class ServerLog
    {
        //Singleton pattern
        private static ServerLog serverLog;
        private ServerLog() {
            syncWriteExceptionLock = new object();
            syncWriteConnectionLock = new object();
            syncWriteConsoleLock = new object();
        }

        public static ServerLog GetInstance()
        {
            if (serverLog is null)
                serverLog = new ServerLog();

            return serverLog;
        }

        //Object to stop all thread try write in the logs files. If object is locked thread will wait to write
        private readonly object syncWriteExceptionLock;
        private readonly object syncWriteConnectionLock;
        private readonly object syncWriteConsoleLock;

        //Config file reader
        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private static readonly string PATH_LOG_FOLDER = config["PATH_LOG_FOLDER"];

        //Based path to put logs files
        private static readonly string PATH_CONNECTIONLOG_FOLDER = PATH_LOG_FOLDER + Path.DirectorySeparatorChar + "ConnectionLogs";
        private static readonly string PATH_EXCEPTIONLOG_FOLDER = PATH_LOG_FOLDER + Path.DirectorySeparatorChar + "ExceptionsLogs";
        private static readonly string PATH_CONSOLELOGS_FOLDER = PATH_LOG_FOLDER + Path.DirectorySeparatorChar + "RunningLogs";

        /// <summary>
        /// Write all uses connections to Connection log file
        /// </summary>
        /// <param name="u">Current logged user</param>
        /// <param name="userIp">User IP</param>
        /// <param name="action">User action</param>
        public void WriteToConnections(User u, string userIp, ConnectionAction action)
        {
            CheckIfExistConnectionsLogsFolder();
            lock (syncWriteConnectionLock)
            {
                File.AppendAllText(PATH_CONNECTIONLOG_FOLDER + Path.DirectorySeparatorChar + "ConnectionsLog_" + DateTime.Now.ToString("yyyyMMdd") + ".log", FormatConnection(u, userIp, action));
            }
        }

        /// <summary>
        /// Write all console output to log
        /// </summary>
        /// <param name="message">Log message</param> to save
        public void WriteToConsole(string message)
        {
            CheckIfExistConsoleLogsFolder();
            lock (syncWriteConsoleLock)
            {
                File.AppendAllText(PATH_CONSOLELOGS_FOLDER + Path.DirectorySeparatorChar + "ConsoleLog_" + DateTime.Now.ToString("yyyyMMdd") + ".log", message+"\n");
            }
        }

        /// <summary>
        /// Write all exceptions to Exception log file
        /// </summary>
        /// <param name="e">Exception</param>
        public void WriteToExceptions(Exception e)
        {
            CheckIfExistExceptionsLogFolder();
            lock(syncWriteExceptionLock){
                File.AppendAllText(PATH_EXCEPTIONLOG_FOLDER + Path.DirectorySeparatorChar + "ExceptionsLog_" + DateTime.Now.ToString("yyyyMMdd") + ".log", FormatException(e));
            }
        }
        /// <summary>
        /// Makes the conection log folder. IF log folder dont exist will create it too.
        /// </summary>
        private void CheckIfExistConnectionsLogsFolder()
        {
            CheckIfExistLogFolder();
            if (!Directory.Exists(PATH_CONNECTIONLOG_FOLDER))
                Directory.CreateDirectory(PATH_CONNECTIONLOG_FOLDER);
        }

        /// <summary>
        /// Makes the console log folder. IF log folder dont exist will create it too.
        /// </summary>
        private void CheckIfExistConsoleLogsFolder()
        {
            CheckIfExistLogFolder();
            if (!Directory.Exists(PATH_CONSOLELOGS_FOLDER))
                Directory.CreateDirectory(PATH_CONSOLELOGS_FOLDER);
        }

        /// <summary>
        /// IF log folder dont exist will create it.
        /// </summary>
        private void CheckIfExistLogFolder()
        {
            if (!Directory.Exists(PATH_LOG_FOLDER))
                Directory.CreateDirectory(PATH_LOG_FOLDER);
        }

        /// <summary>
        /// Makes the exception log folder. IF log folder dont exist will create it too.
        /// </summary>
        private void CheckIfExistExceptionsLogFolder()
        {
            if (!Directory.Exists(PATH_EXCEPTIONLOG_FOLDER))
                Directory.CreateDirectory(PATH_EXCEPTIONLOG_FOLDER);
        }

        /// <summary>
        /// Format the exception info for write into log file
        /// 
        /// Output example:
        /// 
        /// 2020-09-14 11:25:42-1305
        /// TYPE: :System.FormatException
        /// STACK TRACE:    at System.Net.IPAddressParser.Parse(ReadOnlySpan`1 ipSpan, Boolean tryParse)
        ///  at System.Net.IPAddress.Parse(String ipString)
        ///  at CallCenterServer.Server.StartServer(ErrorDelegate errorDelegate) in C:\Users\PTV\source\repos\CallCenterServer\CallCenterServer\Server\Server.cs:line 63
        ///SOURCE: System.Net.Primitives
        ///TARGET SITE: System.Net.IPAddress Parse(System.ReadOnlySpan`1[System.Char], Boolean)
        ///INNER EXCEPTION: System.Net.Sockets.SocketException(10022): Se ha proporcionado un argumento no válido.
        ///MESSAGE: An invalid IP address was specified.
        ///ADDITIONAL DATA: System.Collections.ListDictionaryInternal
        ///=======================================================================================================
        /// </summary>
        /// <param name="exception">The exception info</param>
        /// <returns>Formated string</returns>
        private string FormatException(Exception exception)
        {
            string aux = string.Empty;
            const string newLine = "\n";
            const string separator = newLine + "=======================================================================================================" + newLine;

            aux += DateTime.Now.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'-'FFFF") + newLine;

            aux += "TYPE: :";
            aux += exception.GetType() + newLine;

            aux += "STACK TRACE: ";
            aux += exception.StackTrace + newLine;

            aux += "SOURCE: ";
            aux += exception.Source + newLine;

            aux += "TARGET SITE: ";
            aux += exception.TargetSite + newLine;

            aux += "INNER EXCEPTION: ";
            aux += exception.InnerException + newLine;

            aux += "MESSAGE: ";
            aux += exception.Message + newLine;

            aux += "ADDITIONAL DATA: ";
            aux += exception.Data + newLine;

            aux += separator;

            return aux;
        }
        /// <summary>
        /// Format the user info for write into log file
        /// 
        /// Output examples:
        /// 2020-09-15 08:23:16-6185	damian | damian |  Admin user
        /// 2020-09-15 08:55:38-1452	ADMIN | ADMIN |  Admin user
        /// 2020-09-15 09:52:21-8996	asd | asd |  Normal user
        /// 
        /// </summary>
        /// <param name="user">User into for make a string to log file</param>
        /// <param name="userIp">User IP</param>
        /// <param name="action">User Action</param>
        /// <returns>Formated string with user info</returns>
        private string FormatConnection(User user, string userIp, ConnectionAction action)
        {
            const string newLine = "\n";
            const string separator = newLine + "=======================================================================================================" + newLine;
            string formatedString = string.Empty;

            formatedString += userIp + " | " + DateTime.Now.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'-'FFFF") + newLine;

            switch (action)
            {
                case ConnectionAction.Login:
                    formatedString += "[Login]: ";
                    break;
                case ConnectionAction.Wrong_Login:
                    formatedString += "[Wrong login]: ";
                    break;
                case ConnectionAction.Disconnect:
                    formatedString += "[Disconnection]: ";
                    break;
            }
            
            formatedString += user.ToString();
            formatedString += separator;


            return formatedString;
        }
    }
}
