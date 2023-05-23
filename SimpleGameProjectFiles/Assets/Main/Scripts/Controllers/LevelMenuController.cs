using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelMenuController : MonoBehaviour
{

    private int level;

    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private TMP_Text levelText;

    private UIController _uiController;

    void Start()
    {
        level = 1;
        SetButtonInteractable(ButtonType.minus, false);
        SetButtonInteractable(ButtonType.plus, true);
        UpdateLevelText();
    }

    private void SetButtonInteractable(ButtonType buttonType, bool isInteractable)
    {
        switch (buttonType)
        {
          case ButtonType.minus:
              minusButton.interactable = isInteractable;
              break;
          case ButtonType.plus:
              plusButton.interactable = isInteractable;
              break;
        } 
    }

    private void ChangeLevel(bool isIncreased)
    {
        if (isIncreased)
        {
            level++;
            if (level > 1)
            {
                SetButtonInteractable(ButtonType.minus, true);
            }
        }
        else
        {
            level--;
            if (level < 2)
            {
                SetButtonInteractable(ButtonType.minus, false);
            }
        }

        UpdateLevelText();
    }

    public void OnStartGame()
    {
        _uiController.PlayGame();
    }

    public void OnPlusButton()
    {
        ChangeLevel(true);
    }

    public void OnMinusButton()
    {
        ChangeLevel(false);
    }
    
    public void OnMainMenuButton()
    {
        _uiController.OnMainMenuButton();
    }

    public int GetLevel()
    {
        return level;
    }

    public void SetController(UIController uiController)
    {
        _uiController = uiController;
    }

    private void UpdateLevelText()
    {
        levelText.text = Convert.ToString(level);
    }

}

enum ButtonType
{
    plus,
    minus
}
