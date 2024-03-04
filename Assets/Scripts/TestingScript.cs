using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingScript : MonoBehaviour
{
    private Grid grid;
    private Grid grid2;
    // Start is called before the first frame update
    void Start()
    {
        //grid = new Grid(4, 2, 10f, new Vector3(20,0));
        //grid2 = new Grid(10, 3, 5f, new Vector3(0,20));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            grid.SetValue(GetMouseWorldPosition(), 56);
            grid2.SetValue(GetMouseWorldPosition(), 56);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(grid.GetValue(GetMouseWorldPosition()));
            Debug.Log(grid2.GetValue(GetMouseWorldPosition()));
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane + 1;
        Vector2 position = Camera.main.ScreenToWorldPoint(screenPosition);
        return position;
    }
}
