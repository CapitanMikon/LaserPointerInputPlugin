using UnityEngine;

public abstract class LaserPointerInputBaseState
{
    public abstract void EnterState(LaserPointerInputManager laserPointerInputManager);
    public abstract void UpdateState();
    //void ExitState();
}
