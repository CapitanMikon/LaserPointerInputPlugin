using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualMouse : MonoBehaviour
{
    public static VirtualMouse instance;

    [SerializeField] private GameObject cameraFeed;
    public int maxWidth;
    public int maxHeight;

    private float x;
    private float y;
    
    
    //GUI raycast
    [SerializeField] private GraphicRaycaster guiRaycaster;
    private List<RaycastResult> guiRaycastResults = new List<RaycastResult>();
    private PointerEventData pointerEventData;
    [SerializeField] private EventSystem eventSystem;
    
    void Awake()
    {
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
        pointerEventData = new PointerEventData(eventSystem);
        //Set the Pointer Event Position to that of the game object
        pointerEventData.position = new Vector2(x,y);
 
        //Create a list of Raycast Results
        guiRaycastResults.Clear();
        //Raycast using the Graphics Raycaster and mouse click position
        guiRaycaster.Raycast(pointerEventData, guiRaycastResults);

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
            Debug.LogWarning($"Hit GUI:[ {s} ]");
        }
        
        
        //
        if (guiRaycastResults.Count == 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
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
