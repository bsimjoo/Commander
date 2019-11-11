using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Net.Sockets;
namespace Command_Server
{
	class ClientManager
	{
		public static string CommandFlags { get; set; }
		public Socket ClientSocket = null;
		public bool Muted { get; set; } = false;
		public int WaitSec { get; set; } = 30;
		public string[] ClientInfo = new string[3];
		public string IDName { get; set; }
		public ClientManager(Socket cs) {
			ClientSocket = cs;
			Program.Log(Program.logType.Info, "new client detected, getting information");
			Send("<$>info");        //get client info by sending $ flag and info keyword
			string Text = "";
			if (Read(true, out Text)) {     //check is there any thing to read?
				string[] Parts = Text.Split(';');           //ComputerName;UserName;CostumName
				Array.Copy(Parts, ClientInfo, Parts.Length);        //save client info as an array property of client
			} else {
				Console.WriteLine("Request timed out. waited for {0}s", WaitSec);
				Disconnect(DisconnectReason.timedOut);
			}
			Thread th = new Thread(new ParameterizedThreadStart(CheckClient));
			th.Start(this);
		}
		private static void CheckClient(object client) {
			ClientManager cl = client as ClientManager;
			while (cl.ClientSocket.Connected) {
				Thread.Sleep(10);      //limit usage of resources and check for client connection state every 5 seconds.
				if (!cl.Muted)
					if (cl.Read(false, out string Text)) {
						Program.ColoredWrite(ConsoleColor.Yellow, ConsoleColor.Black, $"Output from {cl.IDName}".PadRight(Console.WindowWidth, '='));
						Console.WriteLine(Text);
					}
			}
			cl.Disconnect(DisconnectReason.clientClosed);
		}
		public delegate void DisconnectEventHandler(ClientManager client, DisconnectReason r);
		public event DisconnectEventHandler Disconnected;
		public void Disconnect(DisconnectReason r) {
			if (ClientSocket.Connected) {
				if (r == DisconnectReason.internalError || r == DisconnectReason.manual)
					Send("<$>close");
				else
					Send("<$>disconnected");
				ClientSocket.Close();
				Disconnected(this, r);
			}
		}
		/// <summary>
		/// if there's any text to read it returns true and else false.
		/// </summary>
		/// <param name="Wait"></param>
		/// <param name="Text"></param>
		/// <returns></returns>
		public bool Read(bool Wait, out string Text) {
			Text = "";
			int count = 0;
			var t = DateTime.Now;
			while (ClientSocket.Connected) {
				try {
					byte[] buffer = new byte[1024];
					count = ClientSocket.Receive(buffer);
					Text += Encoding.ASCII.GetString(buffer, 0, count);
				} catch (Exception ex) { Program.Log(Program.logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); break; }
				if (Text.Contains("<$eof>")) {
					break;
				} else if (count == 0) {
					if (Wait) {
						if ((t - DateTime.Now) >= new TimeSpan(0, 0, WaitSec))
							break;
					} else
						break;
				}
			}
			bool Empty = Text == "";
			if (Text.Length != 0) {
				try { Text = Text.Replace("<$eof>", ""); } catch { /*do nothing*/ }
			}
			return !Empty;
		}
		public void Send(string Text) {
			string Flags = Regex.Match(Text, @"^\<.+\>\b", RegexOptions.Multiline).Value;    //get flags by regex (example)-> https://regexr.com/4obl9 recommend to use external browser.
			Flags = Flags.Trim('<', '>');
			Flags += new string(CommandFlags.Select(c => (Flags.Contains(c) ? '\0' : c)).ToArray());
			Text = $"<{Flags}>{Text.Substring(Flags.Length)}<$eof>";
			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			try {
				if (ClientSocket.Connected)
					ClientSocket.Send(buffer);
			} catch (Exception ex) { Program.Log(Program.logType.Error, $"EXCEPTION IN CLIENT/SEND: {ex.Message}"); }

		}
		public enum DisconnectReason
		{
			timedOut, manual, clientClosed, internalError
		}
	}
}