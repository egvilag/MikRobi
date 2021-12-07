using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace MikRobi3
{
    static class CommandProcessor
    {
        //A list with allowed parameter in the command string
        public static List<string> allowedParameters = new List<string>()
        {
            "betatesting", "user", "username", "channelid", "messageid", "channelname", "groupid", "groupname",
            "hash", "sign", "role", "time", "msg"
        };

        //Process message if it contains a command
        public static void Process(Socket socket, string command)
        {
            //At least one parameter with valid format (contains '&' and '=' character)
            if ((command.Split('&').Length > 1) && (command.Split('=').Length > 1))
            {
                //Separate the command identifier from the parameters
                string commandName = command.Split('&')[0];
                string parameterString = command.Substring(command.IndexOf('&') + 1, command.Length - commandName.Length - 1);

                //Init a dictionary to store the parameters
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                //Get the first parameter
                string key = parameterString.Split('=')[0];
                if (allowedParameters.Contains(key))
                {
                    parameters.Add(key, parameterString.Split('&')[0].Split('=')[1]);
                }
                else return;

                //Get the second parameter
                string secondParameterString = parameterString.Substring(parameterString.IndexOf('&') + 1, parameterString.Length - parameterString.Split('&')[0].Length - 1);
                key = secondParameterString.Split('=')[0];
                if (allowedParameters.Contains(key))
                {
                    parameters.Add(key, secondParameterString.Substring(secondParameterString.IndexOf('=') + 1, secondParameterString.Length - key.Length - 1));
                }
                else return;

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
                        string result = Program.database.GetLatestUpdate(beta, parameters["hash"]);
                        switch (result[0])
                        {
                            case '0': //Running the latest version
                                Program.clientNetwork.Send(socket, "update&status=0");
                                break;
                            case '1': //Running an outdated version
                                Program.clientNetwork.Send(socket, "update&status=1&link=" + result.Substring(result.IndexOf('&') + 1, result.Length - 2));
                                break;
                            case '2': //Running a bad/tampered executable
                                Program.clientNetwork.Send(socket, "update&status=2&link=" + result.Substring(result.IndexOf('&') + 1, result.Length - 2));
                                break;
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
