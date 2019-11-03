using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.IO;
using System.Threading.Tasks;

namespace CommanderClient {
	
	static class Program {
		public static Logger logger = new Logger(@"c:\Commander\Log.log");
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
	static class CommonVar
	{
		public static string ipAddr { get; set; }
		public static string port { get; set; }
		public static string CustomName { get; set; }
		public static StreamWriter CmdStandardInput { get; set; }
		public static StreamReader CmdStandardOutput { get; set; }
	}
}
