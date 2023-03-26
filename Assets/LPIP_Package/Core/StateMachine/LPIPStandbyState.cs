using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPIPStandbyState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;
    
    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("Entered state {LPIPStandbyState}");
        _lpipCoreManager = lpipCoreManager;
    }

    public override void UpdateState()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCalibration();
        }
    }

    public override void ExitState()
    {
        Debug.Log("Leaving state {LPIPStandbyState}");
    }

    private void StartCalibration()
    {
        Debug.LogWarning("Laser input module is now online!");
        _lpipCoreManager.SwitchState(_lpipCoreManager.InitializationState);
    }
}
