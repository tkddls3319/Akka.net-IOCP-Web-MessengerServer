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
    Socket _listener;
    event Func<Session> _sessionFacktory;

    public void Init(IPEndPoint endPoint, Func<Session> sessionFacktory)
    {
        _listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFacktory = sessionFacktory;
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
            Session session = _sessionFacktory.Invoke();
            session.Start(args.AcceptSocket);
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine("AcceptCopleted err");
        }

    }
}
