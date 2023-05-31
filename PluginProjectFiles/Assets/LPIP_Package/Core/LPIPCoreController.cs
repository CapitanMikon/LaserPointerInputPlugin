using UnityEngine;

public class LPIPCoreController : MonoBehaviour
{
  [SerializeField] private LPIPCoreManager _lpipCoreManager;

  public static LPIPCoreController Instance;
  
  private bool isWebCamTextureConfigured = false;
  private bool isProjectorIdConfigured = false;

  private void Awake()
  {
      if (Instance == null)
      {
          Instance = this;
      }
  }

  public void InitializeLPIP()
  {
      if (isWebCamTextureConfigured && isProjectorIdConfigured)
      {
        _lpipCoreManager.StartLPIP();
      }
      else
      {
          if (!(isWebCamTextureConfigured && isProjectorIdConfigured))
          {
              Debug.LogError("WebCamTexture and ProjectorId is not configured!");
          }
      }
  }
  
  public void ResetLPIP()
  {
     _lpipCoreManager.ResetLPIP();
  }

  public void ReCalibrateLPIP()
  {
      _lpipCoreManager.ReCalibrateLPIP();
  }

  public void SetWebCamTexture(WebCamTexture webCamTexture)
  {
      if (webCamTexture == null)
      {
          Debug.LogError("webCamTexture == NULL");
          return;
      }
      _lpipCoreManager.WebCamTexture = webCamTexture;
      isWebCamTextureConfigured = true;

  }
  
  public void SetProjectorDisplayId(int projectorDisplayId)
  {
      if (projectorDisplayId < 0)
      {
          Debug.LogError("projectorDisplayId must be non negative!");
          return;
      }
      _lpipCoreManager.PROJECTOR_DISPLAY_ID = projectorDisplayId;
      isProjectorIdConfigured = true;
  }
}
