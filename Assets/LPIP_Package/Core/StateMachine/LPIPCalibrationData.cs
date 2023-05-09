using UnityEngine;

public struct LPIPCalibrationData
{
    public Vector2[] real;
    public Vector2[] ideal;
}

public struct Bound {
    public int minX;
    public int minY;
    public int maxX;
    public int maxY;
    public int detected;
};

public struct CameraData
{
    public int CAMERA_WIDTH;
    public int CAMERA_HEIGHT;
}

public struct WindowData
{
    public int GAME_WINDOW_WIDTH;
    public int GAME_WINDOW_HEIGHT;
    public float GAME_WINDOW_FACTORX;
    public float GAME_WINDOW_FACTORY;
}