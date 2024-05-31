using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVmanager : MonoBehaviour
{
    public float fovAngle = 90f;
    public float range = 8f;
    public Transform fovPoint;

    private List<GameObject> targets = new List<GameObject>();
    private Dictionary<GameObject, Coroutine> cooldownCoroutines = new Dictionary<GameObject, Coroutine>();
    private RaycastHit2D hit;

    private void Start()
    {
        targets.AddRange(GameObject.FindGameObjectsWithTag("enemy"));
        targets.AddRange(GameObject.FindGameObjectsWithTag("enemyPost"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (GameObject t in targets)
        {
            Vector2 dir = t.transform.position - fovPoint.position;
            float angle = Vector3.Angle(dir, fovPoint.up);
            hit = Physics2D.Raycast(fovPoint.position, dir, range);

            if (angle < fovAngle / 2)
            {
                if (hit.collider != null)
                {
                    if (t.CompareTag("enemy") && hit.collider.gameObject.CompareTag("enemy"))
                    {
                        if (!t.GetComponent<EnemyController>().isSeenByPlayer)
                        {
                            t.GetComponent<EnemyController>().howManyTimesSeen++;
                        }
                        StopCooldownCoroutine(t);
                        t.GetComponent<EnemyController>().isSeenByPlayer = true;
                    }
                    else if (t.CompareTag("enemyPost") && hit.collider.gameObject.CompareTag("enemyPost"))
                    {
                        StopCooldownCoroutine(t);
                        t.GetComponent<PostController>().isSeenByPlayer = true;
                    }
                    else
                    {
                        StartCooldownCoroutine(t);
                    }
                }
            }
            else
            {
                StartCooldownCoroutine(t);
            }
        }
    }

    void StartCooldownCoroutine(GameObject t)
    {
        if (!cooldownCoroutines.ContainsKey(t))
        {
            Coroutine coroutine = StartCoroutine(SeenByPlayerCooldown(t));
            cooldownCoroutines.Add(t, coroutine);
        }
    }

    void StopCooldownCoroutine(GameObject t)
    {
        if (cooldownCoroutines.ContainsKey(t))
        {
            StopCoroutine(cooldownCoroutines[t]);
            cooldownCoroutines.Remove(t);
        }
    }

    IEnumerator SeenByPlayerCooldown(GameObject t)
    {
        yield return new WaitForSeconds(2f);
        if (t.CompareTag("enemy"))
        {
            t.GetComponent<EnemyController>().isSeenByPlayer = false;
        }
        else if (t.CompareTag("enemyPost"))
        {
            t.GetComponent<PostController>().isSeenByPlayer = false;
        }
        cooldownCoroutines.Remove(t);
    }

}