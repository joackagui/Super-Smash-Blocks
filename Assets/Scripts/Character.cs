using UnityEngine;

public class Character: MonoBehaviour
{
    private float speed;
    private float damageReceived = 0;
    private float damageDealt = 0;
    public int lives = 3;
    private bool isMoving = false;
    private bool isGrounded = true;
    private bool isJumping = false;
    private int jumpsRemaining = 2;
    private bool isAttacking = false;
    private bool isHurt = false;

    public AudioClip hurtClip;
    public AudioClip attack1Clip;
    public AudioClip attack2Clip;
    public AudioClip jumpClip;

    public AudioClip deathClip;
    private AudioSource sfxSource;


    public virtual void Move(Vector2 direction)
    {
        isMoving = true;
    }

    public virtual void Jump()
    {
        if(isGrounded)
        {
            ReproduceJumpClip();
            isJumping = true;
            isGrounded = false;
            jumpsRemaining--;
        } else {
            if (jumpsRemaining > 0)
            {
                isJumping = true;
                jumpsRemaining--;
            }
        }
    }

    public virtual void Attack1()
    {
        ReproduceAttack1Clip();
        // base attack
    }

    public virtual void Attack2()
    {
        ReproduceAttack2Clip();
        // special attack
    }

    public void TakeDamage(float dmg)
    {
        ReproduceHurtClip();
        damageReceived += dmg;
        isHurt = true;
    }

    void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        //new GameObject("Hitbox").AddComponent<BoxCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!isGrounded && jumpsRemaining == 2){
            jumpsRemaining = 1;
        }
    }

    void Knockback(Vector2 direction, float force)
    {
        // apply knockback force
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isJumping = false;
            isGrounded = true;
            jumpsRemaining = 2;
        }
    }

    public void ReproduceJumpClip()
    {
        sfxSource.PlayOneShot(jumpClip);
    }
    public void ReproduceAttack1Clip()
    {
        sfxSource.PlayOneShot(attack1Clip);
    }
    public void ReproduceAttack2Clip()
    {
        sfxSource.PlayOneShot(attack2Clip);
    }
    public void ReproduceHurtClip()
    {
        sfxSource.PlayOneShot(hurtClip);
    }

    public void ReproduceDeathClip()
    {
        sfxSource.PlayOneShot(deathClip);
    }
}
