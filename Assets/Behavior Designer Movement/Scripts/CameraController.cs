using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public PerlinNoiseMapGenerator perlinNoiseMapGenerator;  //Add a reference to PerlinNoiseMapGenerator.cs.

    public float panSpeed = 20f; //How fast the camera moves.
    public float panBorderThickness = 10f;

    public Vector2 limitX = new Vector2(-10f, 50f);
    public Vector2 limitY = new Vector2(-10f, 50f);

    void Start()
    {
        if (perlinNoiseMapGenerator != null) //Instantly move the camera to the center of the map on frame 1.
        {
            float centerX = perlinNoiseMapGenerator.map_width;
            float centerY = perlinNoiseMapGenerator.map_height;
            transform.position = new Vector3(centerX, centerY, transform.position.z); //Apply the new position, but keep the camera's original Z depth (-10).
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return; //Checks if there is a keyboard connected.

        Vector3 pos = transform.position;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) //Up with W or up key.
            pos.y += (panSpeed * Time.deltaTime);
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) //Down with S or down key.
            pos.y -= (panSpeed * Time.deltaTime);
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) //Left with A or left key.
            pos.x -= (panSpeed * Time.deltaTime);
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) //Right with D or right key.
            pos.x += (panSpeed * Time.deltaTime);

        pos.x = Mathf.Clamp(pos.x, limitX.x, limitX.y);
        pos.y = Mathf.Clamp(pos.y, limitY.x, limitY.y);

        transform.position = pos;
    }
}