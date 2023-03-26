using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LPIPMouseEmulation : MonoBehaviour
{
    public static LPIPMouseEmulation Instance;

    [SerializeField] private GameObject cameraFeed;
    
    private float x, y;
    
    private List<RaycastResult> _guiRaycastResults;
    private PointerEventData _pointerEventData;

    void Awake()
    {
        _guiRaycastResults = new List<RaycastResult>();
            
        if (Instance == null)
        {
            Instance = this;
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
        _pointerEventData = new PointerEventData(EventSystem.current);
        _pointerEventData.position = new Vector2(x,y);
 
        //list of Raycast Results
        _guiRaycastResults.Clear();
        
        //Raycast using the Graphics Raycaster and mouse click position
        EventSystem.current.RaycastAll(_pointerEventData, _guiRaycastResults);

        //UI has higher priority
        if (_guiRaycastResults.Count > 0)
        {
            string s = "";
            foreach (var r in _guiRaycastResults)
            {
                r.gameObject.TryGetComponent(out Button button);
                if (button != null)
                {
                    button.onClick?.Invoke();
                    s+= $"{r.gameObject.name}";
                    break;
                }
            }
            Debug.LogWarning($"GUI button object: {s} was hit!");
        }
        
        
        //since we want to prioritize UI buttons we wont cast ray if we hit UI button
        if (_guiRaycastResults.Count == 0)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Camera.main == null !");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (hit.collider.TryGetComponent(out LPIPIInteractable laserInteractable))
                {
                    laserInteractable.LPIPOnLaserHit();
                }
            }
            DebugText.instance.ResetText(DebugText.DebugTextGroup.MouseClickPos);
            DebugText.instance.AddText($"Mouse click emulated at: {x},{y}", DebugText.DebugTextGroup.MouseClickPos);
        }

    }

}
