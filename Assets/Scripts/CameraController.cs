using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform playerTr;

    public float camOffset;

    [Range(0.0f, 10.0f)]
    public float smoothFactor;

    [Range(0.0f, 10.0f)]
    public float shakeIntensity;
    
    [Range(0.0f, 10.0f)]
    public float shakeSpeed;

    public uint amountOfShakes;


    private bool isShakeCrOn = false;

    void Start()
    {
        playerTr = GameObject.Find("player").GetComponent<Transform>();
        GameController.current.GameEvent += ScreenShake;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerPos = playerTr.position;
        Vector3 mousePos = MyFunctions.GetMouseWorldPosition();
        Vector3 mouseDir = (mousePos - playerPos);
        Vector3 newPos = new Vector3(playerPos.x, playerPos.y, -50f) + Vector3.ClampMagnitude(mouseDir, camOffset);
        if(!isShakeCrOn)
        {
            transform.position =  Vector3.Lerp(transform.position, newPos, Time.deltaTime * smoothFactor);
        }
    }

    private void ScreenShake(object sender, GameController.EventArgs a)
    {
        if(a.eventName == "playerHurt")
        {
            if(!isShakeCrOn)
            {
                StartCoroutine(ScreenShakeCoroutine());
            }
        }
    }

    IEnumerator ScreenShakeCoroutine()
    {
        isShakeCrOn = true;
        for (int i = 0; i < amountOfShakes; i++)
        {
            yield return new WaitForSeconds(shakeSpeed/10.0f);
            transform.position = transform.position + UnityEngine.Random.insideUnitSphere * shakeIntensity;
        }
        isShakeCrOn = false;

        yield return null;
    }
}
