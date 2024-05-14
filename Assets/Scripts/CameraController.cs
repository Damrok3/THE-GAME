using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform playerTr;
    public float camOffset;
    public float smoothFactor;
    public Vector3 velocity;
    void Start()
    {
        playerTr = GameObject.Find("player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerPos = playerTr.position;
        Vector3 mousePos = MyFunctions.GetMouseWorldPosition();
        Vector3 mouseDir = (mousePos - playerPos);
        Vector3 newPos = new Vector3(playerPos.x, playerPos.y,transform.position.z) + Vector3.ClampMagnitude(mouseDir, camOffset);
        transform.position =  Vector3.Lerp(transform.position, newPos, Time.deltaTime * smoothFactor);
    }
}
