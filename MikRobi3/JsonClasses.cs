using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace MikRobi3
{
    static class JsonClasses
    {
        //Records
        public const string header = "Elements JSON data";
        public const string serverName = "Neo";
        public static string serverVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //public static string clientPlatform = GetPlatform();

        //static string GetPlatform()
        //{
        //    int p = (int)Environment.OSVersion.Platform;
        //    switch (p)
        //    {
        //        case 0:
        //            return "Windows Win32s";
        //        case 1:
        //            return "Windows 9x";
        //        case 2:
        //            return "Windows NT";
        //        case 3:
        //            return "Windows CE";
        //        case 4:
        //            return "Unix";
        //        case 6:
        //            return "OSX";
        //        default:
        //            return "Other";
        //    }
        //}
    }

    class UpdateJSON
    {
        //Records
        const string command = "update";
        //bool betaTesting;
        //string hash;

        //Constructor
        //public UpdateJSON(bool betaTesting, string hash)
        //{
        //    this.betaTesting = betaTesting;
        //    this.hash = hash;
        //}

        //Return JSON text
        public string GetResult(int status, string link)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JsonWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;

            writer.WriteStartObject();
            writer.WritePropertyName("header");
            writer.WriteValue(JsonClasses.header);
            writer.WritePropertyName("server");
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(JsonClasses.serverName);
            writer.WritePropertyName("version");
            writer.WriteValue(JsonClasses.serverVersion);
            writer.WriteEndObject();
            writer.WritePropertyName("command");
            writer.WriteValue(command);
            writer.WritePropertyName("status");
            writer.WriteValue(status);
            writer.WritePropertyName("link");
            writer.WriteValue(link);
            writer.WriteEndObject();

            return sb.ToString();
        }
    }


}
