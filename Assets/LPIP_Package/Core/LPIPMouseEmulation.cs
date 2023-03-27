using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LPIPMouseEmulation : MonoBehaviour
{
    private static LPIPMouseEmulation Instance;

    private List<RaycastResult> _guiRaycastResults;
    private PointerEventData _pointerEventData;

    private bool isUIDetected;
    void Awake()
    {
        isUIDetected = false;
        _guiRaycastResults = new List<RaycastResult>();
            
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        LPIPCoreManager.OnLaserHitDetectedEvent += EmulateLeftMouseClick;
    }

    private void OnDisable()
    {
        LPIPCoreManager.OnLaserHitDetectedEvent -= EmulateLeftMouseClick;
    }

    public void EmulateLeftMouseClick(Vector2 clickPosition)
    {
        //Debug.LogWarning($"Emulated LMB Click at {clickPosition}");
        
        isUIDetected = false;
        //Set the Pointer Event Position
        _pointerEventData = new PointerEventData(EventSystem.current);
        _pointerEventData.position = clickPosition;
 
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
                    isUIDetected = true;
                    break;
                }
            }
            Debug.LogWarning($"GUI button object: {s} was hit!");
        }
        
        
        //since we want to prioritize UI buttons we wont cast ray if we hit UI button
        /*string guihitres = "";
        foreach (var c in _guiRaycastResults)
        {
            guihitres += " " + c.gameObject.name;
        }
        
        Debug.LogError($"_guiRaycastResults.Count = {_guiRaycastResults.Count} result = [{guihitres}]");*/
        if (!isUIDetected)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Camera.main == null !");
                return;
            }
            
            Ray ray = Camera.main.ScreenPointToRay(clickPosition);
            if (Physics.Raycast(ray, out var hit, 100))
            {
                if (hit.collider.TryGetComponent(out LPIPIInteractable laserInteractable))
                {
                    laserInteractable.LPIPOnLaserHit();
                }
            }
            DebugText.instance.ResetText(DebugText.DebugTextGroup.MouseClickPos);
            DebugText.instance.AddText($"Mouse click emulated at: {clickPosition}", DebugText.DebugTextGroup.MouseClickPos);
        }
    }

}
