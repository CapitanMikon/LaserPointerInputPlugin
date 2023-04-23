using UnityEngine;

public abstract class LPIPBaseState : MonoBehaviour
{
    public abstract void EnterState(LPIPCoreManager lpipCoreManager);
    public abstract void UpdateState();
    public abstract void ExitState();
}
