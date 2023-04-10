using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LPIPMouseEmulation : MonoBehaviour
{
    //private static LPIPMouseEmulation Instance;

    private List<RaycastResult> _guiRaycastResults;
    private PointerEventData _pointerEventData;

    private bool _isUIDetected;
    
    private void Awake()
    {
        _isUIDetected = false;
        _guiRaycastResults = new List<RaycastResult>();
            
        /*if (Instance == null)
        {
            Instance = this;
        }*/
    }

    private void OnEnable()
    {
        LPIPCoreManager.OnLaserHitDownDetectedEvent += EmulateLeftMouseClick;
    }

    private void OnDisable()
    {
        LPIPCoreManager.OnLaserHitDownDetectedEvent -= EmulateLeftMouseClick;
    }

    private void EmulateLeftMouseClick(Vector2 clickPosition)
    {
        //Debug.LogWarning($"Emulated LMB Click at {clickPosition}");
        
        _isUIDetected = false;
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
                //try button
                r.gameObject.TryGetComponent(out Button button);
                if (button != null)
                {
                    if (button.interactable)
                    {
                        button.onClick?.Invoke();
                        s+= $"{r.gameObject.name}";
                        _isUIDetected = true;
                        break;
                    }
                }
                //try toggle
                r.gameObject.TryGetComponent(out Toggle toggle);
                if (toggle != null)
                {
                    if (toggle.interactable)
                    {
                        toggle.isOn = !toggle.isOn;
                        s+= $"{r.gameObject.name}";
                        _isUIDetected = true;
                        break;
                    }
                }
                
            }

            if (!s.Equals(""))
            {
                Debug.LogWarning($"GUI button object: {s} was hit!");
            }
        }
        
        
        //since we want to prioritize UI buttons we wont cast ray if we hit UI button
        if (!_isUIDetected)
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
            DebugTextController.Instance.ResetText(DebugTextController.DebugTextGroup.MouseClickPos);
            DebugTextController.Instance.AppendText($"Mouse click emulated at: {clickPosition}", DebugTextController.DebugTextGroup.MouseClickPos);
        }
    }

}
