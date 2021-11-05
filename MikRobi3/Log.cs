using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace MikRobi3
{
    class Log
    {
        // Constants
        const string logPath = "/var/log/MikRobi";
        const string logErrorFilename = "error_[dow].log";
        const string logSecurityFilename = "security_[dow].log";
        const string logMiscFilename = "misc_[dow].log";
        const int maxLogsize = 600000000; // in bytes
        //const int maxLogsize = 433; // in bytes

        // Streawriters
        StreamWriter swError;
        StreamWriter swSecurity;
        StreamWriter swMisc;

        // Buffers to use when files are closed
        List<string> logErrorMemory = new List<string>();
        List<string> logSecurityMemory = new List<string>();
        List<string> logMiscMemory = new List<string>();

        // Checking if the files and paths exist (if not, creates them), then opens the files
        public void Open()
        {
            if (!Directory.Exists(logPath))
            {
                try
                {
                    Directory.CreateDirectory(logPath);
                }
                catch
                {
                    Console.WriteLine("Log path not found, failed to create directory: " + logPath);
                    Environment.Exit(1);
                }
            }
            if (!Directory.Exists(logPath  +"/Archive"))
            {
                try
                {
                    Directory.CreateDirectory(logPath + "/Archive");
                }
                catch
                {
                    Console.WriteLine("Log path not found, failed to create directory: " + logPath);
                    Environment.Exit(1);
                }
            }
            //if (!File.Exists(logErrorFilename))
            {
                try
                {
                    swError = new StreamWriter(logPath + "/" + GetFileName(logErrorFilename, false), true, Encoding.UTF8, 65535);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + GetFileName(logErrorFilename, false) + " file for write.");
                    Environment.Exit(1);
                }
            }
            //if (!File.Exists(GetFileName(logSecurityFilename)))
            {
                try
                {
                    swSecurity = new StreamWriter(logPath + "/" + GetFileName(logSecurityFilename, false), true, Encoding.UTF8, 65535);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + GetFileName(logSecurityFilename, false) + " file for write.");
                    Environment.Exit(1);
                }
            }
            //if (!File.Exists(GetFileName(logMiscFilename)))
            {
                try
                {
                    swMisc = new StreamWriter(logPath + "/" + GetFileName(logMiscFilename, false), true, Encoding.UTF8, 65535);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + GetFileName(logMiscFilename, false) + " file for write.");
                    Environment.Exit(1);
                }
            }
        }


        // Close the logfiles
        public void Close()
        {
            swError.Close();
            swError.Dispose();
            swError = null;
            swSecurity.Close();
            swSecurity.Dispose();
            swSecurity = null;
            swMisc.Close();
            swMisc.Dispose();
            swMisc = null;
        }

        // Write a message to the specified logfile (or into memory, if file is closed)
        public void Write(string file, string message)
        {
            int unixTime = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            switch (file)
            {
                case "error":
                    if (swError != null)
                    {
                        swError.WriteLine(unixTime.ToString() + "-" + DateTime.Now.ToString() + "\t> " + message);
                        swError.Flush();
                    }
                    else logErrorMemory.Add(unixTime.ToString() + "-" + DateTime.Now.ToString() + "\t> " + message);
                    break;
                case "security":
                    if (swSecurity != null)
                    {
                        swSecurity.WriteLine(unixTime.ToString() + "-" + DateTime.Now.ToString() + "\t> " + message);
                        swSecurity.Flush();
                    }
                    else logSecurityMemory.Add(unixTime.ToString() + "-" + DateTime.Now.ToString() + "\t> " + message);
                    break;
                case "misc":
                    if (swMisc != null)
                    {
                        swMisc.WriteLine(unixTime.ToString() + "-" + DateTime.Now.ToString() + "\t> " + message); swMisc.Flush();
                    }
                    else logMiscMemory.Add(unixTime.ToString() + "-" + DateTime.Now.ToString() + "\t> " + message);
                    break;
            }
            CheckFileSize(file);
        }


        // Check if file size reaches the limit. If it does then gzips it, then recreates a new empty one
        private void CheckFileSize(string file)
        {
            //string timestring = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            switch (file)
            {
                case "error":
                    if (new FileInfo(logPath + "/" + GetFileName(logErrorFilename, false)).Length > maxLogsize)
                    {
                        swError.Close();
                        swError.Dispose();
                        swError = null;
                        //using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        //{
                        //    pProcess.StartInfo.FileName = "gzip";
                        //    pProcess.StartInfo.Arguments = logPath + "/" + GetFileName(logErrorFilename, false); //argument
                        //    //pProcess.StartInfo.Arguments =  "< " + logPath + "/" + logErrorFilename.Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + logErrorFilename; //argument
                        //    pProcess.StartInfo.UseShellExecute = false;
                        //    pProcess.StartInfo.RedirectStandardOutput = true;
                        //    pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        //    pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                        //    pProcess.Start();
                        //    string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                        //    pProcess.WaitForExit();
                        //}
                        //File.Move(logPath + "/" + GetFileName(logErrorFilename, false) + ".gz", logPath + "/" + (logErrorFilename + ".gz").Replace(".gz", "_" + timestring + ".gz"));
                        swError = new StreamWriter(logPath + "/" + GetFileName(logErrorFilename, true), true, Encoding.UTF8, 65535);
                        foreach (string s in logErrorMemory)
                        {
                            swError.WriteLine(s);
                            swError.Flush();
                        }
                        logErrorMemory.Clear();

                    }
                    break;
                case "security":
                    if (new FileInfo(logPath + "/" + GetFileName(logSecurityFilename, false)).Length > maxLogsize)
                    {
                        swSecurity.Close();
                        swSecurity.Dispose();
                        swSecurity = null;
                        //using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        //{
                        //    pProcess.StartInfo.FileName = "gzip";
                        //    pProcess.StartInfo.Arguments = logPath + "/" + GetFileName(logSecurityFilename, false); //argument
                        //    //pProcess.StartInfo.Arguments = "< " + logPath + "/" + GetFileName(logSecurityFilename).Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + GetFileName(logSecurityFilename); //argument
                        //    pProcess.StartInfo.UseShellExecute = false;
                        //    pProcess.StartInfo.RedirectStandardOutput = true;
                        //    pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        //    pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                        //    pProcess.Start();
                        //    string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                        //    pProcess.WaitForExit();
                        //}
                        //File.Move(logPath + "/" + GetFileName(logSecurityFilename, false) + ".gz", logPath + "/" + (GetFileName(logSecurityFilename, false) + ".gz").Replace(".gz", "_" + timestring + ".gz"));
                        swSecurity = new StreamWriter(logPath + "/" + GetFileName(logSecurityFilename, true), true, Encoding.UTF8, 65535);
                        foreach (string s in logSecurityMemory)
                        {
                            swSecurity.WriteLine(s);
                            swSecurity.Flush();
                        }
                        logSecurityMemory.Clear();
                    }
                    break;
                case "misc": 
                    if (new FileInfo(logPath + "/" + GetFileName(logMiscFilename, false)).Length > maxLogsize)
                    {
                        swMisc.Close();
                        swMisc.Dispose();
                        swMisc = null;
                        //using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        //{
                        //    pProcess.StartInfo.FileName = "gzip";
                        //    pProcess.StartInfo.Arguments = logPath + "/" + GetFileName(logMiscFilename, false); //argument
                        //    //pProcess.StartInfo.Arguments = "< " + logPath + "/" + GetFileName(logMiscFilename).Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + GetFileName(logMiscFilename); //argument
                        //    pProcess.StartInfo.UseShellExecute = false;
                        //    pProcess.StartInfo.RedirectStandardOutput = true;
                        //    pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        //    pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                        //    pProcess.Start();
                        //    string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                        //    pProcess.WaitForExit();
                        //}
                        //File.Move(logPath + "/" + GetFileName(logMiscFilename, false) + ".gz", logPath + "/" + (GetFileName(logMiscFilename, false) + ".gz").Replace(".gz", "_" + timestring + ".gz"));
                        swMisc = new StreamWriter(logPath + "/" + GetFileName(logMiscFilename, true), true, Encoding.UTF8, 65535);
                        foreach (string s in logMiscMemory)
                        {
                            swMisc.WriteLine(s);
                            swMisc.Flush();
                        }
                        logMiscMemory.Clear();
                    }
                    break;
            }
        }

        public string GetFileName(string constFilename, bool next)
        {
            Dictionary<string, string> days = new Dictionary<string, string>()
            {
                { "Mo", "Hé" },
                { "Tu", "Ke" },
                { "We", "Sze" },
                { "Th", "Cs" },
                { "Fr", "Pé" },
                { "Sa", "Szo" },
                { "Su", "Va" }

            };
            string name = constFilename.Replace("[dow]", DateTime.Now.DayOfWeek.ToString().Substring(0, 2));
            string[] files = Directory.GetFiles(logPath); 
            //List<int> numbers = new List<int>();
            int number = 0;
            string fname;
            foreach (string s in files)
            {
                fname = Path.GetFileName(s);
                if ((fname.Split('_')[0] + fname.Split('_')[1].Substring(0, 2) == name.Split('_')[0] + name.Split('_')[1].Substring(0, 2)) && (fname.Split('_').Length > 2))
                    if (Convert.ToInt32(fname.Split('_')[2].Split('.')[0]) > number)
                        number = Convert.ToInt32(fname.Split('_')[2].Split('.')[0]);
            }
            if (number == 0)
            {
                if (next)
                    name = name.Replace(".log", "_" + (number + 1).ToString() + ".log");
            }
            else
            {
                if (next)
                    name = name.Replace(name.Split('_')[1].Split('.')[0], name.Split('_')[1].Split('.')[0] + "_" + (number + 1).ToString());
                else
                    name = name.Replace(name.Split('_')[1].Split('.')[0], name.Split('_')[1].Split('.')[0] + "_" + number.ToString());
            }


            //if (next) number++;
            //if (number > 0)
            //{
            //    name = name.Replace(".log", "_" + number.ToString() + ".log");
            //}
            return name;
        }

        public void Archive()
        {
            string timestring = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            int unixTime = Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            this.Close();
            //string s;
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                //s = "--remove-files";
                pProcess.StartInfo.FileName = "tar";
                pProcess.StartInfo.Arguments = "--no-recursion -zcvf " + logPath + "/Archive/mikrobi_" + unixTime.ToString() + "-" + timestring + ".tar.gz " + logPath + "/*"; //argument
                //pProcess.StartInfo.Arguments = "< " + logPath + "/" + GetFileName(logMiscFilename).Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + GetFileName(logMiscFilename); //argument
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.Start();
                string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                pProcess.WaitForExit();
            }
            System.IO.DirectoryInfo di = new DirectoryInfo(logPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            //File.Move(Directory.GetFiles(logPath, "*.gz")[0], logPath + "/Archive/" + Path.GetFileName(Directory.GetFiles(logPath, "*.gz")[0]).Replace(".gz", "_" + timestring + ".gz"));
            //File.Move(logPath + "/" + GetFileName(logMiscFilename, false) + ".gz", logPath + "/" + (GetFileName(logMiscFilename, false) + ".gz").Replace(".gz", "_" + timestring + ".gz"));
            this.Open();
        }
    }
}
