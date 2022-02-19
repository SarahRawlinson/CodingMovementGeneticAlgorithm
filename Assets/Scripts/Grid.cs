using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T> 
{
    // Start is called before the first frame update
    private int _width;
    private int _height;
    private float cellSize;

    private Vector3 originPostion;
    private T[,] gridArray;
 
    public event EventHandler<OnGridValueChangedEventArgs> onGridValueChanged;

    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    
    public Grid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this._width = width;
        this._height = height;
        this.cellSize = cellSize;
        this.originPostion = originPosition;
        gridArray = new T[width,height];
        Debug.Log("Grid : Width:" + _width + "Height: " + _height );

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
               Debug.Log(x + "," +  y);
               UtilsClass.CreateWorldText(null, gridArray[x, y].ToString(), GetWorldPosition(x, y) + new Vector3(cellSize,cellSize) *.5f, 20, Color.white,
                   TextAnchor.MiddleCenter, TextAlignment.Center);
               Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x,y+1), Color.white, 100f);
               Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x+1,y),Color.white, 100);
            }
        }
        
        Debug.DrawLine(GetWorldPosition(0,height), GetWorldPosition(width,height), Color.cyan, 100f);
        Debug.DrawLine(GetWorldPosition(width,0), GetWorldPosition(width,height), Color.cyan, 100f);
       
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPostion;
    }

    public void SetValue(int x, int y, T value)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            gridArray[x, y] = value;
        }
    }
    
    public void SetValue(Vector3 worldPosition, T value)
    {
        int x, y;
        GetXY(worldPosition, out x , out y);
        SetValue(x,y, value);

    }

    private T GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            return gridArray[x, y];
        }

        return default;
    
    }

    public T GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }
    
    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPostion).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPostion).y / cellSize);
    }
}
