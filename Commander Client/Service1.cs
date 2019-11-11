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
			main();
		}

		protected override void OnStop() {
		}
		public static void main() {
			using (StreamReader Reader = new StreamReader(new FileStream(@"C:\Commander\Config.cfg", FileMode.Open))) {
				if (Reader.ReadLine().ToLower() == "[commanderconfig]") {
					do {
						string line = Reader.ReadLine();
						string[] linePart = line.Split(';');
						switch (linePart[0].ToLower()) {
						case "ip":
							Listener.ipAddr = linePart[1];
							break;
						case "port":
							Listener.port = linePart[1];
							break;
						case "customname":
							Listener.CustomName = linePart[1];
							break;
						default:
							Program.logger.Log(logType.Warning, "inavald config file attribute");
							break;
						}
					} while (!Reader.EndOfStream);
					Console.WriteLine("config:\nserver addres:{0}:{1}\ncustom name:{2}", Listener.ipAddr, Listener.port, Listener.CustomName);
				} else {
					Program.logger.Log(logType.Error, "inavald config file");
				}
			}
			Listener.Run();
		}
	}
}
