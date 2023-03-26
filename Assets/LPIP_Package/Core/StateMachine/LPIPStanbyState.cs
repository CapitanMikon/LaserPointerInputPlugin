using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPIPStanbyState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;
    
    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("LPIP currentstate = {LPIPStanbyState}");
        _lpipCoreManager = lpipCoreManager;
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
        _lpipCoreManager.SwitchState(_lpipCoreManager.InitializationStateState);
    }
}
