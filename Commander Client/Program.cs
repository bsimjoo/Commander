using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
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
		static void Main() {
			//running a windows service
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new Service1()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
