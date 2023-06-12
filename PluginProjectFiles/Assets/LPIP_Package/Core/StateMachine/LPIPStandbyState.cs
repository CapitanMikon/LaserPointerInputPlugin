using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPIPStandbyState : LPIPBaseState
{
    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        //Debug.Log("Entered state {LPIPStandbyState}");
        LpipCoreManager = lpipCoreManager;
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        //Debug.Log("Leaving state {LPIPStandbyState}");
    }
}
