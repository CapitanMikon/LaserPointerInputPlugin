using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubeManager : MonoBehaviour
{

    public static CubeManager Instance;
    
    private int currentId = 0;
    private int attemptsTreshold = 800;
    
    [SerializeField] private GameObject cubePrefab;
    
    [SerializeField] private int preSpawnCubesCount = 10;
    
    [SerializeField] private Vector2 xValuesInterval = new Vector2(-8.25f, 8.25f);
    [SerializeField] private Vector2 zValuesInterval = new Vector2( -3.4f, 5.4f);
    [SerializeField] private float padding = 0.2f;

    private List<GameObject> cubes = new List<GameObject>();
    private List<GameObject> inUseCubes = new List<GameObject>();
    private Vector3 offScreenPosition = new Vector3(-100, -100, 100);

    [SerializeField] private int generateCubesCount = 5;
    private float size;
    private float resizeStep = 0.15f;
    private Vector3 resizeStepVector;

    private Vector3 defaultScale;
    private Vector3 currentScale;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Initialize()
    {
        cubes = new List<GameObject>();
        inUseCubes = new List<GameObject>();
        defaultScale =  cubePrefab.transform.localScale;
        currentScale = defaultScale;
        
        size = Mathf.Sqrt(Mathf.Pow(cubePrefab.transform.localScale.x, 2) + Mathf.Pow(cubePrefab.transform.localScale.y, 2));
        resizeStepVector = new Vector3(resizeStep, resizeStep, resizeStep);
        Debug.Log($"{size} , sqrt2 = {Mathf.Sqrt(2)}");
    }
    void Start()
    {
        Initialize();
        CreateCubes(preSpawnCubesCount);
        //GenerateCubes();
        //Debug.Log($"Spawning {preSpawnCubesCount} cubes!");
    }

    
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateCubes(generateCubesCount);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            generateCubesCount++;
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if(generateCubesCount > 1)
                generateCubesCount--;
        }*/
    }

    public void GenerateCubes(int count)
    {
        RefreshUsedCubes();
        var attempts = 0;
        for (int i = 0; i < count; i++)
        {
            Vector3 position = offScreenPosition;
            do
            {
                if (attempts > attemptsTreshold)
                {
                    break;
                }
                position = new Vector3(Random.Range(xValuesInterval.x, xValuesInterval.y) ,Random.Range(zValuesInterval.x, zValuesInterval.y), 0);
                attempts++;
            } while (IsOccupied(position));

            if (attempts > attemptsTreshold)
            {
                Debug.LogError("Unable to fit cube in! Resizing and trying again");
                ResizeAndTryAgain(count);
                return;
            }
            
            
            var cube = GetCubeAtIndex(i);
            inUseCubes.Add(cube);
            var cubeController = cube.GetComponent<CubeController>();
            cubeController.SetPosition(position);
            cubeController.ResetToDefault();
        }
    }
    
    public void RefreshGeneratedCubes()
    {
        for (int i = 0; i < inUseCubes.Count; i++)
        {
            var cube = inUseCubes[i];
            var cubeController = cube.GetComponent<CubeController>();
            cubeController.ResetToDefault();
        }
    }

    public void RefreshUsedCubes()
    {
        foreach (var cube in inUseCubes)
        {
            var cubeController = cube.GetComponent<CubeController>();
            cubeController.ResetToDefault();
            cubeController.SetPosition(offScreenPosition);
        }
        inUseCubes.Clear();
    }

    private void ResizeAndTryAgain(int count)
    {
        if (currentScale.x <= 0) // to prevent infinite loop
        {
            StopAllCoroutines();
            throw new Exception("Cannot Create requested number of blocks");
        }
        currentScale -= resizeStepVector;
        foreach (var cube in cubes)
        {
            cube.transform.localScale = currentScale;
        }
        // recalculate size
        size = Mathf.Sqrt(Mathf.Pow(cubePrefab.transform.localScale.x, 2) +
                          Mathf.Pow(cubePrefab.transform.localScale.y, 2));
        GenerateCubes(count);
    }
    
    public void ResetSize()
    {
        cubePrefab.transform.localScale = defaultScale;
        foreach (var cube in cubes)
        {
            cube.transform.localScale = defaultScale;
        }
        // recalculate size
        size = Mathf.Sqrt(Mathf.Pow(cubePrefab.transform.localScale.x, 2) +
                         Mathf.Pow(cubePrefab.transform.localScale.y, 2));
    }

    private GameObject GetCubeAtIndex(int i)
    {
        if (i >= cubes.Count)
        {
            CreateCubes(10);
        }
        return cubes[i];
    }

    private GameObject CreateCube()
    {
        return Instantiate(cubePrefab, offScreenPosition, Quaternion.identity);
    }

    private void CreateCubes(int count)
    {
        Debug.Log($"Creating additional {count} cubes! Total count: {cubes.Count + count}");
        for (int i = 0; i < count; i++)
        {
            var cube = CreateCube();
            cubes.Add(cube);
            cube.transform.localScale = currentScale;
            
            var cubeController = cube.GetComponent<CubeController>();
            cubeController.SetId(currentId++);
            cubeController.InitializeCube();
        }
    }

    private bool IsOccupied(Vector3 position)
    {
        foreach (var cube in inUseCubes)
        {
            var transformA = cube.transform.position;
            
            if ((position - cube.transform.position).magnitude <= size + padding)
            //if ( Mathf.Abs(position.x - transformA.x) <= size.x + padding.x ||  Mathf.Abs(position.y - transformA.y ) <= size.y + padding.y)
            {
                Debug.LogWarning("Obstacled spawn, generating new!");
                return true;
            }
        }
        /*if (Physics.BoxCast(position, size, Vector3.zero))
        {
            Debug.LogWarning("Obstacled spawn, generating new!");
            return true;
        }*/

        return false;
    }

    public void SetTextVisibilityOnAllCubesInUse(bool isVisible)
    {
        foreach (var cube in inUseCubes)
        {
            var cubeController = cube.GetComponent<CubeController>();
            cubeController.SetTextVisibility(isVisible);
        }
    }

}
