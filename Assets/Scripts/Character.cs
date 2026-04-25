using UnityEngine;

public class Character: MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float faceRightYRotation = 90f;
    [SerializeField] private float faceLeftYRotation = -90f;

    private float damageReceived = 0;
    private bool isMoving = false;
    private bool isGrounded = true;
    private bool isJumping = false;
    private int jumpsRemaining = 2;
    private bool isAttacking = false;
    private bool isHurt = false;

    private bool isInvulnerable = false;
    private float invulnerabilityDuration = 1.5f;
    private float invulnerabilityTimer = 0f;

    public AudioClip hurtClip;
    public AudioClip attack1Clip;
    public AudioClip attack2Clip;
    public AudioClip jumpClip;

    public AudioClip deathClip;
    private AudioSource sfxSource;
    private Rigidbody rb;

    private Vector2 moveInput;
    private int facingDirection = 1;
    private bool isDead = false;
    private Player owner;

    public void SetOwner(Player player)
    {
        owner = player;
    }

    public void SetInitialFacing(int direction)
    {
        int clampedDirection = direction >= 0 ? 1 : -1;
        facingDirection = 0;
        FaceDirection(clampedDirection);
    }


    public virtual void Move(Vector2 direction)
    {
        moveInput = direction;
        isMoving = Mathf.Abs(direction.x) > 0.01f;

        if (direction.x > 0.01f)
        {
            FaceDirection(1);
        }
        else if (direction.x < -0.01f)
        {
            FaceDirection(-1);
        }
    }

    public virtual void Jump()
    {
        if (isGrounded || jumpsRemaining > 0)
        {
            ReproduceJumpClip();
            isJumping = true;
            isGrounded = false;
            jumpsRemaining--;
            Vector3 velocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

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
        ApplyHorizontalMovement();
    }

    void Knockback(Vector2 direction, float force)
    {
        // apply knockback force
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isJumping = false;
            isGrounded = true;
            jumpsRemaining = 2;
        } else if (collision.gameObject.CompareTag("Barrier"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        ReproduceDeathClip();

        if (owner != null)
        {
            owner.HandleCharacterDeath();
        }

        Destroy(gameObject, 0.1f);
    }

    private void ApplyHorizontalMovement()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveInput.x * speed;
        rb.linearVelocity = velocity;
    }

    private void FaceDirection(int direction)
    {
        if (facingDirection == direction)
        {
            return;
        }

        facingDirection = direction;
        float yRotation = direction > 0 ? faceRightYRotation : faceLeftYRotation;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void ReproduceJumpClip()
    {
        if (jumpClip != null)
        {
            sfxSource.PlayOneShot(jumpClip);
        }
    }
    public void ReproduceAttack1Clip()
    {
        if (attack1Clip != null)
        {
            sfxSource.PlayOneShot(attack1Clip);
        }
    }
    public void ReproduceAttack2Clip()
    {
        if (attack2Clip != null)
        {
            sfxSource.PlayOneShot(attack2Clip);
        }
    }
    public void ReproduceHurtClip()
    {
        if (hurtClip != null)
        {
            sfxSource.PlayOneShot(hurtClip);
        }
    }

    public void ReproduceDeathClip()
    {
        if (deathClip != null)
        {
            sfxSource.PlayOneShot(deathClip);
        }
    }
}
