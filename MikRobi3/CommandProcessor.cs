using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace MikRobi3
{
    static class CommandProcessor
    {
        //A list with allowed parameter in the command string
        public static List<string> allowedParameters = new List<string>()
        {
            "betatesting", "user", "username", "channelid", "messageid", "channelname", "groupid", "groupname",
            "hash", "sign", "role", "time", "msg", "0"
        };

        //Process message if it contains a command
        //public static void Process(Socket socket, string command)
        //{
        //    //At least one parameter with valid format (contains '&' and '=' character)
        //    if ((command.Split('&').Length > 1) && (command.Split('=').Length > 1))
        //    {
        //        //Separate the command identifier from the parameters
        //        string commandName = command.Split('&')[0];
        //        string parameterString = command.Substring(command.IndexOf('&') + 1, command.Length - commandName.Length - 1);

        //        //Init a dictionary to store the parameters
        //        Dictionary<string, string> parameters = new Dictionary<string, string>();

        //        string key;
        //        //Get the first parameter (if present)
        //        if (parameterString.Split('=').Length > 0)
        //        {
        //            key = parameterString.Split('=')[0];
        //            if (allowedParameters.Contains(key))
        //            {
        //                parameters.Add(key, parameterString.Split('&')[0].Split('=')[1]);
        //            }
        //            else return;
        //        }

        //        //Get the second parameter (if present)
        //        if (parameterString.Contains("&"))
        //        {
        //            string secondParameterString = parameterString.Substring(parameterString.IndexOf('&') + 1, parameterString.Length - parameterString.Split('&')[0].Length - 1);
        //            if (secondParameterString.Length > 0)
        //            {
        //                key = secondParameterString.Split('=')[0];
        //                if (allowedParameters.Contains(key))
        //                {
        //                    parameters.Add(key, secondParameterString.Substring(secondParameterString.IndexOf('=') + 1, secondParameterString.Length - key.Length - 1));
        //                }
        //                else return;
        //            }
        //        }

        //        //Lets's see the command to decide what to do
        //        switch (commandName)
        //        {
        //            case "sendgmessage": //New global message
        //                foreach (Client cl in Program.clientNetwork.clients)
        //                {
        //                    Program.clientNetwork.Send(cl.workSocket, commandName + "&channelid=" + parameters["channelid"] + "&msg=" + parameters["msg"]);
        //                }
        //                break;

        //            case "update": //Asking for update link
        //                bool beta = parameters["betatesting"] == "1";
        //                string result = Program.database.GetLatestUpdate(beta, parameters["hash"]);
        //                switch (result[0])
        //                {
        //                    case '0': //Running the latest version
        //                        Program.clientNetwork.Send(socket, "update&status=0");
        //                        break;
        //                    case '1': //Running an outdated version
        //                        Program.clientNetwork.Send(socket, "update&status=1&link=" + result.Substring(result.IndexOf('&') + 1, result.Length - 2));
        //                        break;
        //                    case '2': //Running a bad/tampered executable
        //                        Program.clientNetwork.Send(socket, "update&status=2&link=" + result.Substring(result.IndexOf('&') + 1, result.Length - 2));
        //                        break;
        //                }
        //                break;
        //            case "getsalt":
        //                int found = -1;
        //                for (int i = 0; i < Program.clientNetwork.clients.Count; i++)
        //                {
        //                    if (Program.clientNetwork.clients[i].workSocket == socket)
        //                    {
        //                        found = i;
        //                        break;
        //                    }
        //                }
        //                if (found > -1)
        //                {
        //                    Random rnd = new Random();
        //                    rnd.NextBytes(Program.clientNetwork.clients[found].salt);
        //                    //Program.clientNetwork.Send(socket, "getsalt&salt=" + Encoding.UTF8.GetString(Program.clientNetwork.clients[found].salt));
        //                    Program.clientNetwork.Send(socket, "getsalt&salt=", Program.clientNetwork.clients[found].salt);
        //                }
        //                break;


        //            default:
        //                return;
        //        }
        //    }
        //    else return;
        //}

        //Process JSON messages
        public static void Process(Socket socket, string command)
        {
            string commandName = "";
            string parameterName = "";
            bool commandNext = false;
            bool parameterNext = false;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //Read the JSON
            if (command.StartsWith("{"))
            {


                JsonTextReader reader = new JsonTextReader(new StringReader(command));
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        if ((commandNext) && (reader.TokenType == JsonToken.String))
                        {
                            commandName = reader.Value.ToString();
                            commandNext = false;
                        }
                        if ((reader.TokenType == JsonToken.PropertyName) && (reader.Value.ToString() == "command"))
                            commandNext = true;
                        if (commandName != "")
                        {
                            if ((parameterNext) && (reader.TokenType != JsonToken.PropertyName) && (parameterName != ""))
                            {
                                parameters.Add(parameterName, reader.Value.ToString());
                                parameterName = "";
                                parameterNext = false;
                            }
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                parameterName = reader.Value.ToString();
                                parameterNext = true;
                            }
                        }
                    }
                }
                reader.Close();
            }

            //Lets's see the command to decide what to do
            switch (commandName)
            {
                case "sendgmessage": //New global message
                    foreach (Client cl in Program.clientNetwork.clients)
                    {
                        Program.clientNetwork.Send(cl.workSocket, commandName + "&channelid=" + parameters["channelid"] + "&msg=" + parameters["msg"]);
                    }
                    break;

                case "update": //Asking for update link
                    
                    bool beta = parameters["betatesting"] == "1";
                    string hash = parameters["hash"];
                    string result = Program.database.GetLatestUpdate(beta, hash);
                    UpdateJSON updateJSON = new UpdateJSON();
                    switch (result[0])
                    {
                        case '0': //Running the latest version
                            Program.clientNetwork.Send(socket, updateJSON.GetResult(0, ""));
                            //Program.clientNetwork.Send(socket, "update&status=0");
                            break;
                        case '1': //Running an outdated version
                            Program.clientNetwork.Send(socket, updateJSON.GetResult(1, result.Substring(result.IndexOf('&') + 1, result.Length - 2));
                            //Program.clientNetwork.Send(socket, "update&status=1&link=" + result.Substring(result.IndexOf('&') + 1, result.Length - 2));
                            break;
                        case '2': //Running a bad/tampered executable
                            Program.clientNetwork.Send(socket, updateJSON.GetResult(2, result.Substring(result.IndexOf('&') + 1, result.Length - 2));
                            //Program.clientNetwork.Send(socket, "update&status=2&link=" + result.Substring(result.IndexOf('&') + 1, result.Length - 2));
                            break;
                    }
                    break;
                case "getsalt":
                    int found = -1;
                    for (int i = 0; i < Program.clientNetwork.clients.Count; i++)
                    {
                        if (Program.clientNetwork.clients[i].workSocket == socket)
                        {
                            found = i;
                            break;
                        }
                    }
                    if (found > -1)
                    {
                        Random rnd = new Random();
                        rnd.NextBytes(Program.clientNetwork.clients[found].salt);
                        Program.clientNetwork.Send(socket, "getsalt&salt=", Program.clientNetwork.clients[found].salt);
                    }
                    break;


                default:
                    return;
            }
        }

        //Convert byte array to hex
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }


        //Convert hex string to byte array
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
