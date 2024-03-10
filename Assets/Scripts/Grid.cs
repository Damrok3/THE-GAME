using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{
    public const int HEAT_MAP_MAX_VALUE = 100;
    public const int HEAT_MAP_MIN_VALUE = 0;

    //.net standard for declaring an event handler that can take class object and store its data as well as trigger certain things
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

    //declaration of a class that objects of can be passed inside of the event
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;
    private TextMesh[,] debugTextArray;

    private int debugTextSize = 10;
    bool showDebugGrid = true;
    bool showDebugText = false;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for(int x = 0; x < gridArray.GetLength(0); x++)
        {
            for(int y = 0; y < gridArray.GetLength(1); y++)
            {
                //here a lambda function is called that sets the default for each field, we're passing it by using Func in constructor which is like a delegate with the difference that it can return things
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        if(showDebugGrid)
        {

            debugTextArray = new TextMesh[width, height];
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    if (showDebugText) 
                    {
                        debugTextArray[x, y] = CreateWorldText(null, gridArray[x, y]?.ToString(), GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, debugTextSize, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center, 1);
                    }
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }

            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            //lambda expression that changes the value of debug text array subscribed to the event
            if (showDebugText)
            {
                OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                    debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x,eventArgs.y]?.ToString();
                };
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
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }
    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;

            //null checking the event in case there is no subscribers attached to it and then invoking the event while passing values into it
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });

        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }
    
    public void TriggerGridObjectChanged()
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { });
    }

    //METHOD SPECIFICALLY FOR THE HEATMAP
    //public void AddValue(int x, int y, TGridObject value)
    //{
    //    SetValue(x, y, GetValue(x, y) + value);
    //}

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
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    //METHOD SPECIFICALLY FOR THE HEATMAP
    //public void AddValue(Vector3 worldPosition, int value, int fullValueRange, int totalRange)
    //{
    //    int lowerValueAmount = Mathf.RoundToInt((float)value / (totalRange - fullValueRange));

    //    GetXY(worldPosition, out int originX, out int originY);
    //    for (int x = 0; x < totalRange; x++)
    //    {
    //        for (int y = 0; y < totalRange - x; y++)
    //        {
    //            int radius = x + y;
    //            int addValueAmount = value;
    //            if(radius > fullValueRange)
    //            {
    //                addValueAmount -= lowerValueAmount  * (radius - fullValueRange);
    //            }
    //            AddValue(originX + x, originY + y, addValueAmount);
    //            if(x != 0)
    //            {
    //                AddValue(originX - x, originY + y, addValueAmount);
    //            }
    //            if(y != 0)
    //            {
    //                AddValue(originX + x, originY - y, addValueAmount);
    //                if(x != 0)
    //                {
    //                    AddValue(originX - x, originY - y, addValueAmount);
    //                }
    //            }
    //        }
    //    }
    //}
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }
    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }
}
