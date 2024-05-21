using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    private bool Iframes = false;
    public Slider staminaBar;
    public Volume globalVolume;

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
                audioSrc.clip = clips[Random.Range(0, clips.Count)];
                audioSrc.Play();
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if(collision.gameObject.CompareTag("enemy") && !Iframes && !enemy.isSeenByPlayer)
        {
            StartCoroutine(ScreenHurtEffect());
            StartCoroutine(PlayerIFrames());
            PushPlayerAway(collision.transform.position, 200f);
            GameController.current.FireEvent("playerHurt", enemy.id);
        }
    }

    private void PushPlayerAway(Vector3 pushDir, float force)
    {
        Vector3 dir = (transform.position - pushDir).normalized;
        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }

    IEnumerator PlayerIFrames()
    {
        int blinks = 20;
        Iframes = true;
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        while (blinks >= 0)
        {
            yield return new WaitForSeconds(0.05f);
            renderer.enabled = !renderer.enabled;
            blinks--;
        }
        renderer.enabled = true;
        Iframes = false;
    }

    IEnumerator ScreenHurtEffect()
    {
        globalVolume.enabled = true;
        yield return new WaitForSeconds(0.1f);
        globalVolume.enabled = false;
    }


}
