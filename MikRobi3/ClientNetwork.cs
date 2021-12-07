using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MikRobi3
{
	//State object for reading client data asynchronously      
	public class Client
	{
		public Socket workSocket = null;
		public const int receiveBufferSize = 1024;  
		public byte[] receiveBuffer = new byte[receiveBufferSize];

		//Received data string.      
		public StringBuilder sb = new StringBuilder();

		public uint messageLength = 0;
		
		//user status
		//
		//0: email not verified
		//1: email verified, logged out
		//2: logged in
		//3: banned
		public uint status;

		public long userid;
		public string username;

		//user role
		//
		//a: admin
		//m: moderator
		//o: team owner
		//t: team admin
		//u: user
		public char role;
	}

	class ClientNetwork
	{
		//Thread signal.      
		public static ManualResetEvent allDone = new ManualResetEvent(false);

		//Clients list
		public List<Client> clients = new List<Client>();
		
		//Method to extract a part of an array
		public byte[] SubArray(byte[] data, int index, int length)
		{
			byte[] result = new byte[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		//Get the ip and port of a socket in 'ipaddress:port' format. Return empty string if socket is not connected.
		public string GetAddress(Socket socket)
        {
			IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
			if (remoteIpEndPoint != null)
			{
				// Using the RemoteEndPoint property.
				return remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port;
			}
			return "";
		}

		//Entry point of the unit. It runs in loop and processes new connections
		public void StartListening()
		{   
			//Automatically determine the IP address (currently this method is not in use)
			//IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName()); 
			//IPAddress ipAddress = ipHostInfo.AddressList[0];
			
			//Use the local IP
			IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(Program.settings["listenport"]));

			//Create a TCP/IP socket.     
			Socket listener; 

			//Bind the socket to the local endpoint and listen for incoming connections.      
			try
			{
				listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				listener.Bind(localEndPoint);
				listener.Listen(5);	//Queue is limited by 5
				Console.WriteLine(" Waiting for a connection... ");

				//Run continuosly since it's  a service
				while (true)
				{
					//Nothing is being done, so reset the event
					allDone.Reset();
					//Start an asynchronous socket to listen for connections.     
					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
					//Wait until a client arrives
					allDone.WaitOne();
				}
			}
			catch (Exception e)
			{
				Program.log.Write("error", e.Message);
			}
		}

		//Client is succesfully connected. Start to listen for incoming messages from them.
		public void AcceptCallback(IAsyncResult ar)
		{
			//Fire the event so the listener can continue processing other incoming connections
			allDone.Set();

			//Get the socket that handles the client request.
			Socket listenerSocket = (Socket)ar.AsyncState;
			Socket handlerSocket = listenerSocket.EndAccept(ar);

			Program.log.Write("security", "New Client connected from " + GetAddress(handlerSocket));

			//Create the client object.     
			Client client = new Client();
			client.workSocket = handlerSocket;
			clients.Add(client);

			//Start listening for incoming packets
			try
			{
				handlerSocket.BeginReceive(client.receiveBuffer, 0, Client.receiveBufferSize, 0, new AsyncCallback(ReadCallback), client);
			}
			catch (Exception ex)
            {
				Program.log.Write("error", ex.Message);
				handlerSocket.Close();
				handlerSocket.Dispose();
				clients.Remove(client);
			}
		}


		//A packet arrived!
		public void ReadCallback(IAsyncResult ar)
		{
			String content = String.Empty;
			Client client = (Client)ar.AsyncState; 
			Socket handlerSocket = client.workSocket;

			//Read data from the client socket.
			int bytesRead;
			try
			{
				bytesRead = handlerSocket.EndReceive(ar);
			}
			catch (Exception ex)
            {
				Program.log.Write("error", ex.Message);
				handlerSocket.Close();
				handlerSocket.Dispose();
				clients.Remove(client);
				return;
            }

			//If the packet is not empty
			if (bytesRead > 0)
			{
				//If the message length is not set, then read the first 4 bytes that should indicate it
				if (client.messageLength == 0)
				{
					try
					{
						client.messageLength = BitConverter.ToUInt32(SubArray(client.receiveBuffer, 0, 4));
						client.sb.Append(Encoding.UTF8.GetString(client.receiveBuffer, 4, bytesRead - 4));
						client.messageLength -= Convert.ToUInt32(bytesRead - 4);
					}
					catch (Exception ex)
					{
						//client.sb.Clear();
						Program.log.Write("error", ex.Message);
						return;
					}
				}
				else
				{
					//There might be more data, so store the data received so far.     
					client.sb.Append(Encoding.UTF8.GetString(client.receiveBuffer, 0, bytesRead));
					client.messageLength -= Convert.ToUInt32(bytesRead);
				}
				
				//Trim the message then store is in a temp variable
				content = client.sb.ToString().Trim();
				client.sb.Clear();

				//Process the message
				if (client.messageLength == 0)
					CommandProcessor.Process(handlerSocket, content);

				//Continue listening
				try
				{
					handlerSocket.BeginReceive(client.receiveBuffer, 0, Client.receiveBufferSize, 0, new AsyncCallback(ReadCallback), client);
				}
				catch (Exception ex)
                {
					Program.log.Write("error", ex.Message);
					handlerSocket.Close();
					handlerSocket.Dispose();
					clients.Remove(client);
				}
			}
			else
				if (!SocketConnected(handlerSocket))
				{
					handlerSocket.Dispose();
					clients.Remove(client);
				}
		}

		//Start sending some data
		public void Send(Socket handler, String data)
		{
			//byte[] byteData = new byte[4 + data.Length];
			byte[] byteData = new byte[4 + Encoding.UTF8.GetBytes(data).Length];
			BitConverter.GetBytes(data.Length).CopyTo(byteData, 0);
			//Convert the string data to byte data using ASCII encoding.      
			//Encoding.ASCII.GetBytes(data).CopyTo(byteData, 4);
			BitConverter.GetBytes(Encoding.UTF8.GetByteCount(data)).CopyTo(byteData, 0);
			Encoding.UTF8.GetBytes(data).CopyTo(byteData, 4);
			Program.log.Write("misc", byteData.Length + " bytes from " + GetAddress(handler) + " > " + data);

			//Begin sending the data to the remote device.    
			try
			{
				handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
			}
			catch (Exception ex)
			{
				Program.log.Write("error", ex.Message);
			}
		}

		//The async method to send
		private void SendCallback(IAsyncResult ar)
		{ 
			Client client = (Client)ar.AsyncState;
			Socket handlerSocket = client.workSocket;
			try
			{
				//Complete sending the data to the remote device.      
				int bytesSent = handlerSocket.EndSend(ar);

				//Originally this was where the story ended. Since we won't stop working, we mustn't close the socket.
				//handlerSocket.Shutdown(SocketShutdown.Both);
				//handlerSocket.Close();
			}
			catch (Exception e)
			{
				Program.log.Write("error", e.Message);
				handlerSocket.Close();
				handlerSocket.Dispose();
				clients.Remove(client);
			}
		}

		//Check if a socket is still connected
		private bool SocketConnected(Socket s)
		{
			bool part1 = s.Poll(1000, SelectMode.SelectRead);
			bool part2 = (s.Available == 0);
			if (part1 && part2)
				return false;
			else
				return true;
		}
	}
}
