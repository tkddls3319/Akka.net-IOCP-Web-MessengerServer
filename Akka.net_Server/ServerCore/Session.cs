using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public abstract class PacketSession : Session
{
    public static readonly int HeaderSize = 2;

    public sealed override int OnRecved(ArraySegment<byte> buffer)
    {
        int processLen = 0;

        while (true)
        {
            if (buffer.Count < HeaderSize)
                break;

            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;

            OnRecvedPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));  
            processLen += dataSize;

            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }
            return processLen;
    }
    public abstract void OnRecvedPacket(ArraySegment<byte> buffer);
}

public abstract class Session
{
    Socket _socket;
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    RecvBuffer _recvBuffer = new RecvBuffer(1024);

    Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

    int _connected = 0;
    object _lock = new object();

    public abstract void OnConnected(EndPoint endPoint);
    public abstract void OnDisconnected(EndPoint endPoint);
    public abstract  int OnRecved(ArraySegment<byte> buffer);
    public abstract void OnSended(int numOfByte);

    public void Start(Socket socket)
    {
        _socket = socket;

        _recvArgs.Completed += RecvArgs_Completed;
        _sendArgs.Completed += SendArgs_Completed;

        RegisterRecv();

    }
    public void Send(List<ArraySegment<byte>> segmentBuffers)
    {
        if (segmentBuffers.Count == 0)
            return;

        lock (_lock)
        {
            foreach (ArraySegment< byte > sendBuffer in segmentBuffers)
            {
                _sendQueue.Enqueue(sendBuffer);
            }
            if (_pendingList.Count == 0)
                RegisterSend();
        }
    }
    public void Send(ArraySegment<byte> segmentBuffer)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(segmentBuffer);
            if(_pendingList.Count == 0)
                RegisterSend();
        }
    }
    void RegisterRecv()
    {
        if (_connected == 1)
            return;

        _recvBuffer.OnClear();
        ArraySegment<byte> recvSegment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(recvSegment.Array, recvSegment.Offset, recvSegment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                RecvArgs_Completed(null, _recvArgs);
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
        }

    }
    private void RecvArgs_Completed(object? sender, SocketAsyncEventArgs args)
    {
        if(_recvArgs.SocketError == SocketError.Success && _recvArgs.BytesTransferred > 0)
        {
            try
            {
                if(_recvBuffer.OnWrite(_recvArgs.BytesTransferred) == false)
                {
                    DisConnect();
                    return;
                }

                int processLen = OnRecved(_recvBuffer.ReadSegment);
                if(_recvBuffer.DataSize < processLen)
                {
                    DisConnect();
                    return;
                }

                if(_recvBuffer.OnRead(processLen) == false)
                {
                    DisConnect();
                    return;
                }

                RegisterRecv();
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                DisConnect();
            }
        }
        else
        {
            DisConnect();
        }
    }
    void RegisterSend()
    {
        if (_connected == 1)
            return;

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
        }
    }
    private void SendArgs_Completed(object? sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (_sendArgs.SocketError == SocketError.Success && _sendArgs.BytesTransferred > 0)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    OnSended(_sendArgs.BytesTransferred);
                    if (_sendQueue.Count > 0)
                        RegisterSend();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                    DisConnect();
                }
            }
            else
            {
                DisConnect();
            }
        }
    }
    void DisConnect()
    {
        if (Interlocked.Exchange(ref _connected, 1) == 1)
            return;

        Console.WriteLine("disconnect");
        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);  
        _socket.Close();
        _sendQueue.Clear();
        _pendingList.Clear();
    }
}
