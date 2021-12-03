using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace MikRobi3
{
    class Network
    {
        Socket serverSocket, clientSocket;
        int bufferSize = 1024;
        byte[] buffer;

        public void StartListen(int port, string host)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
                serverSocket.Listen(100);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                Console.WriteLine("Listening on " + host + ":" + port);
            }
            catch (Exception ex)
            {
                Program.log.Write("security", "Error accepting connection: " + ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket = serverSocket.EndAccept(AR);
                buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception ex)
            {
                Program.log.Write("security", "Error accepting connection: " + ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = clientSocket.EndReceive(AR);
                string text = Encoding.ASCII.GetString(buffer).Trim();
                Array.Resize(ref buffer, received);
                Program.log.Write("security", "Received message: " + text);
                Array.Resize(ref buffer, clientSocket.ReceiveBufferSize);
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception ex)
            {
                Program.log.Write("security", "Error accepting connection: " + ex.Message);
            }
        }

        public void StopListen()
        {
            if (clientSocket != null) clientSocket.Close();
            serverSocket.Close();
        }
    }
}
