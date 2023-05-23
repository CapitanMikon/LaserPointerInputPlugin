using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeController : MonoBehaviour, LPIPIInteractable
{

    [SerializeField] private TMP_Text itemText;
    private int id;

    [SerializeField] private Material correct;
    [SerializeField] private Material incorrect;
    private Material defaultMat;
    [SerializeField] private Renderer cubeRenderer;

    private bool canInteract;
    
    

    private void OnMouseDown()
    {
        if (!canInteract)
            return;

        Debug.Log($"Clicked on cube with ID {id}");
        GameController.OnItemClicked(id);
        SetTextVisibility(true);
        canInteract = false;
    }

    private void OnEnable()
    {
        GameController.OnCubeAnswer += AnswerEvent;
        GameController.OnGameStateChanged += GameStateChangedEvent;
    }

    private void OnDisable()
    {
        GameController.OnCubeAnswer -= AnswerEvent;
        GameController.OnGameStateChanged -= GameStateChangedEvent;
    }

    private void Awake()
    {
        cubeRenderer = GetComponentInChildren<Renderer>();
        defaultMat = cubeRenderer.material;
        canInteract = false;
    }

    private void GameStateChangedEvent(GameState state)
    {
        switch (state)
        {
            case GameState.playing:
                canInteract = true;
                break;
            case GameState.waiting:
                canInteract = false;
                break;
            default:
                canInteract = false;
                break;
        }
    }

    private void AnswerEvent(ItemData data)
    {
        if (data.id == id)
        {
            SetColorCorrect(data.isCorrect);
        }
    }


    public void InitializeCube()
    {
        itemText.text = Convert.ToString(id + 1);
    }

    public void SetTextVisibility(bool isVisible)
    {
        itemText.enabled = isVisible;
        //Debug.Log("Text was hidden!");
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void ResetToDefault()
    {
        cubeRenderer.material = defaultMat;
        SetTextVisibility(true);
    }

    private void SetColorCorrect(bool isCorrect)
    {
        if (isCorrect)
        {
            cubeRenderer.material = correct;
        }
        else
        {
            cubeRenderer.material = incorrect;
        }
    }
    
    public void LPIPOnLaserHit()
    {
        OnMouseDown();
    }
}
