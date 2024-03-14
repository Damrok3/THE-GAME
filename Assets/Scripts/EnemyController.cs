using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private GameObject player;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("player");
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Testing.pathfinding.GetGrid().GetXY(player.transform.position, out int Px, out int Py);
        Testing.pathfinding.GetGrid().GetXY(transform.position, out int x, out int y);
        List<PathNode> path = Testing.pathfinding.FindPath(x, y, Px, Py);
        List<Vector3> vec3Path = Testing.pathfinding.GetPathVector3List(path);
        Vector3 direction = (vec3Path[1] - transform.position) + Vector3.one * Testing.cellSize/2;
        rb.AddForce(direction.normalized * Testing.enemySpeed, ForceMode2D.Force);

        Debug.DrawLine(transform.position,transform.position + direction,Color.red, 0.1f);

        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 5f + Vector3.one * 5f / 2,
            new Vector3(path[i + 1].x, path[i + 1].y) * 5f + Vector3.one * 5f / 2,
            Color.green,
            0.1f);
        }
    }
}
