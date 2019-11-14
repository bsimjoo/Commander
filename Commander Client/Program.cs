using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace CommanderClient
{
	static class Program
	{
		public static internalcmds internalCmd = new internalcmds();
		public static Logger logger = new Logger(@"c:\Commander\Log.log");
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] arg) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//running a windows service
			if (arg.Length != 0)
				if (arg[0] == "-d") {
					Service1.main();
					while (true) { System.Threading.Thread.Sleep(100); }
				}
			runservice:
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new Service1()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
