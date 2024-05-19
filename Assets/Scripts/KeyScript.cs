using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class KeyScript : MonoBehaviour
{
    Vector3 originPosition;
    Vector3 endPosition;
    private float lerpFactor = 0f;
    private int direction = 1;

    private void Start()
    {
        originPosition = transform.position;
        endPosition = originPosition + Vector3.up * 2f;
    }
    private void Update()
    {
        transform.position = Vector3.Lerp(originPosition, endPosition, lerpFactor);
        lerpFactor += Time.deltaTime * direction;
        if(lerpFactor >= 1f || lerpFactor <= 0f)
        {
            direction *= -1;
            Mathf.Clamp01(lerpFactor);
        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameController.keysCollected++;
        GameController.current.FireEvent("keyPicked");
        Destroy(gameObject);
    }
}
