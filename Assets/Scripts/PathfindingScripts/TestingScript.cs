using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestingScript : MonoBehaviour
{
    [SerializeField] private HeatMapVisual heatMapVisual;
    [SerializeField] private HeatMapBoolVisual heatMapBoolVisual;
    [SerializeField] private HeatMapGenericVisual heatMapGenericVisual;
    private Grid<HeatMapGridObject> grid;
    //private Grid<StringGridObject> stringGrid;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid<HeatMapGridObject>(20, 10, 10f, new Vector3(-20f, -40f), (Grid<HeatMapGridObject> g, int x, int y) => new HeatMapGridObject(g, x, y));
        //stringGrid = new Grid<StringGridObject>(20, 10, 10f, new Vector3(-20f, -40f), (Grid<StringGridObject> g, int x, int y) => new StringGridObject(g, x, y));
        //heatMapVisual.SetGrid(grid);
        //heatMapBoolVisual.SetGrid(grid);
        //heatMapGenericVisual.SetGrid(grid);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = GetMouseWorldPosition();

        if (Input.GetMouseButtonDown(0))
        {
            HeatMapGridObject heatMapGridObject = grid.GetGridObject(position);
            if (heatMapGridObject != null)
            {
                heatMapGridObject.AddValue(5);
            }
            //grid.AddValue(position, 100, 0, 5);
            //grid.SetValue(position, true);
        }

        //if (Input.GetKeyDown(KeyCode.A)) { stringGrid.GetGridObject(position).AddLetter("A"); }
        //if (Input.GetKeyDown(KeyCode.B)) { stringGrid.GetGridObject(position).AddLetter("B"); }
        //if (Input.GetKeyDown(KeyCode.C)) { stringGrid.GetGridObject(position).AddLetter("C"); }

        //if (Input.GetKeyDown(KeyCode.Alpha1)) { stringGrid.GetGridObject(position).AddNumber("1"); }
        //if (Input.GetKeyDown(KeyCode.Alpha2)) { stringGrid.GetGridObject(position).AddNumber("2"); }
        //if (Input.GetKeyDown(KeyCode.Alpha3)) { stringGrid.GetGridObject(position).AddNumber("3"); }

    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane + 1;
        Vector2 position = Camera.main.ScreenToWorldPoint(screenPosition);
        return position;
    }
}

public class HeatMapGridObject
{
    private const int MIN = 0;
    private const int MAX = 100;

    private Grid<HeatMapGridObject> grid;
    private int x;
    private int y;
    private int value;

    public HeatMapGridObject(Grid<HeatMapGridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void AddValue(int addValue)
    {
        value += addValue;
        value = Mathf.Clamp(value, MIN, MAX);
        //grid.TriggerGridObjectChanged(x, y);
    }

    public float GetValueNormalized()
    {
        return (float)value / MAX;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

public class StringGridObject
{
    private Grid<StringGridObject> grid;
    private int x;
    private int y;

    private string letters;
    private string numbers;

    public StringGridObject(Grid<StringGridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        letters = "";
        numbers = "";
    }

    public void AddLetter(string letter) 
    {
        letters += letter;
        //grid.TriggerGridObjectChanged(x, y);
    }

    public void AddNumber(string number)
    {
        numbers += number;
        //grid.TriggerGridObjectChanged(x, y);
    }

    public override string ToString()
    {
        return letters + "\n" + numbers;
    }
}


