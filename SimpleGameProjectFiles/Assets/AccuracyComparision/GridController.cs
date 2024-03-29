using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridController : MonoBehaviour
{
    [SerializeField] private UIGridRenderer _uiGridRenderer;
    [SerializeField] private GameObject cellLabelPrefab;
    [SerializeField] private GameObject canvasRoot;

    private List<List<GameObject>> labelRefMatrix;
    
    [SerializeField] private Vector2Int gridSize = new Vector2Int(1,1);
    [SerializeField] private float lineThickess;
    [SerializeField] private Vector2 currentSelectedGridCellPos;

    private Vector2Int lastPos;
    
    // Start is called before the first frame update
    void Start()
    {
        Refresh();
        PopulateLabels();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var gridPos = GetGridCellFromMouseClick();
            lastPos = gridPos;
            var _blockSizeX = Convert.ToSingle(Screen.width) / gridSize.x;
            var _blockSizeY = Convert.ToSingle(Screen.height) / gridSize.y;
            
            currentSelectedGridCellPos = new Vector2(gridPos.x * _blockSizeX + _blockSizeX/2, gridPos.y * _blockSizeY + _blockSizeY/2);
            
            _uiGridRenderer.SetSelected(gridPos);
            _uiGridRenderer.SetVerticesDirty();
            
            Debug.Log($"<color=cyan>LMB at row {gridPos.x + 1}, col {gridPos.y + 1}</color>, <color=yellow>{Input.mousePosition.x}, {Input.mousePosition.y}</color>, Block size: {_blockSizeX}, {_blockSizeY}");
        }else if (Input.GetMouseButtonDown(1))
        {
            UpdateSelectedTextColor(lastPos.x, lastPos.y, Color.white);
            lastPos = Vector2Int.one * -1;
            
            currentSelectedGridCellPos = Vector2.one * -1;
            
            _uiGridRenderer.SetSelected(Vector2Int.one * -1);
            _uiGridRenderer.SetVerticesDirty();
        }
        Debug.Log($"Start: Resolution = <color=green>{Screen.width}, {Screen.height}</color>");
    }

    private Vector2Int GetGridCellFromMouseClick()
    {
        UpdateSelectedTextColor(lastPos.x, lastPos.y, Color.white);
        
        var mousePos = Input.mousePosition;

        var _blockSizeX = Convert.ToSingle(Screen.width) / gridSize.x;
        var _blockSizeY = Convert.ToSingle(Screen.height) / gridSize.y;
        
        var xPos = Mathf.FloorToInt(mousePos.x / _blockSizeX);
        var yPos = Mathf.FloorToInt(mousePos.y / _blockSizeY);
        Debug.Log($"<color=red> {xPos}, {yPos}</color>");
        
        UpdateSelectedTextColor(xPos, yPos, Color.green);
        
        return new Vector2Int(xPos,yPos);
    }

    private void PopulateLabels()
    {
        var _blockSizeX = Convert.ToSingle(Screen.width) / gridSize.x;
        var _blockSizeY = Convert.ToSingle(Screen.height) / gridSize.y;
        
        List<List<GameObject>> allLabels = new List<List<GameObject>>();
        for (int y = 0; y < gridSize.y; y++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int x = 0; x < gridSize.x; x++)
            {
                var label = Instantiate(cellLabelPrefab, canvasRoot.transform);
                var pos = new Vector2(x * _blockSizeX + 10f, y * _blockSizeY);
                label.transform.position = pos;
                row.Add(label);

                if (label.TryGetComponent(out TextMeshProUGUI tmpText))
                {
                    tmpText.text = $"Δ {Random.Range(1, 900)} px";
                }
            }
            allLabels.Add(row);
        }

        labelRefMatrix = allLabels;
    }

    private void Refresh()
    {
        if (gridSize.x < 1)
        {
            gridSize.x = 1;
        }
        
        if (gridSize.y < 1)
        {
            gridSize.y = 1;
        }

        _uiGridRenderer.thickness = lineThickess;
        _uiGridRenderer.gridSize = gridSize;
        
        currentSelectedGridCellPos = Vector2.one * -1;
        _uiGridRenderer.SetSelected(Vector2Int.one * -1);

        _uiGridRenderer.SetAllDirty();
    }

    private void UpdateSelectedTextColor(int x, int y, Color color)
    {
        if (x > -1 && y > -1)
        {
            if(labelRefMatrix[y][x].TryGetComponent(out TextMeshProUGUI tmpTextUi))
            {
                tmpTextUi.color = color;
                
                if(!color.Equals(Color.green))
                {
                    return;
                }
                
                Regex r = new Regex("[1-9][0-9]*[0-9]");
                MatchCollection matches = r.Matches(tmpTextUi.text);
                int count = Int32.Parse(matches[0].Value);
                count++;
                tmpTextUi.text = $"Δ {count} px";
            }
        }
    }

    private void OnValidate()
    {
        Refresh();
    }
}
