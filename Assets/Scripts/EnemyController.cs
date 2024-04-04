using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Diagnostics;
using System.Data;
using Unity.VisualScripting;

public class EnemyController : MonoBehaviour
{

    private GameObject player;

    private Rigidbody2D rb;

    private Grid<PathNode> grid;

    private List<PathNode> path;

    private RaycastHit2D raycastHit;

    private Vector3 playerDirection;
    private Vector3 direction;

    private bool canSeePlayer = false;
    private bool collidedWithWall = false;
    private bool wallCollisionCooldown = false;
    private bool isPathingAroundWall = false;

    private Stopwatch stuckAtEachOtherTimer = new Stopwatch();
    private Stopwatch enemyCollisionCooldown = new Stopwatch();
    private Stopwatch pathingAroundWallTimer = new Stopwatch();

    public bool isSeenByPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("player");
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CheckIfCanSeePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (shouldFindPath())
        {
            path = FindPath();
        }
        DebugPath();
        MoveTowardsPlayer();
        ManageCollissions();
        ClearTraversedPath();
        

    }

    private void ManageCollissions()
    {
        int layerIgnoreCollision = LayerMask.NameToLayer("ignore collision");
        int layerDefault = LayerMask.NameToLayer("Default");
        if (pathingAroundWallTimer.ElapsedMilliseconds > 1500)
        {
            wallCollisionCooldown = false;
            pathingAroundWallTimer.Stop();
            pathingAroundWallTimer.Reset();
        }
        if (stuckAtEachOtherTimer.ElapsedMilliseconds > 1000)
        {
            enemyCollisionCooldown.Start();
            gameObject.layer = layerIgnoreCollision;        
        }
        if (enemyCollisionCooldown.ElapsedMilliseconds > 1000)
        {
            enemyCollisionCooldown.Stop();
            enemyCollisionCooldown.Reset();
            gameObject.layer = layerDefault;
        }
    }

    private bool shouldFindPath()
    {
        if (path == null && !canSeePlayer) return true;
        if (!canSeePlayer && path.Count < 2) return true;
        if (collidedWithWall) { collidedWithWall = false; return true; }
        return false;
    }

    private void CheckIfCanSeePlayer()
    {
        playerDirection = (player.transform.position - transform.position).normalized;
        raycastHit = Physics2D.Raycast(transform.position, playerDirection);

        if (raycastHit.collider.tag == "Player" && !wallCollisionCooldown)
        {
            canSeePlayer = true;
            path = null;
        }
        else
        {
            canSeePlayer = false;
        }
    }
    private void MoveTowardsPlayer()
    {
        if (canSeePlayer)
        {
            direction = (player.transform.position - transform.position);
        }
        else if (path != null && path.Count >= 2)
        {
            Vector3 vec3Node = Testing.pathfinding.NodeToVector3(path[1]);
            direction = (vec3Node - transform.position) + new Vector3(Testing.cellSize / 2, Testing.cellSize / 2, 0f);
        }
        else
        {
            direction = Vector3.zero;
        }

        //UnityEngine.Debug.DrawLine(transform.position, transform.position + direction, Color.red);

        if (direction != Vector3.zero && !isSeenByPlayer)
        {
            
            rb.AddForce(direction.normalized * Testing.enemySpeed, ForceMode2D.Force);
        }
        else if(isSeenByPlayer)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = 0f;           
        }
        
    }
    private void DebugPath()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                UnityEngine.Debug.DrawLine(new Vector3(path[i].x, path[i].y) * Testing.cellSize + Vector3.one * Testing.cellSize / 2,
                new Vector3(path[i + 1].x, path[i + 1].y) * Testing.cellSize + Vector3.one * Testing.cellSize / 2,
                Color.green);
            }
        }
    }
    private void ClearTraversedPath()
    {
        if(path != null && path.Count < 2)
        {
            path = null;
        }
        if (path != null && (Testing.pathfinding.NodeToVector3(path[1]) - transform.position).magnitude < 3.6f)
        {
            path.Remove(path[0]);
            path.Remove(path[0]);
        }
        else if ( path!= null && isPathingAroundWall)
        {
            path.Remove(path[0]);
            isPathingAroundWall = false;
        }
    }
    private List<PathNode> FindPath()
    {
        grid = Testing.pathfinding.GetGrid();
        int Px, Py;
        int x, y;
        if (Testing.isPlayerOnThePath)
        {
            grid.GetXY(player.transform.position, out Px, out Py);
        }
        else
        {
            grid.GetXY(Testing.playerClosestPathNodePosition, out Px, out Py);
        }

        if (!grid.GetGridObject(transform.position).isWalkable)
        {
            Vector3 nearestWalkableNodePos = Testing.pathfinding.getNearestWalkableNodePosition(gameObject);
            grid.GetXY(nearestWalkableNodePos, out x, out y);
        }
        else
        {
            grid.GetXY(transform.position, out x, out y); 
        }
        List<PathNode> path = Testing.pathfinding.FindPath(x, y, Px, Py);
        return path;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            if (!stuckAtEachOtherTimer.IsRunning)
            {
                stuckAtEachOtherTimer.Start();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            collidedWithWall = true;
            wallCollisionCooldown = true;
            isPathingAroundWall = true;
            pathingAroundWallTimer.Start();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            stuckAtEachOtherTimer.Stop();
            stuckAtEachOtherTimer.Reset();
        }
    }
}
