using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapGenericVisual : MonoBehaviour
{
    private Grid<HeatMapGridObject> grid;
    private Mesh mesh;
    private bool updateMesh = false;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    private void Grid_OnGridValueChanged(object sender, Grid<HeatMapGridObject>.OnGridObjectChangedEventArgs e)
    {
        updateMesh = true;
        UpdateHeatMapVisual();
    }
    public void SetGrid(Grid<HeatMapGridObject> grid)
    {
        this.grid = grid;
        UpdateHeatMapVisual();

        grid.OnGridObjectChanged += Grid_OnGridValueChanged;
    }
    private void LateUpdate()
    {
        if (updateMesh)
        {
            updateMesh = false;
            UpdateHeatMapVisual();
        }
    }
    private void UpdateHeatMapVisual()
    {
        CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                int index = x * grid.GetHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();
                HeatMapGridObject gridObject = grid.GetGridObject(x, y);
                float gridValueNormalized = gridObject.GetValueNormalized();
                Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(x, y) + quadSize * .5f, 0f, quadSize, gridValueUV, gridValueUV);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[4 * quadCount];
        uvs = new Vector2[4 * quadCount];
        triangles = new int[6 * quadCount];
    }
}
