using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayUIController : MonoBehaviour
{
    private UIController _uiController;
    public static event Action OnReturnToMainMenu;
    
    public void SetController(UIController uiController)
    {
        _uiController = uiController;
    }
    
    public void OnMainMenuButton()
    {
        OnReturnToMainMenu?.Invoke();
        _uiController.OnMainMenuButton();
    }
}
