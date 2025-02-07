using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, bool multiTest = true, int count = 998)
        {
            if (multiTest)
            {
                //TODO : 멀티 채팅 테스트를 하려면 COUNT 개수를 변경해주세요
                Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = 10 }, i =>
                {
                    Connect(endPoint, sessionFactory);
                });

                //비동기식 방법인데 흠 별차이안나는 것 같음
                //await Parallel.ForEachAsync(
                //          Enumerable.Range(0, count),
                //          new ParallelOptions { MaxDegreeOfParallelism = 10 },
                //          async (i, _) =>
                //          {
                //              await Task.Delay(Random.Shared.Next(0, 100)); // 0~100ms 랜덤 딜레이
                //              Connect(endPoint, sessionFactory);
                //          });
            }
            else
            {
                Connect(endPoint, sessionFactory);
            }
        }

        void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
            }
        }

    }
}
