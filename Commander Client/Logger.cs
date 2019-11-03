using System;
using System.IO;
using System.Text;

namespace CommanderClient
{
	class Logger {
		string filePath { get; set; }
		public Logger(string filePath) =>this.filePath=filePath;
		public void Log(logType type, string text) {
			text = $"{prefix} [{type.ToString().ToUpper()}-{DateTime.Now}] {text}\n";
			File.AppendAllText(filePath, text);
		}
		public string prefix { get; set; }
	}
	public enum logType
	{
		Info, Warning, Error
	}
}
