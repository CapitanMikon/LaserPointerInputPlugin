using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private LevelMenuController levelMenuController;
    [SerializeField] private GamePlayUIController gamePlayUIController;
    
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject subMainMenuUI;
    [SerializeField] private GameObject gamePlayUI;

    [SerializeField] private Button playGameButton;

    private void Awake()
    {
        levelMenuController.SetController(this);
        gamePlayUIController.SetController(this);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        LPIPUtilityController.OnUtilityMenuDisabled += ShowMainMenuUI;
    }
    
    private void OnDisable()
    {
        LPIPUtilityController.OnUtilityMenuDisabled -= ShowMainMenuUI;
    }
    
    private void Start()
    {
        ShowMainMenuUI();
    }

    public void OnPlayButton()
    {
        ShowSubMainMenuUI();
    }
    
    public void OnMainMenuButton()
    {
       ShowMainMenuUI();
    }


    public void OnQuitButton()
    {
        QuitGame();
    }

    public void OnCalibrateButton()
    {
        LPIPUtilityPortal.Instance.OpenUtilityMenu();
        HideMainMenuUI();
    }
    
    private void QuitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    public void PlayGame()
    {
        Debug.LogWarning($"PlayGameUIController level {levelMenuController.GetLevel() - 1}");
        GameController.Instance.StartGame(levelMenuController.GetLevel() - 1);
        HideMainMenuUI();
        ShowGamePlayUI();

    }

    private void StartCalibration()
    {
        ShowCalibrationUI();
    }

    private void ShowCalibrationUI()
    {
        mainMenuUI.SetActive(false);
        subMainMenuUI.SetActive(false);
        gamePlayUI.SetActive(false);
    }

    private void ShowSubMainMenuUI()
    {
        mainMenuUI.SetActive(false);
        subMainMenuUI.SetActive(true);
        gamePlayUI.SetActive(false);

    }
    
    public void ShowMainMenuUI()
    {
        mainMenuUI.SetActive(true);
        subMainMenuUI.SetActive(false);
        gamePlayUI.SetActive(false);
    }

    private void HideMainMenuUI()
    {
        mainMenuUI.SetActive(false);
        subMainMenuUI.SetActive(false);

    }

    private void ShowGamePlayUI()
    {
        gamePlayUI.SetActive(true);
    }

    private void EnablePlayGameButton(bool isEnabled)
    {
        playGameButton.interactable = isEnabled;
    }
}
