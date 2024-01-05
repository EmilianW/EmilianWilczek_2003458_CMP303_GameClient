// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;
    public float sensitivity = 100f;
    public float clampAngle = 85f;
    private float _horizontalRotation;

    private float _verticalRotation;

    private void Start()
    {
        _verticalRotation = transform.localEulerAngles.x;
        _horizontalRotation = player.transform.eulerAngles.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursorMode();

        if (Cursor.lockState == CursorLockMode.Locked) Look();
        var _transform = transform;
        Debug.DrawRay(_transform.position, _transform.forward * 2, Color.red);
    }

    private void Look()
    {
        var _mouseVertical = -Input.GetAxis("Mouse Y");
        var _mouseHorizontal = Input.GetAxis("Mouse X");

        _verticalRotation += _mouseVertical * sensitivity * Time.deltaTime;
        _horizontalRotation += _mouseHorizontal * sensitivity * Time.deltaTime;

        _verticalRotation = Mathf.Clamp(_verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        player.transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
    }

    private static void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}