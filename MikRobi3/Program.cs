using System;
using System.Collections.Generic;
using System.Threading;

namespace MikRobi3
{
    class Program
    {
        // Units
        public static Log log;
        public static Config config;
        //public static Network network;
        public static Database database;
        public static ClientNetwork clientNetwork;

        public static Dictionary<string, string> settings;

        public static Timer timer;

        static void TimerRing(object state)
        {
            if ((DateTime.Now.Hour == 0) && (DateTime.Now.Minute == 0) && (DateTime.Now.Second == 0) && (DateTime.Now.DayOfWeek == DayOfWeek.Monday))
                if (log != null) 
                    log.Archive();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("MikRobi v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            settings = new Dictionary<string, string>()
            {
                { "listenport", "8888"},
                { "maxlogsize", "5120" },
                { "sql-server", "" },
                { "sql-port", "" },
                { "sql-user", "" },
                { "sql-password", "" },
                { "sql-database", "" }
            };

            timer = new Timer(TimerRing, null, 0, 1000);

            config = new Config();
            config.Read();

            log = new Log();
            log.Open();
            log.Write("misc", "Program started.");

            database = new Database();
            database.TestDB();
            //Console.WriteLine(database.GetLatestUpdate(false));
            //Console.WriteLine(database.GetLatestUpdate(true));

            //network = new Network();
            //network.StartListen(Convert.ToInt32(settings["listenport"]), "127.0.0.1");

            clientNetwork = new ClientNetwork();
            clientNetwork.StartListening();

            Console.WriteLine("Esc to stop.");
            ConsoleKey s;
            do
            {
                s = Console.ReadKey().Key;
            } while (s != ConsoleKey.Escape);

            //network.StopListen();

            log.Write("misc", "Program Stopped.");
            log.Close();
        }
    }
}
