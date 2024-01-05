// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static readonly Dictionary<int, PlayerManager> Players = new();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

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

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        var _player = Instantiate(_id == Client.instance.myId ? localPlayerPrefab : playerPrefab, _position, _rotation);

        _player.GetComponent<PlayerManager>().Initialize(_id, _username);
        Players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}