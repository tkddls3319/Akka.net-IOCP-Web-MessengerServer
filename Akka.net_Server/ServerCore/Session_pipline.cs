
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession_pipline : Session_pipline
    {
        public static readonly int HeaderSize = 2;

        protected sealed override int OnReceive(ReadOnlySequence<byte> buffer)
        {
            long consumed = 0;
            var remaining = buffer;

            while (true)
            {
                if (remaining.Length < HeaderSize)
                    break;

                ushort packetSize = ReadUInt16LittleEndian(remaining);
                if (packetSize < HeaderSize)
                    throw new Exception($"Invalid packet size: {packetSize}");

                if (remaining.Length < packetSize)
                    break;

                var packetSeq = remaining.Slice(0, packetSize);

                if (packetSeq.IsSingleSegment && MemoryMarshal.TryGetArray(packetSeq.First, out ArraySegment<byte> seg))
                {
                    OnRecvedPacket(new ArraySegment<byte>(seg.Array!, seg.Offset, packetSize));
                }
                else
                {
                    byte[] arr = packetSeq.ToArray();
                    OnRecvedPacket(new ArraySegment<byte>(arr, 0, arr.Length));
                }

                remaining = remaining.Slice(packetSize);
                consumed += packetSize;
            }

            return (int)consumed;
        }

        private static ushort ReadUInt16LittleEndian(ReadOnlySequence<byte> seq)
        {
            if (seq.FirstSpan.Length >= 2)
                return BinaryPrimitives.ReadUInt16LittleEndian(seq.FirstSpan.Slice(0, 2));

            Span<byte> tmp = stackalloc byte[2];
            seq.Slice(0, 2).CopyTo(tmp);
            return BinaryPrimitives.ReadUInt16LittleEndian(tmp);
        }

        public abstract void OnRecvedPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session_pipline
    {
        private Socket _socket;

        private readonly Pipe _pipe = new Pipe(new PipeOptions(
            pauseWriterThreshold: 1024 * 64,
            resumeWriterThreshold: 1024 * 32,
            minimumSegmentSize: 4096 * 2  // Larger buffer for less fragmentation
        ));

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        // Bounded queue: prevents unlimited memory growth
        private readonly Channel<ReadOnlyMemory<byte>> _sendChannel =
            Channel.CreateBounded<ReadOnlyMemory<byte>>(new BoundedChannelOptions(8192)
            {
                FullMode = BoundedChannelFullMode.DropWrite, // Or: Wait / DropOldest / DropNewest
                SingleReader = true,
                SingleWriter = false
            });

        private int _closed;

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract void OnSended(int numOfByte);

        protected abstract int OnReceive(ReadOnlySequence<byte> buffer);

        public void Start(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));

            OnConnected(_socket.RemoteEndPoint!);

            _ = Task.Run(ReceiveLoopAsync);
            _ = Task.Run(ProcessLoopAsync);
            _ = Task.Run(SendLoopAsync);
        }

        private async Task ReceiveLoopAsync()
        {
            Exception error = null;

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    const int minimumBufferSize = 8192;

                    Memory<byte> memory = _pipe.Writer.GetMemory(minimumBufferSize);
                    int bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None, _cts.Token).ConfigureAwait(false);

                    if (bytesRead == 0)
                        break;

                    _pipe.Writer.Advance(bytesRead);

                    var result = await _pipe.Writer.FlushAsync(_cts.Token).ConfigureAwait(false);
                    if (result.IsCompleted)
                        break;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { error = ex; }
            finally
            {
                await _pipe.Writer.CompleteAsync(error).ConfigureAwait(false);
                Close();
            }
        }

        private async Task ProcessLoopAsync()
        {
            Exception error = null;

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var result = await _pipe.Reader.ReadAsync(_cts.Token).ConfigureAwait(false);
                    var buffer = result.Buffer;

                    if (buffer.Length == 0 && result.IsCompleted)
                        break;

                    while (true)
                    {
                        int consumed = OnReceive(buffer);
                        if (consumed == 0)
                            break;

                        if (consumed < 0 || consumed > buffer.Length)
                            throw new InvalidOperationException("Invalid consumed length");

                        buffer = buffer.Slice(consumed);
                    }

                    _pipe.Reader.AdvanceTo(buffer.Start, buffer.End);

                    if (result.IsCompleted && buffer.Length == 0)
                        break;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { error = ex; }
            finally
            {
                await _pipe.Reader.CompleteAsync(error).ConfigureAwait(false);
                Close();
            }
        }

        public void Send(ReadOnlyMemory<byte> buffer)
        {
            if (_cts.IsCancellationRequested)
                return;

            var writer = _sendChannel.Writer;
            writer.TryWrite(buffer); // If full: drops or waits depending on FullMode
        }

        private async Task SendLoopAsync()
        {
            Exception error = null;

            try
            {
                var reader = _sendChannel.Reader;

                while (await reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var buffer))
                    {
                        int totalSent = 0;
                        while (totalSent < buffer.Length)
                        {
                            int sent = await _socket.SendAsync(buffer.Slice(totalSent), SocketFlags.None, _cts.Token).ConfigureAwait(false);
                            if (sent == 0)
                                throw new SocketException((int)SocketError.ConnectionReset);

                            totalSent += sent;
                        }

                        OnSended(buffer.Length);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { error = ex; }
            finally
            {
                Close();
            }
        }

        private void Close()
        {
            if (Interlocked.Exchange(ref _closed, 1) == 1)
                return;

            try { _cts.Cancel(); } catch { }
            try { _sendChannel.Writer.TryComplete(); } catch { }

            EndPoint remote = null;
            try { remote = _socket?.RemoteEndPoint; } catch { }

            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Close();
            }
            catch { }

            if (remote != null)
            {
                try { OnDisconnected(remote); } catch { }
            }
        }
    }
}

