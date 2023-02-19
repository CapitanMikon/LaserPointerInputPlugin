using System;
using UnityEngine;

public class VirtualMouse : MonoBehaviour
{
    public static VirtualMouse instance;

    [SerializeField] private GameObject cameraFeed;
    public int maxWidth;
    public int maxHeight;

    private int x;
    private int y;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            DebugText.instance.ResetText();
            MouseOperations.SetCursorPosition(Mathf.Abs(0),Mathf.Abs(0));
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp | MouseOperations.MouseEventFlags.LeftDown);
        }
    }

    public void HideCameraFeed()
    {
        cameraFeed.SetActive(false);
    }
    
    public void ShowCameraFeed()
    {
        cameraFeed.SetActive(true);
    }

    public void SetMouseClickPositions(float x, float y)
    {
        this.x = Convert.ToInt32(x);
        this.y = Convert.ToInt32(y);
    }

    public void PerformLeftMouseClick()
    {
        DebugText.instance.ResetText();
        MouseOperations.SetCursorPosition(Mathf.Abs(0-x),Mathf.Abs(maxHeight-y));
        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp | MouseOperations.MouseEventFlags.LeftDown);
        DebugText.instance.AddText($"Mouse PRESSED at: {MouseOperations.GetCursorPosition().X},{MouseOperations.GetCursorPosition().Y}");

    }

}
