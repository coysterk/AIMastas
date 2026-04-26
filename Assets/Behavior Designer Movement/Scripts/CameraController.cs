using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public PerlinNoiseMapGenerator perlinNoiseMapGenerator;

    //Camera Control Settings
    public float panSpeed = 20f;
    public Vector2 limitX = new Vector2(-10f, 50f);
    public Vector2 limitY = new Vector2(-10f, 50f);

    //Cinematic Zoom Intro
    public bool playIntro = true;
    public float normalZoom = 5f;
    public float introZoomSpeed = 10f;
    public float introWaitTime = 3f; //How long to wait before zooming in to play.
    private float currentWaitTimer; //Tracks the countdown
    [HideInInspector] public bool isPresenting = false;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (perlinNoiseMapGenerator != null)
        {
            float mapStartX = perlinNoiseMapGenerator.transform.position.x;
            float mapStartY = perlinNoiseMapGenerator.transform.position.y;
            float mapWidth = perlinNoiseMapGenerator.mapWidth;
            float mapHeight = perlinNoiseMapGenerator.mapHeight;

            float centerX = mapStartX + (mapWidth / 2f);
            float centerY = mapStartY + (mapHeight / 2f);

            transform.position = new Vector3(centerX, centerY, transform.position.z);

            limitX = new Vector2(mapStartX, mapStartX + mapWidth);
            limitY = new Vector2(mapStartY, mapStartY + mapHeight);

            if (playIntro)
            {
                float verticalZoom = mapHeight / 2f;
                float horizontalZoom = (mapWidth / 2f) / cam.aspect;

                cam.orthographicSize = Mathf.Max(verticalZoom, horizontalZoom) + 1f;

                currentWaitTimer = introWaitTime; //Initialize the timer
                isPresenting = true;
            }
            else
            {
                cam.orthographicSize = normalZoom;
            }
        }
        else
        {
            Debug.LogError("The Camera doesn't know where the Map is! Drag the Map object into the Camera script.");
        }
    }

    void Update()
    {
        if (isPresenting)
        {
            //Check if we are still waiting.
            if (currentWaitTimer > 0f)
            {
                currentWaitTimer -= Time.deltaTime; //Tick the timer down.
                return; //Stop here, don't zoom yet, and keep WASD locked.
            }

            //The timer hit 0! Start zooming in smoothly.
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, normalZoom, introZoomSpeed * Time.deltaTime);

            if (Mathf.Abs(cam.orthographicSize - normalZoom) < 0.01f)
            {
                cam.orthographicSize = normalZoom;
                isPresenting = false;
                Debug.Log("Zoom Intro finished. Player controls unlocked.");
            }

            return;
        }

        if (Keyboard.current == null) return;

        Vector3 pos = transform.position;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) { //Up with W key or UP arrow key.
            pos.y += panSpeed * Time.deltaTime; 
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) //Down with S key or DOWN arrow key.
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) //Left with A key or LEFT arrow key.
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) //Right with D key or RIGHT arrow key.
        {
            pos.x += panSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, limitX.x, limitX.y);
        pos.y = Mathf.Clamp(pos.y, limitY.x, limitY.y);

        transform.position = pos;
    }
}