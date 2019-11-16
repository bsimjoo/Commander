using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace CommanderClient
{
	class internalcmds
	{
		public static Dictionary<string, command> Commands = new Dictionary<string, command>();
		public internalcmds() {
			Commands["run"] = new command(Run);
			Commands["print"] = new command(Print);
			Commands["info"] = new command(Info);
			Commands["close"] = new command(Close);
			Commands["directcmd"] = new command(DirectCmd.Run);
			Commands["closecmd"] = new command(DirectCmd.Close);
		}
		public void Do(string CommandLine) {
			string Command = CommandLine;
			var M = Regex.Matches(CommandLine, Properties.Resources.regexFormat);
			List<string> Args = new List<string>();
			if (M.Count >= 2) {     //may be there is argument
				Command = M[0].Value;
				for(int i = 1; i < M.Count; i++) {
					Args.Add(M[i].Value);
				}
				Args.Remove(" ");
				Args.Remove("");
			}
			//^ there was too many special chars in regex format so I saved it in resources for easier access and edit.
			//regex example -> https://regexr.com/4obll recommend to use external browser.
			
			Console.WriteLine($"Running {Command} with {Args.Count} Argument(s).");
			if (!Commands.Keys.Contains(Command)) {
				Console.WriteLine($"\'{Command}\'is not recognized as an internal");
			} else
				Commands[Command](Args.ToArray());

		}
		public static void Run(string[] args) {
			if (args.Length == 0)
				Console.WriteLine("Usage: !run <program and arguments>\nlike: !run Python3 test.py");
			else {
				ProcessStartInfo stinfo = new ProcessStartInfo() {
					FileName = args[0],
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = false,
					Arguments = string.Join(" ", args.Skip(1)),
				};
				using (Process p = Process.Start(stinfo)) {
					var reader = p.StandardOutput;
					while (!p.HasExited) {
						while (!reader.EndOfStream) {
							string line = reader.ReadLine().Trim('\r', '\n', '\0');
							Console.WriteLine("Input: \"" + line + "\"");
						}
					}
				}
			}
		}


		static MessageBox messageBox;
		//uncompleted
		
		public static void Print(string[] args) {
			//write output of program, useful for muted clients
			try {
				if (Listener.CommandProcess.HasExited) {
					if (args.Length > 0)
						if (args[0] == "-f" || args[0] == "--full") {
							Listener.CommandProcess.StandardOutput.BaseStream.Position = 0;
						}
					if (Listener.CommandProcess != null) {
						string output = Listener.CommandProcess.StandardOutput.ReadToEnd();
						Listener.Send(output);
					} else {
						Listener.Send("Theres not any cmd to read output");
					}
				} else {
					Listener.Send("Proccess not exited.");
				}
			}catch(Exception ex) { Listener.Send(ex.Message); }
		}
		public static void Info(string[] args) {
			string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			if (args.Length > 0) {
				if (args[0] == "-l")
					Listener.Send($"Computer Name: {Environment.MachineName}\nUser name: {userName}\nCustom name: {Listener.CustomName}");
			} else
				Listener.Send($"{Environment.MachineName};{userName};{Listener.CustomName}");
		}
		public static void Close(string[] args) {
			Environment.Exit(0);
		}
		public class DirectCmd
		{
			public static Process CmdProcess = null;        //Direct cmd control process
			private static Thread Share = new Thread(new ThreadStart(CmdReader));
			public static void Run(string[] args) {
				//run an direct cmd
				if (CmdProcess == null) {
					var startInf = new ProcessStartInfo("cmd.exe") {
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardInput = true,
					};
					CmdProcess = Process.Start(startInf);
					Share.Start();
				}
			}
			private static void CmdReader() {
				while (!CmdProcess.HasExited) {
					int ch=CmdProcess.StandardOutput.Read();
					Console.Write((char)ch);
					Listener.Send(((char)ch).ToString());
				}
				Close(null);
				CmdProcess = null;
			}
			public static void Close(string[] args) {
				if (!CmdProcess.HasExited)
					CmdProcess.Kill();
				if (Share.ThreadState == System.Threading.ThreadState.Running)
					Share.Abort();
				Listener.Send("Direct cmd had been closed");
			}
		}
	}
	delegate void command(string[] args);
}
