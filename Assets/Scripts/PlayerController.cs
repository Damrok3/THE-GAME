using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    public float playerStamina;
    private float finalSpeed;
    public List<AudioClip> clips;
    private AudioSource audioSrc;
    private Animator anim;
    private Rigidbody2D rb;
    private Stopwatch audioDelay = new Stopwatch();
    private Stopwatch sprintCooldown = new Stopwatch();
    public Slider staminaBar;

    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();  
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        audioDelay.Start();
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
            ManagePlayerLookDirection();
    }

    void Update()
    {
        ManagePlayerMovement();
        
    }

    private void ManagePlayerMovement()
    {

        ManagePLayerSprint();

        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");
        if(hAxis != 0 || vAxis != 0)
        {
            if (audioDelay.ElapsedMilliseconds > 750)
            {
                audioSrc.PlayOneShot(clips[Random.Range(0, clips.Count)]);
                audioDelay.Restart();
            }
            anim.SetBool("isWalking", true);
            Vector2 vVector = Vector2.up * vAxis;
            Vector2 hVector = Vector2.right * hAxis;
            
            rb.AddForce((vVector + hVector).normalized * finalSpeed * Time.deltaTime, ForceMode2D.Force);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }
    private void ManagePLayerSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !sprintCooldown.IsRunning)
        {
            finalSpeed = speed * 2;
            audioSrc.pitch = 1.3f;
            playerStamina -= Time.deltaTime * 15f;
        }
        else
        {
            playerStamina += Time.deltaTime * 5f;
            if (sprintCooldown.IsRunning)
            {
                finalSpeed = speed / 2;
                audioSrc.pitch = 1;
            }
            else
            {
                finalSpeed = speed;
                audioSrc.pitch = 1;
            }
        }

        

        playerStamina = Mathf.Clamp(playerStamina, 0, 100);
        staminaBar.value = playerStamina;

        if (playerStamina <= 0)
        {
            sprintCooldown.Start();
        }
        if (sprintCooldown.ElapsedMilliseconds >= 4000)
        {
            sprintCooldown.Stop();
            sprintCooldown.Reset();
        }
    }
    private void ManagePlayerLookDirection()
    {
        rb.angularVelocity = 0f;
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, GetPlayerAngle() - 90)); ; 
        if (Input.GetKey(KeyCode.LeftShift))
        {
            float angle = Vector3.SignedAngle(Mathf.Round(Input.GetAxis("Horizontal")) * -Vector3.right + Mathf.Round(Input.GetAxis("Vertical")) * Vector3.up, Vector3.up, Vector3.forward);
            rotation = Quaternion.Euler(0f,0f,angle);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }


    private float GetPlayerAngle()
    {
        Vector2 mousePos = MyFunctions.GetMouseWorldPosition();
        Vector2 playerPos = transform.position;
        Vector2 directionVec = mousePos - playerPos;
        float angleInRadians = Mathf.Atan2(directionVec.y, directionVec.x);
        return angleInRadians * Mathf.Rad2Deg;
    }

 
}
