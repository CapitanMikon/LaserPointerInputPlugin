using UnityEngine;

public abstract class LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;

    public LPIPCoreManager LpipCoreManager
    {
        get { return _lpipCoreManager;}
        set { _lpipCoreManager = value; }
    }

    public abstract void EnterState(LPIPCoreManager lpipCoreManager);
    public abstract void UpdateState();
    public abstract void ExitState();
}
