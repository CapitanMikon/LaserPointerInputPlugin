using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPIPStanbyState : LPIPBaseState
{
    private LPIPStateManager _lpipStateManager;
    
    public override void EnterState(LPIPStateManager lpipStateManager)
    {
        Debug.Log("STATE: OFFLINE");
        _lpipStateManager = lpipStateManager;
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
        _lpipStateManager.SwitchState(_lpipStateManager.InitializationStateState);
    }
}
