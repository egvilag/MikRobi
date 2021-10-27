using System;

namespace MikRobi3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
