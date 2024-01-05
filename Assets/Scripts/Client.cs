// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    private const int DataBufferSize = 4096;
    private static Dictionary<int, PacketHandler> _packetHandlers;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId;

    private bool _isConnected;
    public TCP tcp;
    public UDP udp;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        _isConnected = true;
        tcp.Connect();
    }

    private static void InitializeClientData()
    {
        _packetHandlers = new Dictionary<int, PacketHandler>
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned }
        };
        Debug.Log("Initialized packets.");
    }

    private void Disconnect()
    {
        if (!_isConnected) return;
        _isConnected = false;
        tcp.socket.Close();
        udp.socket.Close();

        Debug.Log("Disconnected from server.");
    }

    private delegate void PacketHandler(Packet _packet);

    public class TCP
    {
        private byte[] _receiveBuffer;
        private Packet _receivedData;
        public TcpClient socket;

        private NetworkStream _stream;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = DataBufferSize,
                SendBufferSize = DataBufferSize
            };

            _receiveBuffer = new byte[DataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected) return;

            _stream = socket.GetStream();

            _receivedData = new Packet();

            _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null) _stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                var _byteLength = _stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                var _data = new byte[_byteLength];
                Array.Copy(_receiveBuffer, _data, _byteLength);

                _receivedData.Reset(HandleData(_data));
                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            var _packetLength = 0;

            _receivedData.SetBytes(_data);

            if (_receivedData.UnreadLength() >= 4)
            {
                _packetLength = _receivedData.ReadInt();
                if (_packetLength <= 0) return true;
            }

            while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength())
            {
                var _packetBytes = _receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using var _packet = new Packet(_packetBytes);
                    var _packetId = _packet.ReadInt();
                    _packetHandlers[_packetId](_packet);
                });

                _packetLength = 0;
                if (_receivedData.UnreadLength() < 4) continue;
                _packetLength = _receivedData.ReadInt();
                if (_packetLength <= 0) return true;
            }

            return _packetLength <= 1;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        private IPEndPoint _endPoint;
        public UdpClient socket;

        public UDP()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(_endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using var _packet = new Packet();
            SendData(_packet);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                socket?.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                var _data = socket.EndReceive(_result, ref _endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private static void HandleData(byte[] _data)
        {
            using (var _packet = new Packet(_data))
            {
                var _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using var _packet = new Packet(_data);
                var _packetId = _packet.ReadInt();
                _packetHandlers[_packetId](_packet);
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            _endPoint = null;
            socket = null;
        }
    }
}