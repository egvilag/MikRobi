using System;

namespace MikRobi3
{
    class Program
    {
        public static Log log;

        static void Main(string[] args)
        {
            Console.WriteLine("MikRobi v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            log = new Log();
            log.Open();
            log.Write("misc", "Program started.");
            log.Close();
        }
    }
}
