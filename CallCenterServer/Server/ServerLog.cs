using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CallCenterServer
{
    class ServerLog
    {
        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        private static readonly string PATH_LOG_FILE = config["PATH_LOG_FOLDER"];

    }
}
