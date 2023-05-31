using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginOpenerHelper : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            LPIPUtilityPortal.Instance.OpenUtilityMenu();
        }
    }
}
