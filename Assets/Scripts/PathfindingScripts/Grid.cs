using System;
using UnityEngine;

public class Grid<TGridObject>
{

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                //here a lambda function is called that sets the default for each field, we're passing it by using Func in constructor which is like a delegate with the difference that it can return things
                gridArray[x, y] = createGridObject(x, y);
            }
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        //with the cellsize of 10 the worldPosition 5 will be on grid 0 and the worldPosition 15 will be on grid 1
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }
    public void GetXYtoWorldPosition(ref float x, ref float y)
    {
        x = x * cellSize + originPosition.x;
        y = y * cellSize + originPosition.y;
    }
    
    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }
    // returns grid object after passing Vector3 with position
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }
}
