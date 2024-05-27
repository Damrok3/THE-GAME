using System.Collections;
using UnityEngine;

public class FOVmanager : MonoBehaviour
{
    public float fovAngle = 90f;

    public Transform fovPoint;
    private GameObject[] targets;

    public float range = 8f;

    private void Start()
    {
        targets = GameObject.FindGameObjectsWithTag("enemy");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject t in targets)
        {
            Vector2 dir = t.transform.position - fovPoint.position;
            float angle = Vector3.Angle(dir, fovPoint.up);
            RaycastHit2D hit = Physics2D.Raycast(fovPoint.position, dir, range);

            if (angle < fovAngle / 2)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.CompareTag("enemy"))
                    {
                        if(t.GetComponent<EnemyController>().isSeenByPlayer == false)
                        {
                            t.GetComponent<EnemyController>().howManyTimesSeen++;
                        }
                        t.GetComponent<EnemyController>().isSeenByPlayer = true;
                        StopAllCoroutines();
                    }
                    else
                    {
                         StartCoroutine(SeenByPlayerCooldown(t));
                    }
                } 
            }
            else
            {
                StartCoroutine(SeenByPlayerCooldown(t)); 
            }
        }
        
    }

    IEnumerator SeenByPlayerCooldown(GameObject t)
    {
        yield return new WaitForSeconds(5f);
        t.GetComponent<EnemyController>().isSeenByPlayer = false;
    }
}
