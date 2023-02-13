using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCamera : MonoBehaviour
{
    public GameObject copy;
    public Renderer rend_other;
    public Renderer main;
    private WebCamTexture webcam;
    private Color[] camPixels;

    private WebCamTexture image;
    private Texture2D image2;

    // Start is called before the first frame update
    void Start()
    {
        rend_other = copy.GetComponent<Renderer>();
        main = GetComponent<Renderer>();

        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
        {
            Debug.Log("camera <" + devices[i].name + "> detected");
        }

        if (devices.Length > 0)
        {
            webcam = new WebCamTexture(devices[0].name);
            main.material.mainTexture = webcam;
            //setTo720p30FPS();
            setTo1080p60FPS();
            webcam.Play();
        }
        image = main.material.mainTexture as WebCamTexture;
        image2 = new Texture2D(image.width, image.height);
        //StartCoroutine("Process");
    }

    void setTo720p30FPS()
    {
        webcam.requestedFPS = 30;
        webcam.requestedWidth = 1280;
        webcam.requestedHeight = 720;
    }
    
    void setTo1080p60FPS()
    {
        webcam.requestedFPS = 60;
        webcam.requestedWidth = 1920;
        webcam.requestedHeight = 1080;
    }

    private void Update()
    {
        Debug.Log(String.Format("Camera FPS: {0}", webcam.requestedFPS));
        Debug.Log(String.Format("Camera resolution: {0}x{1}", webcam.width, webcam.height));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //doIMGStuff();
    }

    void doIMGStuff()
    {
        WebCamTexture image = main.material.mainTexture as WebCamTexture;
        Texture2D image2 = new Texture2D(image.width, image.height);
        camPixels = image.GetPixels();

        for (int i = 0; i < camPixels.Length; i++)
        {
            camPixels[i].r = camPixels[i].g = camPixels[i].b = (camPixels[i].r + camPixels[i].g + camPixels[i].b) / 3;
        }
        image2.SetPixels(camPixels);
        image2.Apply();
        rend_other.material.mainTexture = image2;
        
        /*for (int y = 0; y < image.height; y++)
        {
            for (int x = 0; x < image.width; x++)
            {
                Color color = image.GetPixel(x, y);
                color.b = color.g = color.r = (color.b + color.g + color.r) / 3;
                image2.SetPixel(x, y, color);
            }
        }*/

        //image2.Apply();
        //rend_other.material.mainTexture = image2;
    }
    
    IEnumerator Process()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(1f);
            camPixels = image.GetPixels();

            for (int i = 0; i < camPixels.Length; i++)
            {
                camPixels[i].r = camPixels[i].g = camPixels[i].b = (camPixels[i].r + camPixels[i].g + camPixels[i].b) / 3;
            }
            image2.SetPixels(camPixels);
            image2.Apply();
            rend_other.material.mainTexture = image2;
        }
    }
}