using System;
using UnityEngine;

public class LaserPointerInputSetUp : LaserPointerInputBaseState
{
    private LaserPointerInputManager _laserPointerInputManager;
    
    
    //scaling from game to camera
    private int GAME_WINDOW_WIDTH = -1;
    private int GAME_WINDOW_HEIGHT = -1;
    private float GAME_WINDOW_FACTORX = -1;
    private float GAME_WINDOW_FACTORY = -1;
    
    private int CAMERA_WIDTH = -1;
    private int CAMERA_HEIGHT = -1;
    
    public override void EnterState(LaserPointerInputManager laserPointerInputManager)
    {
        Debug.Log("STATE: SetUp");
        _laserPointerInputManager = laserPointerInputManager;
        
        WebCamDevice[] devices = WebCamTexture.devices;
        
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log("camera <" + devices[i].name + "> detected");
        }

        if (devices.Length > 0)
        {
            _laserPointerInputManager.webCamTexture = new WebCamTexture(devices[0].name);
            _laserPointerInputManager.webcamImageHolder.texture = _laserPointerInputManager.webCamTexture;
            ConfigureWebcam(1920, 1080, 30);
            //ConfigureWebcam(1280, 720, 30);
            _laserPointerInputManager.webCamTexture.Play();
        }
        LogWebcamInfo();
        
        //get real camera/game width and height
        CAMERA_WIDTH = _laserPointerInputManager.webCamTexture.requestedWidth;
        CAMERA_HEIGHT = _laserPointerInputManager.webCamTexture.requestedHeight;
        
        //GAME_WINDOW_HEIGHT = Display.main.systemHeight;
        GAME_WINDOW_HEIGHT = Display.displays[_laserPointerInputManager.PROJECTOR_DISPLAY_ID].systemHeight;
        //GAME_WINDOW_WIDTH = Display.main.systemWidth;
        GAME_WINDOW_WIDTH = Display.displays[_laserPointerInputManager.PROJECTOR_DISPLAY_ID].systemWidth;
        
        VirtualMouse.instance.maxWidth = GAME_WINDOW_WIDTH;
        VirtualMouse.instance.maxHeight = GAME_WINDOW_HEIGHT;
        
        Debug.LogWarning($"Window res: {GAME_WINDOW_WIDTH}x{GAME_WINDOW_HEIGHT}");
        DebugText.instance.AddText($"\nWindow res: {GAME_WINDOW_WIDTH}x{GAME_WINDOW_HEIGHT}", DebugText.DebugTextGroup.Resolution);
        DebugText.instance.AddText($"\nScreen res: {Screen.currentResolution}", DebugText.DebugTextGroup.Resolution);

        GAME_WINDOW_FACTORX =  GAME_WINDOW_WIDTH / Convert.ToSingle(CAMERA_WIDTH);
        GAME_WINDOW_FACTORY =  GAME_WINDOW_HEIGHT / Convert.ToSingle(CAMERA_HEIGHT);
        Debug.LogWarning($"Window res factor is: {GAME_WINDOW_FACTORX}:{GAME_WINDOW_FACTORY}");

        SaveData();
        _laserPointerInputManager.SwitchState(_laserPointerInputManager.calibrationState);
    }

    public override void UpdateState()
    {
    }
    
    void ConfigureWebcam(int width, int height, int fps)
    {
        _laserPointerInputManager.webCamTexture.requestedFPS = fps;
        _laserPointerInputManager.webCamTexture.requestedWidth = width;
        _laserPointerInputManager.webCamTexture.requestedHeight = height;
    }
    
    void LogWebcamInfo()
    {
        Debug.Log($"\nCurrent camera configuration:\nFPS: {_laserPointerInputManager.webCamTexture.requestedFPS}\nRes: {_laserPointerInputManager.webCamTexture.width}x{_laserPointerInputManager.webCamTexture.height}");
        DebugText.instance.AddText(
            $"Current camera configuration:\nFPS: {_laserPointerInputManager.webCamTexture.requestedFPS}\nRes: {_laserPointerInputManager.webCamTexture.width}x{_laserPointerInputManager.webCamTexture.height}", DebugText.DebugTextGroup.Resolution);
    }

    void SaveData()
    {
        WindowData windowData = new WindowData
        {
            GAME_WINDOW_WIDTH = GAME_WINDOW_WIDTH,
            GAME_WINDOW_HEIGHT = GAME_WINDOW_HEIGHT,
            GAME_WINDOW_FACTORX = GAME_WINDOW_FACTORX,
            GAME_WINDOW_FACTORY = GAME_WINDOW_FACTORY
        };

        _laserPointerInputManager.SetWindowData(windowData);

        CameraData cameraData = new CameraData
        {
            CAMERA_WIDTH = CAMERA_WIDTH,
            CAMERA_HEIGHT = CAMERA_HEIGHT
        };

        _laserPointerInputManager.SetCameraData(cameraData);
    }
}
