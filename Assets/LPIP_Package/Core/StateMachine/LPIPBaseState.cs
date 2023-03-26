using UnityEngine;

public abstract class LPIPBaseState
{
    public abstract void EnterState(LPIPStateManager lpipStateManager);
    public abstract void UpdateState();
    //public abstract void ExitState();
}
