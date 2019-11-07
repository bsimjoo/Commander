using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommanderClient
{
	class internalcmds
	{
		public static Dictionary<string, command> Commands = new Dictionary<string, command>();
		public internalcmds() {
			Commands["run"] = new command(Run);
		}
		public void Do(string Command, string args) {
			string[] Args = new Regex(Properties.Resources.regexFormat).Split(args);
			List<string> _Args = new List<string>(Args);
			_Args.Remove(" ");
			_Args.Remove("");
			if (!Commands.Keys.Contains(Command)) {
				Console.WriteLine($"\'{Command}\'is not recognized as an internal");
			} else
				Commands[Command](_Args.ToArray());

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
	}
	delegate void command(string[] args);
}
