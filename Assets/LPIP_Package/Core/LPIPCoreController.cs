using UnityEngine;

public class LPIPCoreController : MonoBehaviour
{
  [SerializeField] private LPIPCoreManager _lpipCoreManager;

  public static LPIPCoreController Instance;

  private bool onlyOnce = true;

  private void Awake()
  {
      if (Instance == null)
      {
          Instance = this;
      }
  }

  public void ActivateLPIP()
  {
      if (onlyOnce)
      {
        _lpipCoreManager.StartLPIP();
        onlyOnce = false;
      }
      else
      {
          Debug.LogWarning("LPIP may be activated only once!");
      }
  }

  public void RecalibrateLPIP()
  {
      _lpipCoreManager.RecalibrateLPIP();
  }

  private void Update()
  {
      if (Input.GetKeyDown(KeyCode.F1))
      {
          LPIPCalibrationUIController.Instance.ShowCameraFeed();
      }
      else if(Input.GetKeyDown(KeyCode.F2))
      {
          LPIPCalibrationUIController.Instance.HideCameraFeed();
      }
      else if (Input.GetKeyDown(KeyCode.F5))
      {
          RecalibrateLPIP();
      }else if (Input.GetKeyDown(KeyCode.S))
      {
          ActivateLPIP();
      }
  }
}
