﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public class RecvBuffer
{
    ArraySegment<byte> _buffer;
    int _writePos = 0;
    int _readPos = 0;
    public RecvBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }

    public int DataSize { get { return _writePos - _readPos; } }
    public int FreeSize { get { return _buffer.Count - _writePos; } }

    public ArraySegment<byte> ReadSegment { get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); } }
    public ArraySegment<byte> WriteSegment { get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); } }

    public void OnClear()
    {
        int datasize = DataSize;
        if (datasize == 0)
        {
            _writePos = _readPos = 0;
        }
        else
        {
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, datasize);
            _readPos = 0;
            _writePos = datasize;
        }
    }

    public bool OnRead(int numOfBytes)
    {
        if (numOfBytes > DataSize)
            return false;

        _readPos += numOfBytes;
        return true;
    }

    public bool OnWrite(int numOfBytes)
    {
        if (numOfBytes > FreeSize)
            return false;

        _writePos += numOfBytes;
        return true;
    }

}
