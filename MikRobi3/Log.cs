using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MikRobi3
{
    class Log
    {
        // Constants
        const string logPath = "/var/log/MikRobi";
        const string logErrorFilename = "error.log";
        const string logSecurityFilename = "security.log";
        const string logMiscFilename = "misc.log";
        const int maxLogsize = 600000000; // in bytes

        // Streawriters
        StreamWriter swError;
        StreamWriter swSecurity;
        StreamWriter swMisc;


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
            //if (!File.Exists(logErrorFilename))
            {
                try
                {
                    swError = new StreamWriter(logPath + "/" + logErrorFilename, true, Encoding.UTF8, 65535);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + logErrorFilename + " file for write.");
                    Environment.Exit(1);
                }
            }
            //if (!File.Exists(logSecurityFilename))
            {
                try
                {
                    swSecurity = new StreamWriter(logPath + "/" + logSecurityFilename, true, Encoding.UTF8, 65535);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + logSecurityFilename + " file for write.");
                    Environment.Exit(1);
                }
            }
            //if (!File.Exists(logMiscFilename))
            {
                try
                {
                    swMisc = new StreamWriter(logPath + "/" + logMiscFilename, true, Encoding.UTF8, 65535);
                }
                catch
                {
                    Console.WriteLine("Failed to open the " + logMiscFilename + " file for write.");
                    Environment.Exit(1);
                }
            }
        }


        // Close the logfiles
        public void Close()
        {
            swError.Close();
            swError = null;
            swSecurity.Close();
            swSecurity = null;
            swMisc.Close();
            swMisc = null;
        }

        // Write a message to the specified logfile
        public void Write(string file, string message)
        {
            switch (file)
            {
                case "error": swError.WriteLine(DateTime.Now.ToString() + "\t> " + message); swError.Flush(); break;
                case "security": swSecurity.WriteLine(DateTime.Now.ToString() + "\t> " + message); swSecurity.Flush(); break;
                case "misc": swMisc.WriteLine(DateTime.Now.ToString() + "\t> " + message); swMisc.Flush(); break;
            }
            CheckFileSize(file);
        }


        // Check if file size reaches the limit. If it does then gzips it, then recreates a new empty one
        private void CheckFileSize(string file)
        {
            string timestring = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            switch (file)
            {
                case "error":
                    if (new FileInfo(logPath + "/" + logErrorFilename).Length > maxLogsize)
                    {
                        swError.Close();
                        using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        {
                            pProcess.StartInfo.FileName = "gzip";
                            pProcess.StartInfo.Arguments = logPath + "/" + logErrorFilename; //argument
                            //pProcess.StartInfo.Arguments =  "< " + logPath + "/" + logErrorFilename.Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + logErrorFilename; //argument
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
                        File.Move(logPath + "/" + logErrorFilename + ".gz", logPath + "/" + (logErrorFilename + ".gz").Replace(".gz", "_" + timestring + ".gz"));
                        swError = new StreamWriter(logPath + "/" + logErrorFilename, true, Encoding.UTF8, 65535);
                    }
                    break;
                case "security":
                    if (new FileInfo(logPath + "/" + logSecurityFilename).Length > maxLogsize)
                    {
                        swSecurity.Close();
                        using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        {
                            pProcess.StartInfo.FileName = "gzip";
                            pProcess.StartInfo.Arguments = logPath + "/" + logSecurityFilename; //argument
                            //pProcess.StartInfo.Arguments = "< " + logPath + "/" + logSecurityFilename.Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + logSecurityFilename; //argument
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
                        File.Move(logPath + "/" + logSecurityFilename + ".gz", logPath + "/" + (logSecurityFilename + ".gz").Replace(".gz", "_" + timestring + ".gz"));
                        swSecurity = new StreamWriter(logPath + "/" + logSecurityFilename, true, Encoding.UTF8, 65535);
                    }
                    break;
                case "misc": 
                    if (new FileInfo(logPath + "/" + logMiscFilename).Length > maxLogsize)
                    {
                        swMisc.Close();
                        using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        {
                            pProcess.StartInfo.FileName = "gzip";
                            pProcess.StartInfo.Arguments = logPath + "/" + logMiscFilename; //argument
                            //pProcess.StartInfo.Arguments = "< " + logPath + "/" + logMiscFilename.Replace(".log", "_") + timestring + ".gz >" + logPath + "/" + logMiscFilename; //argument
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
                        File.Move(logPath + "/" + logMiscFilename + ".gz", logPath + "/" + (logMiscFilename + ".gz").Replace(".gz", "_" + timestring + ".gz"));
                        swMisc = new StreamWriter(logPath + "/" + logMiscFilename, true, Encoding.UTF8, 65535);
                    }
                    break;
            }
        }
    }
}
