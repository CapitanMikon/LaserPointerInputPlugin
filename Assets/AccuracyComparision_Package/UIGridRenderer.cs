using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGridRenderer : Graphic
{

    [SerializeField] public float thickness = 10f;
    [SerializeField] public Vector2Int gridSize = new Vector2Int(1,1);


    private float width = 1;
    private float height = 1;

    private float offsetX;
    private float offsetY;

    public float cellWidth;
    public float cellHeight;

    public Vector2Int currentSelectedGridCellXYPos;
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        var transformRect = rectTransform.rect;

        cellWidth = transformRect.width / gridSize.x;
        cellHeight = transformRect.height / gridSize.y;

        offsetX = transformRect.width / 2;
        offsetY = transformRect.height / 2;


        int count = 0;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                DrawCell(x,y, count, vh);
                count++;
            }
        }

        if (currentSelectedGridCellXYPos.x != -1 && currentSelectedGridCellXYPos.y != -1)
        {
            DrawSquare(20f, vh, vh.currentVertCount);
        }
    }

    private void DrawSquare(float size, VertexHelper vh, int start)
    {
        float xPos = cellWidth * currentSelectedGridCellXYPos.x + cellWidth /2 - offsetX;
        float yPos = cellHeight * currentSelectedGridCellXYPos.y + cellHeight /2 - offsetY;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = Color.red;
        
        vertex.position = new Vector3(xPos - size/2, yPos - size/2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + size/2, yPos - size/2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + size/2, yPos + size/2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos - size/2, yPos + size/2);
        vh.AddVert(vertex);
        
        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }

    private void DrawCell(int x, int y, int index, VertexHelper vh)
    {
        float xPos = cellWidth * x - offsetX;
        float yPos = cellHeight * y - offsetY;
        
        
        UIVertex vertex = UIVertex.simpleVert;
        
        vertex.color = color;
        
        if (x == currentSelectedGridCellXYPos.x && y == currentSelectedGridCellXYPos.y)
        {
            vertex.color = Color.green;
        }
        //vertex.color = Color.green;

        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos,  yPos + cellHeight);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos);
        vh.AddVert(vertex);
        
        //vh.AddTriangle(0, 1, 2);
        //vh.AddTriangle(2, 3, 0);


        float widthSqr = thickness * thickness;
        float distanceSqr = widthSqr / 2f;
        float distance = Mathf.Sqrt(distanceSqr);

        vertex.position = new Vector3(xPos + distance, yPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + distance, yPos + (cellHeight - distance));
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + (cellWidth - distance), yPos + (cellHeight - distance));
        vh.AddVert(vertex);
        
        vertex.position = new Vector3( xPos + (cellWidth - distance), yPos  + distance);
        vh.AddVert(vertex);

        int offset = index * 8;

        
        vh.AddTriangle(offset + 0, offset + 1, offset + 5);
        vh.AddTriangle(offset + 5, offset + 4, offset + 0);
        
        vh.AddTriangle(offset + 1, offset + 2, offset + 6);
        vh.AddTriangle(offset + 6, offset + 5, offset + 1);
        
        vh.AddTriangle(offset + 2, offset + 3, offset + 7);
        vh.AddTriangle(offset + 7, offset + 6, offset + 2);
        
        vh.AddTriangle(offset + 3, offset + 0, offset + 4);
        vh.AddTriangle(offset + 4, offset + 7, offset + 3);
    }

    public void SetSelected(Vector2Int pos)
    {
        currentSelectedGridCellXYPos = pos;
    }
}
