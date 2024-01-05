// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        var _msg = _packet.ReadString();
        var _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        var _id = _packet.ReadInt();
        var _username = _packet.ReadString();
        var _position = _packet.ReadVector3();
        var _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        var _id = _packet.ReadInt();
        var _position = _packet.ReadVector3();

        GameManager.Players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        var _id = _packet.ReadInt();
        var _rotation = _packet.ReadQuaternion();

        GameManager.Players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        var _id = _packet.ReadInt();

        Destroy(GameManager.Players[_id].gameObject);
        GameManager.Players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        var _id = _packet.ReadInt();
        var _health = _packet.ReadFloat();

        GameManager.Players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        var _id = _packet.ReadInt();

        GameManager.Players[_id].Respawn();
    }
}