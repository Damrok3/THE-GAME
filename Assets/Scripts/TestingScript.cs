using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestingScript : MonoBehaviour
{
    [SerializeField] private HeatMapVisual heatMapVisual;
    private Grid grid;
    private float mouseMoveTimer;
    private float mouseMoveTimerMax = .01f;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(20, 20, 5f, new Vector3(0, 0));
        heatMapVisual.SetGrid(grid);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 position = GetMouseWorldPosition();
            grid.AddValue(position, 100, 0, 5);
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


