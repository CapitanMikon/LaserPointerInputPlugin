using UnityEngine;

public class LPIPUtilityPortal : MonoBehaviour
{
    [SerializeField] private LPIPUtilityController lpipUtilityController;

    public static LPIPUtilityPortal Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            lpipUtilityController.SetActiveCameraFeed(true);
        }
        else if(Input.GetKeyDown(KeyCode.F2))
        {
            lpipUtilityController.SetActiveCameraFeed(false);
        }
    }
    
    public void OpenUtilityMenu()
    {
        lpipUtilityController.OpenUtilityUI();
    }

    public void CloseUtilityMenu()
    {
        lpipUtilityController.CloseUtilityUI();
    }
}
