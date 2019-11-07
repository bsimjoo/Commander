using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
namespace Command_Server
{
	class ClientManager
	{
		public Socket ClientSocket = null;
		public int WaitSec { get; set; } = 30;
		public string[] ClientInfo = new string[3];
		public ClientManager(Socket cs) {
			ClientSocket = cs;
			Send("<$>info");        //get client info by sending this keyword
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
				Thread.Sleep(500);      //limit using of resources and check for client connection state every 5 seconds.
			}
			cl.Disconnect(DisconnectReason.clientClosed);
		}
		public delegate void DisconnectEventHandler(ClientManager client, DisconnectReason r);
		public event DisconnectEventHandler Disconnected;
		public void Disconnect(DisconnectReason r) {
			if (r == DisconnectReason.internalError || r == DisconnectReason.manual)
				Send("<$>close");
			else
				Send("<$>disconnected");
			ClientSocket.Close();
			Disconnected(this, r);
		}
		/// <summary>
		/// if there's any text to read it returns true and else false.
		/// </summary>
		/// <param name="Wait"></param>
		/// <param name="Text"></param>
		/// <returns></returns>
		public bool Read(bool Wait, out string Text) {
			Text = "";
			var t = DateTime.Now;
			while (Wait && (t - DateTime.Now) <= new TimeSpan(0, 0, WaitSec)) {
				try {
					byte[] buffer = new byte[1024];
					int count = ClientSocket.Receive(buffer);
					Text += Encoding.ASCII.GetString(buffer, 0, count);
				} catch (Exception ex) { Program.Log(Program.logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); break; }
				if (Text.LastIndexOf("<$eof>") > -1)
					break;
			}
			if (Text.Length != 0)
				Text.Replace("<$eof>", "");
			return Text.Length != 0;
		}
		public string DefFlag { get; set; } = "<nf>";		//Flages: n:normal, f:get feedback, a:admin, v:visible
		public void Send(string Text) {
			Text =DefFlag+Text+"<$eof>";
			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			try {
				ClientSocket.Send(buffer);
			} catch (Exception ex) { Program.Log(Program.logType.Error, $"EXCEPTION IN CLIENT/SEND: {ex.Message}"); }

		}
		public enum DisconnectReason
		{
			timedOut, manual, clientClosed, internalError
		}
	}
}
