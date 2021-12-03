using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace MikRobi3
{
    static class CommandProcessor
    {
        public static List<string> firstParameters = new List<string>()
        {
            "betatesting", "user", "username", "channelid", "messageid", "channelname", "groupid", "groupname"
        };

        public static List<string> secondParameters = new List<string>()
        {
            "hash", "sign", "role", "time", "msg", "channelid", "channelname", "groupid", "groupname"
        };

        public static void Process(Socket socket, string command)
        {
            //At least one parameter with valid format
            if ((command.Split('&').Length > 1) && (command.Split('=').Length > 1))
            {
                string commandName = command.Split('&')[0];
                string parameterString = command.Substring(command.IndexOf('&') + 1, command.Length - commandName.Length - 1);
                //string[] parametersArray = new string[2];
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                //Get the first parameter
                string key = parameterString.Split('=')[0];
                if (firstParameters.Contains(key))
                {
                    parameters.Add(key, parameterString.Split('&')[0]);
                }
                else return;

                //Get the second parameter
                string secondParameterString = parameterString.Substring(parameterString.IndexOf('&') + 1, parameterString.Length - parameterString.Split('&').Length - 1);
                key = secondParameterString.Split('=')[0];
                if (secondParameters.Contains(key))
                {
                    parameters.Add(key, secondParameterString.Substring(secondParameterString.IndexOf('='), secondParameterString.Length - key.Length - 1));
                }
                else return;

                //string key = "";
                //foreach (string s in parametersArray) // pl gmessage&channelid=0&msg=és&most=asdf
                //{
                //    if (s.Contains('='))
                //    {
                //        key = s.Split('=')[0];
                //        switch (parameters.Count)
                //        {
                //            case 0:
                //                if (firstParameters.Contains(key))
                //                    parameters.Add(key, s.Substring(s.IndexOf('=') + 1, s.Length - key.Length - 1));
                //                else return;
                //                break;

                //        }
                //    }
                //    else
                //    {
                //        parameters[key] += '&' + s;
                //    }
                //}

                //Decide what to do
                switch (commandName)
                {
                    case "sendgmessage":
                        foreach (Client cl in Program.clientNetwork.clients)
                        {
                            Program.clientNetwork.Send(cl.workSocket, parameters["msg"]);
                        }
                        break;
                    default:
                        return;
                }
            }
            else return;

            
        }
    }
}
