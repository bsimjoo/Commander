using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace CommanderClient
{
	public partial class Service1 : ServiceBase
	{
		public Service1() {
			InitializeComponent();
		}

		protected override void OnStart(string[] args) {
			using (StreamReader Reader = new StreamReader(new FileStream(@"C:\Commander\Config.cfg", FileMode.Open))) {
				if (Reader.ReadLine().ToLower() == "[commanderconfig]") {
					string line = Reader.ReadLine();
					string[] linePart = line.Split(';');
					switch (linePart[0].ToLower()) {
					case "ip":
						CommonVar.ipAddr = linePart[1];
						break;
					case "port":
						CommonVar.port = linePart[1];
						break;
					case "customname":
						CommonVar.CustomName = linePart[1];
						break;
					case "debug":
						ProcessStartInfo st = new ProcessStartInfo() {
							FileName = "cmd.exe",
							Arguments = "/k echo off",
							RedirectStandardInput = true,
							UseShellExecute = false,
							CreateNoWindow = false,
							WindowStyle = ProcessWindowStyle.Normal
						};
						Program.logger.outputStream = Process.Start(st).StandardInput;
						Program.logger.prefix = "echo";
						break;
					default:
						Program.logger.Log(logType.Warning, "inavald config file");
						break;
					}
				} else {
					Program.logger.Log(logType.Error, "inavald config file");
				}
			}
		}

		protected override void OnStop() {
		}
	}
}
