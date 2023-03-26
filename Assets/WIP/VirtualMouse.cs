using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualMouse : MonoBehaviour
{
    public static VirtualMouse instance;

    [SerializeField] private GameObject cameraFeed;
    
    private float x;
    private float y;
    
    private List<RaycastResult> guiRaycastResults;
    private PointerEventData pointerEventData;

    void Awake()
    {
        guiRaycastResults = new List<RaycastResult>();
            
        if (instance == null)
        {
            instance = this;
        }
    }

    public void HideCameraFeed()
    {
        Debug.Log("CameraFeed OFF");
        cameraFeed.SetActive(false);
    }
    
    public void ShowCameraFeed()
    {
        Debug.Log("CameraFeed ON");
        cameraFeed.SetActive(true);
    }

    public void SetMouseClickPositions(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public void PerformLeftMouseClick()
    {
        //Set the Pointer Event Position
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = new Vector2(x,y);
 
        //list of Raycast Results
        guiRaycastResults.Clear();
        
        //Raycast using the Graphics Raycaster and mouse click position
        EventSystem.current.RaycastAll(pointerEventData, guiRaycastResults);

        //UI has higher priority
        if (guiRaycastResults.Count > 0)
        {
            string s = "";
            foreach (var r in guiRaycastResults)
            {
                r.gameObject.TryGetComponent(out Button button);
                if (button != null)
                {
                    button.onClick?.Invoke();
                    s+= $"{r.gameObject.name}, ";
                    break;
                }
            }
            Debug.LogWarning($"Hit GUI button:[ {s} ]");
        }
        
        
        //since we want to prioritize UI buttons we wont cast ray if we hit UI button
        if (guiRaycastResults.Count == 0)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Camera.main == null !");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (hit.collider.TryGetComponent(out ILaserInteractable laserInteractable))
                {
                    laserInteractable.OnLaserClickEvent();
                }
            }
            DebugText.instance.ResetText(DebugText.DebugTextGroup.MouseClickPos);
            DebugText.instance.AddText($"Mouse PRESSED at: {x},{y}", DebugText.DebugTextGroup.MouseClickPos);
        }

    }

}
