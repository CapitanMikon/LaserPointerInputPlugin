using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    public static DebugText instance;

    [SerializeField] private TextMeshProUGUI text;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        ResetText();
    }

    public void ResetText()
    {
        text.text = "";
    }

    public void AddText(string message)
    {
        text.text += message;
    }
}
