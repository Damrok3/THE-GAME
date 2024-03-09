using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] GameObject player;
    public bool showDebugGrid = true;

    private Pathfinding pathfinding;
    private float cellSize = 5f;
    private void Start()
    {
        pathfinding = new Pathfinding(160, 100, cellSize);

    }
    private void Update()
    {

        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetGrid().GetXY(player.transform.position, out int Px, out int Py);
            List<PathNode> path = pathfinding.FindPath(Px, Py, x, y);
            List<Vector3> vec3Path = pathfinding.GetPathVector3List(path);
            foreach (Vector3 vec3 in vec3Path)
            {
                Debug.Log(vec3);
            }
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine( new Vector3(path[i].x, path[i].y) * cellSize + Vector3.one * cellSize/2,
                                    new Vector3(path[i + 1].x, path[i + 1].y) * cellSize + Vector3.one * cellSize/2,
                                    Color.green,
                                    2f);
                }
            }
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
