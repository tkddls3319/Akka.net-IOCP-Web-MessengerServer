using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    public class ClientP
    {
        static void Main(string[] args)
        {
            //서버 보다 빨리 켜져서 Log 클러스터가 서버에 붙기 전에 켜짐 그래서 sleep 걸어놈
            if (Environment.GetEnvironmentVariable("VisualStudioEdition") != null)
            {
                Thread.Sleep(5000); // Visual Studio 환경에서만 동작
            }

            string hostName = Dns.GetHostName();

            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];

            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); }, false);

            while (true) { }
        }
    }
}
