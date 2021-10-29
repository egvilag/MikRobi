using System;
using System.Collections.Generic;

namespace MikRobi3
{
    class Program
    {
        // Units
        public static Log log;
        public static Config config;
        public static Network network;

        public static Dictionary<string, string> settings;

        static void Main(string[] args)
        {
            Console.WriteLine("MikRobi v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            settings = new Dictionary<string, string>()
            {
                { "ListenPort", ""}
            };
            log = new Log();
            log.Open();
            log.Write("misc", "Program started.");
            config = new Config();
            config.Read();
            network = new Network();
            log.Write("misc", "Program Stopped.");
            log.Close();
        }
    }
}
