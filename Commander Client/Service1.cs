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
		public static EventLog Log = new EventLog();
		public Service1() {
			InitializeComponent();
		}

		protected override void OnStart(string[] args) {
			/*var xmlReader = new XmlDataDocument();
			if (File.Exists(@"C:\CommanderConfig\Config.xml")) {
				xmlReader.Load(@"C:\CommanderConfig\Config.xml");
				string ip = xmlReader.GetElementById("serverip").InnerText;
				string customName = xmlReader.GetElementById("customName").InnerText;
				Log.WriteEntry($"config readed: IP:{ip} CustomName:{customName}");
			} else
				this.Stop();*/
		}

		protected override void OnStop() {
		}
	}
}
