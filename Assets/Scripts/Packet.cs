// Emilian Wilczek 2003458
// Based on packet file provided for Unity C# Networking tutorial by Tom Weiland

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPosition,
    playerRotation,
    playerDisconnected,
    playerHealth,
    playerRespawned
}

public enum ClientPackets
{
    welcomeReceived = 1,
    playerMovement,
    playerShoot
}

public class Packet : IDisposable
{
    private List<byte> _buffer;

    private bool _disposed;
    private byte[] _readableBuffer;
    private int _readPos;
    
    public Packet()
    {
        _buffer = new List<byte>();
        _readPos = 0;
    }
    
    public Packet(int _id)
    {
        _buffer = new List<byte>();
        _readPos = 0;

        Write(_id);
    }
    
    public Packet(byte[] _data)
    {
        _buffer = new List<byte>();
        _readPos = 0;

        SetBytes(_data);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool _disposing)
    {
        if (!_disposed)
        {
            if (_disposing)
            {
                _buffer = null;
                _readableBuffer = null;
                _readPos = 0;
            }

            _disposed = true;
        }
    }

    #region Functions
    
    public void SetBytes(byte[] _data)
    {
        Write(_data);
        _readableBuffer = _buffer.ToArray();
    }
    
    public void WriteLength()
    {
        _buffer.InsertRange(0,
            BitConverter.GetBytes(_buffer.Count));
    }
    
    public void InsertInt(int _value)
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(_value));
    }
    
    public byte[] ToArray()
    {
        _readableBuffer = _buffer.ToArray();
        return _readableBuffer;
    }
    
    public int Length()
    {
        return _buffer.Count;
    }
    
    public int UnreadLength()
    {
        return Length() - _readPos;
    }
    
    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            _buffer.Clear();
            _readableBuffer = null;
            _readPos = 0;
        }
        else
        {
            _readPos -= 4;
        }
    }

    #endregion

    #region Write Data
    
    public void Write(byte _value)
    {
        _buffer.Add(_value);
    }

    private void Write(byte[] _value)
    {
        _buffer.AddRange(_value);
    }
    
    public void Write(short _value)
    {
        _buffer.AddRange(BitConverter.GetBytes(_value));
    }
    
    public void Write(int _value)
    {
        _buffer.AddRange(BitConverter.GetBytes(_value));
    }
    
    public void Write(long _value)
    {
        _buffer.AddRange(BitConverter.GetBytes(_value));
    }

    private void Write(float _value)
    {
        _buffer.AddRange(BitConverter.GetBytes(_value));
    }
    
    public void Write(bool _value)
    {
        _buffer.AddRange(BitConverter.GetBytes(_value));
    }
    
    public void Write(string _value)
    {
        Write(_value.Length);
        _buffer.AddRange(Encoding.ASCII.GetBytes(_value));
    }
    
    public void Write(Vector3 _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
    }
    
    public void Write(Quaternion _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
        Write(_value.w);
    }

    #endregion

    #region Read Data

    public byte ReadByte(bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'byte'!");
        var _value = _readableBuffer[_readPos];
        if (_moveReadPos) _readPos += 1;
        return _value;
    }
    
    public byte[] ReadBytes(int _length, bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'byte[]'!");
        var _value = _buffer.GetRange(_readPos, _length).ToArray();
        if (_moveReadPos) _readPos += _length;
        return _value;
    }
    
    public short ReadShort(bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'short'!");
        var _value = BitConverter.ToInt16(_readableBuffer, _readPos);
        if (_moveReadPos) _readPos += 2;
        return _value;
    }
    
    public int ReadInt(bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'int'!");
        var _value = BitConverter.ToInt32(_readableBuffer, _readPos);
        if (_moveReadPos) _readPos += 4;
        return _value;

    }
    
    public long ReadLong(bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'long'!");
        var _value = BitConverter.ToInt64(_readableBuffer, _readPos);
        if (_moveReadPos) _readPos += 8;
        return _value;
    }
    
    public float ReadFloat(bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'float'!");
        var _value = BitConverter.ToSingle(_readableBuffer, _readPos);
        if (_moveReadPos) _readPos += 4;
        return _value;
    }
    
    public bool ReadBool(bool _moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'bool'!");
        var _value = BitConverter.ToBoolean(_readableBuffer, _readPos);
        if (_moveReadPos) _readPos += 1;
        return _value;
    }
    
    public string ReadString(bool _moveReadPos = true)
    {
        try
        {
            var _length = ReadInt();
            var _value = Encoding.ASCII.GetString(_readableBuffer, _readPos, _length);
            if (_moveReadPos && _value.Length > 0) _readPos += _length;
            return _value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }
    
    public Vector3 ReadVector3(bool _moveReadPos = true)
    {
        return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
    }
    
    public Quaternion ReadQuaternion(bool _moveReadPos = true)
    {
        return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos),
            ReadFloat(_moveReadPos));
    }

    #endregion
}