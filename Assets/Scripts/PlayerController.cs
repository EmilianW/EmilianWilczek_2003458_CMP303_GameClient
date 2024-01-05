// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) ClientSend.PlayerShoot(camTransform.forward);
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private static void SendInputToServer()
    {
        bool[] _inputs =
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };

        ClientSend.PlayerMovement(_inputs);
    }
}