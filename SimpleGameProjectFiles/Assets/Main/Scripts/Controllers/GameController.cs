using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    
    private int level = 0;
    [SerializeField] private bool randomRepeatedLevel;
    public static event Action<int> OnCubeClicked;
    public static event Action<ItemData> OnCubeAnswer;
    public static event Action<GameState> OnGameStateChanged;

    private int expected = 0;
    private GameState currentGameState;

    private ItemData _itemData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Initializating GameController");
        SetToDefaultSettings();
        //StartCoroutine(StartGameCoroutine());
    }

    private void SetToDefaultSettings()
    {
        _itemData = new ItemData { id = -1, isCorrect = false};
        ChangeGameState(GameState.waiting);
        expected = 0;
        
    }

    public void StartGame(int startingLevel)
    {
        Debug.LogWarning($"Game started with param {startingLevel}");
        SetToDefaultSettings();
        CubeManager.Instance.ResetSize();
        if (startingLevel >= 0 )
        {
            level = startingLevel;
        }

        StartCoroutine(StartGameCoroutine());
    }

    private void OnEnable()
    {
        OnCubeClicked += OnItemClickedEventFired;
        GamePlayUIController.OnReturnToMainMenu += StopGame;
    }

    private void OnDisable()
    {
        OnCubeClicked -= OnItemClickedEventFired;
        GamePlayUIController.OnReturnToMainMenu -= StopGame;

    }

    IEnumerator StartGameCoroutine()
    {
        //game starts after 1 s
        Debug.LogWarning("Starting game in 1 second");
        yield return new WaitForSeconds(1);
        //start game level round ()
        yield return GameLevelCoroutine(LevelState.initial);
    }

    IEnumerator GameLevelCoroutine(LevelState levelState)
    {
        Debug.LogWarning($"Game level {level + 1} has started!");
        ChangeGameState(GameState.waiting);
        //generate cubes for level
        if (levelState == LevelState.next || levelState == LevelState.initial)
        {
            CubeManager.Instance.GenerateCubes(level + 1);
        }else if (levelState == LevelState.restartKeep)
        {
            CubeManager.Instance.RefreshGeneratedCubes();
        }else if (levelState == LevelState.restartRandom)
        {
            CubeManager.Instance.GenerateCubes(level + 1);
        }
        
        //show text
        yield return SetTextVisibilityAfterSecondsCoroutine(0, true);
        //hide text after X seconds
        yield return SetTextVisibilityAfterSecondsCoroutine(2,false);
        //allow user to start playing
        ChangeGameState(GameState.playing);
    }
    
    IEnumerator SetTextVisibilityAfterSecondsCoroutine(int afterSeconds, bool isVisible)
    {
        if (afterSeconds > 0)
        {
            yield return new WaitForSeconds(afterSeconds);
        }
        
        CubeManager.Instance.SetTextVisibilityOnAllCubesInUse(isVisible);
    }
    
    public static void OnItemClicked(int id)
    {
        OnCubeClicked?.Invoke(id);
    }

    private void OnItemClickedEventFired(int id)
    {
        if (currentGameState == GameState.waiting)
        {
            return;
        }
        
        Debug.Log($"expected {expected} got {id}");
        if (id == expected)
        {
            (_itemData.id, _itemData.isCorrect) = (id, true);
            OnCubeAnswer?.Invoke(_itemData);
            
            //if guessed all correct
            if (expected == level)
            {
                ChangeGameState(GameState.waiting);
                
                //start next level
                StartCoroutine(NextLevelAfterSeconds(2));
            }
            else
            {
                expected++;
            }
        }
        else
        {
            (_itemData.id, _itemData.isCorrect) = (id, false);
            OnCubeAnswer?.Invoke(_itemData);
            
            ChangeGameState(GameState.waiting);
            //restart current level
            StartCoroutine(RestartLevelAfterSeconds(2));
        }
        
    }

    private IEnumerator NextLevelAfterSeconds(int seconds)
    {
        Debug.LogWarning($"Level completed, next level in {seconds} seconds!");
        yield return new WaitForSeconds(seconds);
        yield return NextLevel();
    }
    
    private IEnumerator RestartLevelAfterSeconds(int seconds)
    {
        Debug.LogWarning($"Wrong answer, restarting in {seconds} seconds!");
        yield return new WaitForSeconds(seconds);
        yield return RestartLevel();
    }

    private IEnumerator NextLevel()
    {
        //set level data
        level++;
        expected = 0;
        
        //perform usual routine
        yield return GameLevelCoroutine(LevelState.next);
    }
    
    private IEnumerator RestartLevel()
    {
        //set level data
        expected = 0;
        
        //perform usual routine
        if (randomRepeatedLevel)
        {
            yield return GameLevelCoroutine(LevelState.restartRandom);
        }
        else
        {
            yield return GameLevelCoroutine(LevelState.restartKeep);
        }
    }

    /*IEnumerator RestartLevelAfterSeconds(int seconds)
    {
        Debug.LogWarning($"Wrong answer, restarting in {seconds} seconds!");
        yield return new WaitForSeconds(seconds);
        RestartLevel();
        yield return new WaitForSeconds(1);
        currentGameState = GameState.playing;
    }*/
    
    /*IEnumerator NextLevelAfterSeconds(int seconds)
    {
        Debug.LogWarning($"Level completed, next level in {seconds} seconds!");
        yield return new WaitForSeconds(seconds);
        NextLevel();
        yield return new WaitForSeconds(1);
        currentGameState = GameState.playing;
    }*/

    private void ChangeGameState(GameState gameState)
    {
        currentGameState = gameState;
        OnGameStateChanged?.Invoke(currentGameState);
    }

    private void StopGame()
    {
        Debug.LogWarning("Game stopped. Returning to main menu!");
        StopAllCoroutines();
        CubeManager.Instance.RefreshUsedCubes();
        SetToDefaultSettings();
    }
}
public enum GameState
{
    playing,
    waiting
}

enum LevelState
{
    next,
    restartKeep,
    restartRandom,
    initial,
    
}

public struct ItemData
{
    public int id;
    public bool isCorrect;
}
