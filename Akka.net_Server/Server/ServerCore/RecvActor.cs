using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.ServerCore
{
    public class RecvActor : ReceiveActor
    {
        private readonly Socket _socket;
        private readonly RecvBuffer _recvBuffer = new RecvBuffer(1024);
        private readonly SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        public static readonly int HeaderSize = 2;

        public RecvActor(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += RecvArgs_Completed;

            RegisterRecv();
        }
        private void RegisterRecv()
        {
            try
            {
                _recvBuffer.OnClear();
                ArraySegment<byte> recvSegment = _recvBuffer.WriteSegment;
                _recvArgs.SetBuffer(recvSegment.Array, recvSegment.Offset, recvSegment.Count);

                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                {
                    RecvArgs_Completed(null, _recvArgs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Context.Stop(Self);
            }
        }
        private void RecvArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (_recvArgs.SocketError == SocketError.Success && _recvArgs.BytesTransferred > 0)
            {
                try
                {
                    if (_recvBuffer.OnWrite(_recvArgs.BytesTransferred) == false)
                    {
                        Context.Stop(Self);
                        return;
                    }

                    int processLen = OnRecved(_recvBuffer.ReadSegment);
                    if (_recvBuffer.DataSize < processLen)
                    {
                        Context.Stop(Self);
                        return;
                    }

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Context.Stop(Self);
                        return;
                    }
                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Context.Stop(Self);
                }
            }
            else
            {
                Console.WriteLine("Receive failed: " + _recvArgs.SocketError);
                Context.Stop(Self);
            }
        }
        public int OnRecved(ArraySegment<byte> buffer)
        {
            Console.WriteLine(Encoding.UTF8.GetString(buffer.Array));
            int processLen = 0;
            while (true)
            {
                if (buffer.Count < HeaderSize)
                    break;

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                //OnRecvedPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                processLen += dataSize;

                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            return processLen;
        }
    }
}
