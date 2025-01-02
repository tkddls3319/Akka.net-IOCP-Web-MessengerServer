using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using static Server.SessionManagerActor;

namespace Server.ServerCore
{
    public class SendActor : ReceiveActor
    {
        private readonly Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        private readonly Socket _socket;
        private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        object _lock = new object();
        public SendActor(Socket socket)
        {
            _socket = socket;
            _sendArgs.Completed += SendArgs_Completed;

            Receive<ArraySegment<byte>>(message => SendHandle(message));
        }
        private void SendHandle(ArraySegment<byte> segmentBuffer)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(segmentBuffer);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }
        private void RegisterSend()
        {
            lock (_sendQueue)
            {
                while (_sendQueue.Count > 0)
                {
                    _pendingList.Add(_sendQueue.Dequeue());
                }
                _sendArgs.BufferList = _pendingList;

                try
                {
                    bool pending = _socket.SendAsync(_sendArgs);
                    if (pending == false)
                        SendArgs_Completed(null, _sendArgs);

                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                    Context.Stop(Self);

                }
            }
        }
        private void SendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            lock (_lock)
            {
                if (_sendArgs.SocketError == SocketError.Success && _sendArgs.BytesTransferred > 0)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();


                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                        Context.Stop(Self);
                    }
                }
                else
                {
                    //Context.Parent.Tell(new SocketErrorMessage(_sendArgs.SocketError));
                    Context.Stop(Self);
                }
            }
        }
    }
}
