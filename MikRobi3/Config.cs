using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MikRobi3
{
    class Config
    {
        // Constants
        const string confPath = "/etc/dev/MikRobi";
        const string confFilename = "mikrobi.conf";

        StreamReader srConfig;

        // Opens the config file then ready the values from it
        public void Read()
        {
            if (File.Exists(confPath + "/" + confFilename))
            {
                try
                {
                    srConfig = new StreamReader(confPath + "/" + confFilename, Encoding.UTF8);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + confFilename + " file for read.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    File.Copy(confFilename.Replace(".conf", ".sample.conf"), confPath + "/" + confFilename);
                }
                catch
                {
                    Console.WriteLine("Failed to copy the " + confFilename + " sample file.");
                    Environment.Exit(1);
                }
            }
            string line, argument, value;
            int count = 0;
            try
            {
                while ((line = srConfig.ReadLine()) != null)
                {
                    argument = line.Split('=')[0];
                    value = line.Split('=')[1];
                    Program.settings[argument] = value;
                    count++;
                }
                if (count < Program.settings.Keys.Count) throw new Exception();
            }
            catch
            {
                Console.WriteLine("Failed to read from the " + confFilename + " file.");
                Environment.Exit(1);
            }
            srConfig.Close();
        }
    }
}
