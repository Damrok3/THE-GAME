using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestingScript : MonoBehaviour
{
    private Grid grid;
    private float mouseMoveTimer;
    private float mouseMoveTimerMax = .01f;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(20, 10, 10f, new Vector3(0,0));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.SetValue(GetMouseWorldPosition(), 56);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(grid.GetValue(GetMouseWorldPosition()));
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


