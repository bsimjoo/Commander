using System;
using System.IO;
using System.Text;

namespace CommanderClient
{
	class Logger {
		public StreamWriter outputStream { get; set; }
		public Logger(StreamWriter st) {
			outputStream = st;
		}
		public void Log(logType type, string text) {
			text = $"{prefix} [{type.ToString().ToUpper()}-{DateTime.Now}] {text}\n\r";
			outputStream.Write(text);
		}
		public string prefix { get; set; }
	}
	public enum logType
	{
		Info, Warning, Error
	}
}
