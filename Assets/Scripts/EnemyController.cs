using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GameObject player;

    private EnemyController collidedEnemyThatIsStopped = null;
    private EnemyController collidedEnemyThatIsSeen = null;

    private Rigidbody2D rb;

    private Grid<PathNode> grid;

    private List<PathNode> path;

    private Animator anim;

    private Stopwatch stuckAtEachOtherTimer = new Stopwatch();
    private Stopwatch stuckAtDoorTimer = new Stopwatch();
    private Stopwatch pathingAroundDoorTimer = new Stopwatch();
    private Stopwatch enemyCollisionCooldown = new Stopwatch();
    private Stopwatch pathingAroundWallTimer = new Stopwatch();


    private bool canSeePlayer = false;
    private bool collidedWithWall = false;
    private bool wallCollisionCooldown = false;
    private bool isPathingAroundWall = false;
    private bool wentRight = false;
    private bool wentDown = false;
    private bool wentLeft = false;
    private bool wentUp = false;
    private bool waitingAtPost = false;
    private bool pathingToPost = false;
    private float slerpSpeed = 5f;
    private float playerDist = 0f;

    private GameObject[] posts;
    private GameObject currentPost = null;
    private List<GameObject> visitedPosts = new List<GameObject>();

    public bool isSeenByPlayer = false;
    public bool shouldStop = false;
    public bool shouldPathAroundDoor = false;
    public int id;
    public int howManyTimesSeen;
    public int annoyanceLevel;
    public float enemySpeed;
    public float enemyRange;
    public string enemyName;

    private RaycastHit2D raycastHit;

    private Vector3 playerDirection;
    private Vector3 direction;



    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("player");
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        posts = GameObject.FindGameObjectsWithTag("enemyPost");
        GameController.current.GameEvent += PlayGrowl;
    }

    private void FixedUpdate()
    {
        if (howManyTimesSeen <= annoyanceLevel)
        {
            CheckIfCanSeePlayer();
        }
    }


    // Update is called once per frame
    void Update()
    {
        playerDist = (transform.position - player.transform.position).magnitude;
        if (shouldPathTowardsPlayer())
        {
            path = FindPath(player);
        }
        else if (ShouldPathToPost())
        {
            pathingToPost = true;
            float minDist = float.MaxValue;
            if(currentPost == null)
            {
                foreach (GameObject post in posts)
                {
                    // check if new post isn't one that already had been visited
                    if(visitedPosts.Contains(post))
                    {
                        continue;
                    }
                    // check if other enemy is not assigned to that post
                    if (post.GetComponent<PostController>().assignedEnemyId != 0)
                    { 
                        continue;
                    }
                    if (post.GetComponent<PostController>().isSeenByPlayer)
                    {
                        continue;
                    }
                    else
                    {
                        float dist = (transform.position - post.transform.position).sqrMagnitude;
                        if(dist < minDist)
                        {
                            minDist = dist;
                            currentPost = post;
                        }
                    }
                }
                currentPost.GetComponent<PostController>().assignedEnemyId = id;
            }
            if(howManyTimesSeen > annoyanceLevel && !isSeenByPlayer && playerDist > 15f)
            {
                transform.position = currentPost.transform.position;
                transform.rotation = currentPost.transform.rotation;
            }
            else
            {
                path = FindPath(currentPost);
            }
        }
        DebugPath();
        Move();
        if (!isSeenByPlayer && !waitingAtPost)
        {
            Rotate();
        }
        ManageCollissions();
        ClearTraversedPath();
        ManagePosts();

    }
    private void PlayGrowl(object sender, GameController.EventArgs a)
    {
        if (a.eventName == "playerHurt" && a.id == id)
        {
            AudioSource audio = gameObject.GetComponent<AudioSource>();
            audio.PlayOneShot(audio.clip);
        }
    }

    private void Rotate()
    {
        float time = Time.deltaTime * slerpSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, MyFunctions.GetVectorAngle(direction))), time);
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
        if (stuckAtEachOtherTimer.ElapsedMilliseconds > 500)
        {
            enemyCollisionCooldown.Start();
            gameObject.layer = layerIgnoreCollision;
        }
        if (enemyCollisionCooldown.ElapsedMilliseconds > 500)
        {
            enemyCollisionCooldown.Stop();
            enemyCollisionCooldown.Reset();
            gameObject.layer = layerDefault;
        }
        if (collidedEnemyThatIsSeen != null && shouldStop && !collidedEnemyThatIsSeen.isSeenByPlayer)
        {
            shouldStop = false;
            collidedEnemyThatIsSeen = null;
        }
        if (collidedEnemyThatIsStopped != null && shouldStop && !collidedEnemyThatIsStopped.shouldStop)
        {
            shouldStop = false;
            collidedEnemyThatIsStopped = null;
        }
        if (stuckAtDoorTimer.ElapsedMilliseconds > 1500)
        {
            shouldPathAroundDoor = true;
            pathingAroundDoorTimer.Start();
        }
        if (pathingAroundDoorTimer.ElapsedMilliseconds > 1500)
        {
            shouldPathAroundDoor = false;
            path = null;
            if (!wentRight) wentRight = true;
            else if (!wentDown) wentDown = true;
            else if (!wentLeft) wentLeft = true;
            else if (!wentUp) wentUp = true;
            else
            {
                wentRight = false;
                wentDown = false;
                wentLeft = false;
                wentUp = false;
            }
            pathingAroundDoorTimer.Stop();
            pathingAroundDoorTimer.Reset();
        }
    }

    private bool shouldPathTowardsPlayer()
    {
        if (howManyTimesSeen > annoyanceLevel) return false;
        if (waitingAtPost) return false;
        if (!canSeePlayer && path == null)
        {
            if(playerDist <= enemyRange)
            {
                return true;
            }
        }
        if (!canSeePlayer && path != null && path.Count < 2)
        {
            if (playerDist <= enemyRange)
            {
                return true;
            }
        }

        if (collidedWithWall) { collidedWithWall = false; return true; }
        return false;
    }

    private bool ShouldPathToPost()
    {
        if (howManyTimesSeen > annoyanceLevel + 1) howManyTimesSeen = 0;
        if (waitingAtPost) return false;
        if (path == null && playerDist > enemyRange) return true;
        if (path == null && howManyTimesSeen > annoyanceLevel && !isSeenByPlayer) return true;
        return false;
    }

    private void ManagePosts()
    {
        foreach (GameObject post in posts)
        {
            if(pathingToPost)
            {
                Vector3 postPos = currentPost.transform.position;
                float dist = (postPos - transform.position).magnitude;
                if (dist < 10f && waitingAtPost == false)
                {
                    StartCoroutine(WaitingAtPost());
                }
            }
        }
        if(visitedPosts.Count > 2)
        {
            visitedPosts.RemoveAt(0);
        }
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
    private void Move()
    {
        if (canSeePlayer && howManyTimesSeen <= annoyanceLevel && playerDist <= enemyRange)
        {
            direction = (player.transform.position - transform.position);
        }
        else if (path != null && path.Count >= 2)
        {
            Vector3 vec3Node = GameController.pathfinding.NodeToVector3(path[1]);
            direction = (vec3Node - transform.position) + new Vector3(GameController.cellSize / 2, GameController.cellSize / 2, 0f);
        }
        else
        {
            direction = Vector3.zero;
        }
        if (shouldPathAroundDoor)
        {
            if (!wentRight)
            {
                direction += Vector3.right * 10;
            }
            else if (!wentDown)
            {
                direction += Vector3.down * 10;
            }
            else if (!wentLeft)
            {
                direction += Vector3.left * 10;
            }
            else if (!wentUp)
            {
                direction += Vector3.up * 10;
            }
        }

        UnityEngine.Debug.DrawLine(transform.position, transform.position + direction, Color.red);

        if (direction != Vector3.zero && !isSeenByPlayer && !shouldStop)
        {
            anim.SetBool("isChasing", true);
            rb.mass = 1f;
            rb.AddForce(direction.normalized * enemySpeed * Time.deltaTime, ForceMode2D.Force);
        }
        else if (isSeenByPlayer || shouldStop)
        {
            anim.SetBool("isChasing", false);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = 0f;
            rb.mass = 1000f;
        }

    }
    private void DebugPath()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                UnityEngine.Debug.DrawLine(new Vector3(path[i].x, path[i].y) * GameController.cellSize + Vector3.one * GameController.cellSize / 2,
                new Vector3(path[i + 1].x, path[i + 1].y) * GameController.cellSize + Vector3.one * GameController.cellSize / 2,
                Color.green);
            }
        }
    }
    private void ClearTraversedPath()
    {
        float dist = float.MaxValue;
        if (path != null && path.Count < 2)
        {
            path = null;
        }
        if(path != null)
        {
            dist = (GameController.pathfinding.NodeToVector3(path[1]) - transform.position).magnitude;
        }
        if(enemyName == "box")
        {
            if (path != null && dist < 7f)
            {
                path.Remove(path[0]);
                path.Remove(path[0]);
            }
        }
        else
        {
            if (path != null && dist < 4.5f)
            {
                path.Remove(path[0]);
                path.Remove(path[0]);
            }
        }
        if (path != null && isPathingAroundWall)
        {
            path.Remove(path[0]);
            isPathingAroundWall = false;
        }
    }
    private List<PathNode> FindPath(GameObject obj)
    {
        grid = GameController.pathfinding.GetGrid();
        int Px, Py;
        int x, y;
        if (GameController.CheckIfObjectOnPath(obj))
        {
            grid.GetXY(obj.transform.position, out Px, out Py);
        }
        else
        {
            grid.GetXY(GameController.pathfinding.getNearestWalkableNodePosition(obj), out Px, out Py);
        }

        if (!grid.GetGridObject(transform.position).isWalkable)
        {
            Vector3 nearestWalkableNodePos = GameController.pathfinding.getNearestWalkableNodePosition(gameObject);
            grid.GetXY(nearestWalkableNodePos, out x, out y);
        }
        else
        {
            grid.GetXY(transform.position, out x, out y);
        }
        List<PathNode> path = GameController.pathfinding.FindPath(x, y, Px, Py);
        return path;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            EnemyController collisionObj = collision.gameObject.GetComponent<EnemyController>();

            // if not seen by player and collided enemy is seen, then stop moving
            if (!isSeenByPlayer && collisionObj.isSeenByPlayer)
            {
                collidedEnemyThatIsSeen = collisionObj;
                shouldStop = true;
            }
            // if not seen by player and not stopped and collided enemy is stopped, then stop moving
            if (!isSeenByPlayer && !shouldStop && collisionObj.shouldStop)
            {
                collidedEnemyThatIsStopped = collisionObj;
                shouldStop = true;
            }
            if (!stuckAtEachOtherTimer.IsRunning && !isSeenByPlayer && !shouldStop && !collisionObj.isSeenByPlayer && !collisionObj.shouldStop)
            {
                stuckAtEachOtherTimer.Start();
            }
        }
        if (collision.gameObject.CompareTag("door"))
        {
            if (!stuckAtDoorTimer.IsRunning)
            {
                stuckAtDoorTimer.Start();
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
        if (collision.gameObject.CompareTag("door"))
        {
            stuckAtDoorTimer.Stop();
            stuckAtDoorTimer.Reset();
        }
    }

    IEnumerator WaitingAtPost()
    {
        PostController currentPostController = currentPost.GetComponent<PostController>();
        transform.position = currentPostController.transform.position;
        transform.rotation = currentPostController.transform.rotation;
        waitingAtPost = true;
        shouldStop = true;
     
        yield return new WaitForSeconds(10.0f);

        currentPostController.assignedEnemyId = 0;
        if(visitedPosts.Contains(currentPost) == false)
        {
            visitedPosts.Add(currentPost);
        }
        currentPost = null;
        howManyTimesSeen = 0;
        shouldStop = false;
        waitingAtPost = false;
        pathingToPost = false;
    }
}
