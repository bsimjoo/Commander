using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommanderClient
{
	class ClientListener
	{
		private static Socket ServerSocket;
		public void Run() {
			Thread th = new Thread(new ThreadStart(ClientThread));
			th.Start();
		}
		static void ClientThread() {
			IPAddress ip = IPAddress.Parse(CommonVar.ipAddr);
			ServerSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				while (true) {
					if (!ServerSocket.Connected) {
						ServerSocket.Connect(ip, int.Parse(CommonVar.port));
					}
				}
			}catch(Exception ex) { Program.logger.Log(logType.Error, ex.Message); }
		}
		public static void ListenToServer() {
			while (ServerSocket.Connected) {
				string Text = "";
				while (true) {
					while (true) {
						try {
							byte[] buffer = new byte[1024];
							int count = ServerSocket.Receive(buffer);
							Text += Encoding.ASCII.GetString(buffer, 0, count);
						} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); }
						if (Text.LastIndexOf("<$eof>") > -1)
							break;
					}
					if (Text != "")
						break;
				}
				Text.Replace("<$eof>", "");
				CommonVar.CmdStandardInput.WriteLine(Text);
			}
		}

		public string Read(bool Wait) {
			string Text = "";
			while (Wait) {
				while (true) {
					try {
						byte[] buffer = new byte[1024];
						int count = ServerSocket.Receive(buffer);
						Text += Encoding.ASCII.GetString(buffer, 0, count);
					} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); }
					if (Text.LastIndexOf("<$eof>") > -1)
						break;
				}
				if (Text != "")
					break;
			}
			Text.Replace("<$eof>", "");
			return Text;
		}
		public void Send(string Text) {
			Text += "<$eof>";
			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			try {
				ServerSocket.Send(buffer);
			} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN CLIENT/SEND: {ex.Message}"); }

		}
	}
}
