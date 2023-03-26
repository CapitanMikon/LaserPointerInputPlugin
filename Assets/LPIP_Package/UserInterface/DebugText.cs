using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    public static DebugText instance;

    [SerializeField] private TextMeshProUGUI textResolution;
    [SerializeField] private TextMeshProUGUI textMouseClick;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()    
    {
        ResetText(DebugTextGroup.Everything);
    }

    public void ResetText(DebugTextGroup type)
    {
        switch (type)
        {
            case DebugTextGroup.Resolution:
                textResolution.text = "";
                break;
            case DebugTextGroup.MouseClickPos:
                textMouseClick.text = "";
                break;
            case DebugTextGroup.Everything:
                textResolution.text = "";
                textMouseClick.text = "";
                break;
        }
    }

    public void AddText(string message, DebugTextGroup type)
    {
        switch (type)
        {
            case DebugTextGroup.Resolution:
                textResolution.text += message;
                break;
            case DebugTextGroup.MouseClickPos:
                textMouseClick.text += message;
                break;
            case DebugTextGroup.Everything:
                textResolution.text += message;
                textMouseClick.text += message;
                break;
        }
    }
    public enum DebugTextGroup
    {
        Resolution,
        MouseClickPos,
        Everything
    }
}
