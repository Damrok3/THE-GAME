using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapGenericVisual : MonoBehaviour
{
    private Grid<PathNode> grid;
    private Mesh mesh;
    private bool updateMesh = false;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    private void Grid_OnGridValueChanged(object sender, Grid<PathNode>.OnGridObjectChangedEventArgs e)
    {
        updateMesh = true;
        UpdateHeatMapVisual();
    }
    public void SetGrid(Grid<PathNode> grid)
    {
        this.grid = grid;
        
        UpdateHeatMapVisual();
        
        grid.OnGridObjectChanged += Grid_OnGridValueChanged;
    }
    private void LateUpdate()
    {
        if (updateMesh && mesh != null)
        {
            updateMesh = false;
            UpdateHeatMapVisual();
        }
    }
    private void UpdateHeatMapVisual()
    {
        if(mesh != null)
        {

            CreateEmptyMeshArrays(grid.GetWidth() / 5 * grid.GetHeight() / 5, out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
            for (int x = 0; x < grid.GetWidth() / 5; x++)
            {
                for (int y = 0; y < grid.GetHeight() / 5; y++)
                {
                    int index = x * grid.GetHeight() / 5 + y;
                    Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();
                    PathNode gridObject = grid.GetGridObject(x, y);
                    float gridValue = gridObject.isWalkable ? 0f : 0.1f;
                    Vector2 gridValueUV = new Vector2(gridValue, 0f);
                    //if(!gridObject.isWalkable)
                    {
                        MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(x, y) + quadSize * .5f, 0f, quadSize, gridValueUV, gridValueUV);
                    }
                }
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }
    }

    public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[4 * quadCount];
        uvs = new Vector2[4 * quadCount];
        triangles = new int[6 * quadCount];
    }
}
