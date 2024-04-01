using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVmanager : MonoBehaviour
{
    public float fovAngle = 90f;

    public Transform fovPoint;
    public List<GameObject> target;

    public float range = 8f;

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject t in target)
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
                        Debug.DrawRay(transform.position, dir, Color.cyan);
                        t.GetComponent<EnemyController>().isSeenByPlayer = true;
                    }       

                } 
            }
            else
            {
                t.GetComponent<EnemyController>().isSeenByPlayer = false;
            }
        }
        
    }
}
