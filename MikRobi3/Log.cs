using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MikRobi3
{
    class Log
    {
        const string logPath = "/var/log/MikRobi";
        const string logErrorFilename = "error.log";
        const string logSecurityFilename = "security.log";
        const string logMiscFilename = "misc.log";
        const int maxLogsize = 600;

        StreamWriter swError;
        StreamWriter swSecurity;
        StreamWriter swMisc;

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
            if (!File.Exists(logErrorFilename))
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
            if (!File.Exists(logSecurityFilename))
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
            if (!File.Exists(logMiscFilename))
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

        public void Close()
        {
            swError.Close();
            swError = null;
            swSecurity.Close();
            swSecurity = null;
            swMisc.Close();
            swMisc = null;
        }

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

        private void CheckFileSize(string file)
        {
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
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
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
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
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
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
                        swMisc = new StreamWriter(logPath + "/" + logMiscFilename, true, Encoding.UTF8, 65535);
                    }
                    break;
            }
        }
    }
}
