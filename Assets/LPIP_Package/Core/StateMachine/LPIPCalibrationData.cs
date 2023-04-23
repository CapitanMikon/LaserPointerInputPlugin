using UnityEngine;

public struct LPIPCalibrationData
{
    public Pair restrictionTopLeft;
    public Pair restrictionBottomRight;

    public float factorX;
    public float factorY;

    public Vector2[] real;
    public Vector2[] ideal;
}

public struct BorderPoint
{
    public double luminance;
    public int value;
}

public struct Pair
{
    public int x;
    public int y;
}

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