using System;
using System.IO;
using System.Text;

namespace CommanderClient
{
	class Logger {
		string filePath { get; set; }
		public Logger(string filePath) =>this.filePath=filePath;
		public void Log(logType type, string text) {
			text = $"[{type.ToString().ToUpper()}-{DateTime.Now}] {text}\n";
			File.AppendAllText(filePath, text);
			Console.WriteLine($"[{type.ToString().ToUpper()}] {text}");
		}
	}
	public enum logType
	{
		Info, Warning, Error
	}
}
