using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServerCore;

public class Listener
{
    public class GenerateSession
    {
        public Socket SessionSocket { get; set; }
        public GenerateSession(Socket sessionSocekt)
        {
            SessionSocket = sessionSocekt;
        }
    }
    Socket _listener;
    event Action<Socket> _sessionFacktory;

    public void Init(IPEndPoint endPoint, Action<Socket> sessionFacktory)
    {
        _sessionFacktory = sessionFacktory;

        _listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(endPoint);
        _listener.Listen(1000);

        SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
        acceptArgs.Completed += AcceptCompleted;

        RegisterAccept(acceptArgs);
    }

    void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;
        try
        {
            bool pending = _listener.AcceptAsync(args);

            if (pending == false)
                AcceptCompleted(null, args);
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
        }
    }

    private void AcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            _sessionFacktory.Invoke(args.AcceptSocket);
            //Session session = _sessionFacktory.Invoke();
            //session.Start(args.AcceptSocket);
            //session.OnConnected(args.AcceptSocket.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine("AcceptCopleted err");
        }
    }
}
