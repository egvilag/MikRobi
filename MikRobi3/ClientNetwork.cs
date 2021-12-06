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

		public void StartListening()
		{     
			//IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName()); 
			//IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(Program.settings["listenport"]));

			//Create a TCP/IP socket.     
			Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			//Bind the socket to the local endpoint and listen for incoming connections.      
			try
			{
				listener.Bind(localEndPoint);
				listener.Listen(100);
				Console.WriteLine(" Waiting for a connection... ");
				while (true)
				{
					allDone.Reset();
					//Start an asynchronous socket to listen for connections.     
					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
					allDone.WaitOne();
				}
			}
			catch (Exception e)
			{
				Program.log.Write("error", e.Message);
			}
		}

		public void AcceptCallback(IAsyncResult ar)
		{
			allDone.Set();

			//Get the socket that handles the client request.     
			Socket listener = (Socket)ar.AsyncState;
			Socket handler = listener.EndAccept(ar);

			Program.log.Write("security", "New Client connected from " + GetAddress(handler));

			//Create the state object.     
			Client client = new Client();
			client.workSocket = handler;
			clients.Add(client);
			handler.BeginReceive(client.receiveBuffer, 0, Client.receiveBufferSize, 0, new AsyncCallback(ReadCallback), client);
		}

		public void ReadCallback(IAsyncResult ar)
		{
			String content = String.Empty;
			//Retrieve the state object and the handler socket      
			//from the asynchronous state object.     
			Client client = (Client)ar.AsyncState; 
			Socket handler = client.workSocket;

			//Read data from the client socket.      
			int bytesRead = handler.EndReceive(ar);
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
					catch   // 4asdf
					{
						return;
					}
				}
				else
				{
					//There might be more data, so store the data received so far.     
					client.sb.Append(Encoding.UTF8.GetString(client.receiveBuffer, 0, bytesRead));
					client.messageLength -= Convert.ToUInt32(bytesRead);
				}
   
				content = client.sb.ToString().Trim();
				client.sb.Clear();
				if (client.messageLength == 0)
				{
					CommandProcessor.Process(handler, content);

					
				}
				handler.BeginReceive(client.receiveBuffer, 0, Client.receiveBufferSize, 0, new AsyncCallback(ReadCallback), client);
			}
		}

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
			handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				//Retrieve the socket from the state object.     
				Socket handler = (Socket)ar.AsyncState;

				//Complete sending the data to the remote device.      
				int bytesSent = handler.EndSend(ar);

				//handler.Shutdown(SocketShutdown.Both);
				//handler.Close();
			}
			catch (Exception e)
			{
				Program.log.Write("error", e.Message);
			}
		}
	}
}
