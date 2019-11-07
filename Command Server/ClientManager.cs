using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
namespace Command_Server {
    class ClientManager {
        public Socket ClientSocket = null;
        public int WaitSec { get; set; } = 30;
        public string[] ClientInfo=new string[3];
        public ClientManager(Socket cs) {
            ClientSocket = cs;
            Send("<$>info");
            string Text = Read(true);
            if (Text != "") {
                string[] Parts = Text.Split(';');           //ComputerName;UserName;CostumName
                Array.Copy(Parts, ClientInfo, Parts.Length);
            } else {
                Console.WriteLine("Request timed out. waited for {0}s", WaitSec);
                Disconnect(DisconnectReason.timedOut);
            }
            Thread th = new Thread(new ParameterizedThreadStart(CheckClient));
            th.Start(this);
        }
        private static void CheckClient(object client) {
            ClientManager cl = client as ClientManager;
            while (cl.ClientSocket.Connected) { }
            cl.Disconnect(DisconnectReason.clientClosed);
        }
        public delegate void DisconnectEventHandler(ClientManager client,DisconnectReason r);
        public event DisconnectEventHandler Disconnected;
        public void Disconnect (DisconnectReason r){
            if (r == DisconnectReason.internalError || r == DisconnectReason.manual)
                Send("<$>close");
            else
                Send("<$>disconnected");
            ClientSocket.Close();
            Disconnected(this,r);
        }
        public string Read(bool Wait) {
            string Text = "";
            var t = DateTime.Now;
            while (Wait && (t - DateTime.Now) <= new TimeSpan(0, 0, WaitSec)) {
                while (true) {
                    try {
                        byte[] buffer = new byte[1024];
                        int count = ClientSocket.Receive(buffer);
                        Text += Encoding.ASCII.GetString(buffer, 0, count);
                    }catch(Exception ex) { Program.Log(Program.logType.Error, $"EXCEPTION IN CLIENT/READ: {ex.Message}"); }
                    if (Text.LastIndexOf("<$eof>") > -1)
                        break;
                }
                if (Text != "")
                    break;
            }
            Text.Replace("<$eof>", "");
            return Text;
        }
        public void Send(string Text) {
            Text += "<$eof>";
            byte[] buffer = Encoding.ASCII.GetBytes(Text);
            try {
                ClientSocket.Send(buffer);
            } catch (Exception ex) { Program.Log(Program.logType.Error, $"EXCEPTION IN CLIENT/SEND: {ex.Message}"); }
            
        }
        public enum DisconnectReason {
            timedOut,manual,clientClosed,internalError
        }
    }
}
