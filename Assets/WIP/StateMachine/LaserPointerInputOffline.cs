using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointerInputOffline : LaserPointerInputBaseState
{
    private LaserPointerInputManager _laserPointerInputManager;
    
    public override void EnterState(LaserPointerInputManager laserPointerInputManager)
    {
        Debug.Log("STATE: OFFLINE");
        _laserPointerInputManager = laserPointerInputManager;
    }

    public override void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCalibration();
        }
    }

    private void StartCalibration()
    {
        Debug.LogWarning("Laser input module is now online!");
        _laserPointerInputManager.SwitchState(_laserPointerInputManager.setUpState);
    }
}
