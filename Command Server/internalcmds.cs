using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Command_Server {
    class internalcmds {
        public static Dictionary<string, command> Commands = new Dictionary<string, command>();
		public static Dictionary<string, string> Discribtions = new Dictionary<string, string>();

		public internalcmds() {
            Commands["run"] = new command(Run);
			Discribtions["run"] = "Run a program or script and get commands from standard output.\nUsage: run [program] [arguments]\n";

			Commands["help"] = new command(Help);
			Discribtions["help"] = "Show discribtion of a command or all commands\nUsage: help [command] or help\n";

			Commands["send"] = new command(Send);
			Discribtions["send"] = "Send a command to a specified client\nUsage: send [client] [command]\n";

			Commands["disconnect"] = new command(Disconnect);
			Discribtions["disconnect"] = "disconnect one or more client(s)\nUsage: disconnect [client1] [client2] ...\n";

			Commands["mute"] = new command(MuteClient);
			Discribtions["mute"] = "mute one or more or all client(s)\nUsage: mute [client1] [client2] ... or mute all\n";

			Commands["unmute"] = new command(MuteClient);
			Discribtions["unmute"] = "unmute one or more or all client(s)\nUsage: unmute [client1] [client2] ... or unmute all\n";

			Commands["clients"] = new command(Clients);
			Discribtions["clients"] = "show all connected clients name\n";

			Commands["directcmd"] = new command(DirectCmd.Run);
			Discribtions["directcmd"] = "Run a remote cmd on a client\n";
			Discribtions["closecmd"] = "Close a remote cmd on a client.\n";

			Commands["setpref"] = new command(SetPrefix);
			Discribtions["setpref"] = "set some test before input\n";
		}
        public void Do(string CommandLine) {
			string Command = CommandLine;
			var M = Regex.Matches(CommandLine, Properties.Resources.regexFormat);
			List<string> Args = new List<string>();
			if (M.Count >= 2) {     //may be there is argument
				Command = M[0].Value;
				for (int i = 1; i < M.Count; i++) {
					Args.Add(M[i].Value);
				}
				Args.Remove(" ");
				Args.Remove("");
			}
			//^ there was too many special chars in regex format so I saved it in resources for easier access and edit.
			//regex example -> https://regexr.com/4obll recommend to use external browser.

			//Console.WriteLine($"Running {Command} with {Args.Count} Argument(s).");
			if (!Commands.Keys.Contains(Command)) {
				Console.WriteLine($"\'{Command}\' is not recognized as an internal command");
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
                    Arguments = string.Join(" ",args.Skip(1)),
                };
                using (Process p = Process.Start(stinfo)) {
                    var reader = p.StandardOutput;
                    while (!p.HasExited) {
                        while (!reader.EndOfStream) {
                            string line=reader.ReadLine().Trim('\r', '\n', '\0');
                            Console.WriteLine("Input: \""+line+"\"");
                        }
                    }
                }
            }
        }
		public static void Help(string[] args) {
			if (args.Length >= 1) {
				if (Commands.ContainsKey(args[0]))
					Console.WriteLine(Discribtions[args[0]]);
				else
					Console.WriteLine($"\'{args[0]}\' is not recognized as an internal command");
			} else {
				foreach(var entery in Discribtions)
					Console.WriteLine($"{entery.Key}\t{entery.Value}");
			}
		}
		public static void Send(string[] args) {
			if (args.Length >= 2) {
				if (Program.Clients.ContainsKey(args[0])) {
					string text = string.Join(" ", args, 1,args.Length-1);
					Program.Clients[args[0]].Send(text);
				} else {
					Console.WriteLine("No such client found");
				}
			} else {
				Console.WriteLine("Bad command usage.");
			}
		}
		public static void Disconnect(string[] args) {
			if (args.Length >= 1) {
				foreach (string client in args) {
					if (Program.Clients.ContainsKey(client)) {
						Program.Clients[client].Disconnect(DisconnectReason.manual);
						Console.WriteLine($"Disconnected from \"{client}\"");
					} else {
						Console.WriteLine($"No such client found that named as\"{client}\".");
					}
				}
			} else {
				Console.WriteLine("Bad command usage.");
			}
		}
		public static void SetFlag(string[] args) {

		}
		public static void SetPrefix(string [] args) {
			if (args.Length == 0)
				Program.DefPrefix = "";
			else
				Program.DefPrefix = string.Join(" ", args);
		}
		public static void MuteClient(string[] args) {
			if (args.Length == 0) {
				if (args[0] == "all") {
					foreach (var c in Program.Clients.Values)
						c.Muted = true;
					Console.WriteLine("Muted all clients.");
				}
			} else if (args.Length > 1) {
				foreach (string client in args) {
					if (Program.Clients.ContainsKey(client)) {
						Program.Clients[client].Muted = true;
						Console.WriteLine($"muted \"{client}\"");
					} else {
						Console.WriteLine($"No such client found that named as\"{client}\".");
					}
				}
			} else {
				Console.WriteLine("Bad command usage.");
			}
		}
		public static void UnMuteClient(string[] args) {
			if (args.Length == 0) {
				if (args[0] == "all") {
					foreach (var c in Program.Clients.Values)
						c.Muted = false;
					Console.WriteLine("Muted all clients.");
				}
			} else if (args.Length > 1) {
				foreach (string client in args) {
					if (Program.Clients.ContainsKey(client)) {
						Program.Clients[client].Muted = false;
						Console.WriteLine($"muted \"{client}\"");
					} else {
						Console.WriteLine($"No such client found that named as\"{client}\".");
					}
				}
			} else {
				Console.WriteLine("Bad command usage.");
			}
		}
		public static void Clients(string [] args) => Console.WriteLine(string.Join(", ", Program.Clients.Keys));
		public class DirectCmd
		{
			private static Thread Listener = new Thread(new ParameterizedThreadStart(CmdListener));
			private static ClientManager Client;
			public static void Run(string[] args) {
				//run a remote cmd on client
				if (args.Length == 1) {
					if (Program.Clients.ContainsKey(args[0])) {
						Client = Program.Clients[args[0]];
						Program.readClients = false;
						Client.Send("<$>directcmd");						//sey client to run direct cmd protocol
						Listener.Start(Client);
						Commands["closecmd"] = new command(Close);
						Commands.Remove("directcmd");
					} else {
						Console.WriteLine($"No such client found that named as\"{args[0]}\".");
					}
				} else {
					Console.WriteLine("Bad command usage.");
				}
			}
			public static void Close(string[] args) {
				Listener.Abort();
				Client.Send("<$>closecmd");
				ClientManager.ReadClients = true;
				Commands["directcmd"] = new command(Run);
				Commands.Remove("closecmd");
				Console.WriteLine("Closed directcmd");
			}
			private static void CmdListener(object param) {
				var Client = param as ClientManager;
				while (Client.ClientSocket.Connected) {
					if (Client.Read(true, out string Text))
						Console.Write(Text);
				}
			}
		}
		
	}
    delegate void command(string[] args);
}
