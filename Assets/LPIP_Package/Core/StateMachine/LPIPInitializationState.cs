using System;
using UnityEngine;

public class LPIPInitializationState : LPIPBaseState
{
    private LPIPCoreManager _lpipCoreManager;
    
    
    //scaling from game to camera
    private int GAME_WINDOW_WIDTH = -1;
    private int GAME_WINDOW_HEIGHT = -1;
    private float GAME_WINDOW_FACTORX = -1;
    private float GAME_WINDOW_FACTORY = -1;
    
    private int CAMERA_WIDTH = -1;
    private int CAMERA_HEIGHT = -1;

    private WebCamTexture _webCamTexture;
    
    public override void EnterState(LPIPCoreManager lpipCoreManager)
    {
        Debug.Log("Entered state {LPIPInitializationState}");
        _lpipCoreManager = lpipCoreManager;
        _webCamTexture = _lpipCoreManager.WebCamTexture;
        Initialize();
        SaveData();
        //_lpipCoreManager.SwitchState(_lpipCoreManager.ManualCalibrationState);
    }

    public override void UpdateState()
    {
    }
    
    public override void ExitState()
    {
        Debug.Log("Leaving state {LPIPInitializationState}");
    }

    void Initialize()
    {
        
        CAMERA_WIDTH = _webCamTexture.width;
        CAMERA_HEIGHT = _webCamTexture.height;
        
        GAME_WINDOW_HEIGHT = Display.displays[_lpipCoreManager.PROJECTOR_DISPLAY_ID].systemHeight;
        GAME_WINDOW_WIDTH = Display.displays[_lpipCoreManager.PROJECTOR_DISPLAY_ID].systemWidth;
        
        
        Debug.LogWarning($"Window res: {GAME_WINDOW_WIDTH}x{GAME_WINDOW_HEIGHT}");
        DebugText.instance.AddText($"\nWindow res: {GAME_WINDOW_WIDTH}x{GAME_WINDOW_HEIGHT}", DebugText.DebugTextGroup.Resolution);
        DebugText.instance.AddText($"\nScreen res: {Screen.currentResolution}", DebugText.DebugTextGroup.Resolution);

        GAME_WINDOW_FACTORX =  GAME_WINDOW_WIDTH / Convert.ToSingle(CAMERA_WIDTH);
        GAME_WINDOW_FACTORY =  GAME_WINDOW_HEIGHT / Convert.ToSingle(CAMERA_HEIGHT);
        Debug.LogWarning($"Window res factor is: {GAME_WINDOW_FACTORX}:{GAME_WINDOW_FACTORY}");
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

        _lpipCoreManager.WindowData = windowData;

        CameraData cameraData = new CameraData
        {
            CAMERA_WIDTH = CAMERA_WIDTH,
            CAMERA_HEIGHT = CAMERA_HEIGHT
        };

        _lpipCoreManager.CameraData = cameraData;
    }
}
