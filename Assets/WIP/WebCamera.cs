using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera : MonoBehaviour
{
    [SerializeField] private RawImage webcamImageHolder;
    [SerializeField] private RawImage outputImageHolder;
    
    private WebCamTexture webCamTexture;
    private Color[] webCamPixels;
    
    private Texture2D resultImage;

    private IEnumerator laserDetectorCoroutine;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log("camera <" + devices[i].name + "> detected");
        }

        if (devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(devices[0].name);
            webcamImageHolder.texture = webCamTexture;
            ConfigureWebcam(1920, 1080, 60);
            webCamTexture.Play();
        }
        resultImage = new Texture2D(webCamTexture.width, webCamTexture.height);
        LogWebcamInfo();
        
        laserDetectorCoroutine = LaserDetectorProcess();
        StartCoroutine(laserDetectorCoroutine);
    }

    void ConfigureWebcam(int width, int height, int fps)
    {
        webCamTexture.requestedFPS = fps;
        webCamTexture.requestedWidth = width;
        webCamTexture.requestedHeight = height;
    }

    void LogWebcamInfo()
    {
        Debug.Log($"Current camera configuration:\nFPS: {webCamTexture.requestedFPS}\nRes: {webCamTexture.width}x{webCamTexture.height}");
    }

    private void Update()
    {
        
    }

    IEnumerator LaserDetectorProcess()
    {
        for (; ; )
        {
            yield return new WaitForEndOfFrame();
            webCamPixels = webCamTexture.GetPixels();

            for (int i = 0; i < webCamPixels.Length; i++)
            {
                webCamPixels[i].r = webCamPixels[i].g = webCamPixels[i].b = (webCamPixels[i].r + webCamPixels[i].g + webCamPixels[i].b) / 3;
            }
            resultImage.SetPixels(webCamPixels);
            resultImage.Apply();
            outputImageHolder.texture = resultImage;
        }
    }
}