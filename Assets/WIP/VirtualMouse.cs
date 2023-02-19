using System;
using UnityEngine;

public class VirtualMouse : MonoBehaviour
{
    public static VirtualMouse instance;

    [SerializeField] private GameObject cameraFeed;
    public int maxWidth;
    public int maxHeight;

    private float x;
    private float y;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        this.x = x;
        this.y = y;
    }

    public void PerformLeftMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.collider.TryGetComponent(out ILaserInteractable laserInteractable))
            {
                laserInteractable.OnClick();
            }
        }
        DebugText.instance.ResetText(DebugText.DebugTextGroup.MouseClickPos);
        DebugText.instance.AddText($"Mouse PRESSED at: {x},{y}", DebugText.DebugTextGroup.MouseClickPos);

    }

}
