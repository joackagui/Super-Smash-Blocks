using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float faceRightYRotation = 90f;
    [SerializeField] private float faceLeftYRotation = -90f;

    private float damageReceived = 0;
    private bool isWalking = false;
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
    private Animator animator;
    private float fixedZPosition;
    private int attackComboIndex = 0;
    private int specialComboIndex = 0;
    private float comboResetTime = 1.2f;
    private float lastAttackTime = 0f;
    private Quaternion desiredRotation = Quaternion.identity;
    private static readonly int PARAM_IS_GROUNDED     = Animator.StringToHash("isGrounded");
    private static readonly int PARAM_IS_WALKING      = Animator.StringToHash("isWalking");
    private static readonly int PARAM_ATTACK_COMBO    = Animator.StringToHash("attackComboIndex");
    private static readonly int PARAM_SPECIAL_COMBO   = Animator.StringToHash("specialComboIndex");
    private static readonly int PARAM_JUMP1           = Animator.StringToHash("Jump1");
    private static readonly int PARAM_JUMP2           = Animator.StringToHash("Jump2");
    private static readonly int PARAM_ATTACK1_GROUND  = Animator.StringToHash("Attack1Ground");
    private static readonly int PARAM_ATTACK1_AIR     = Animator.StringToHash("Attack1Air");
    private static readonly int PARAM_ATTACK2_GROUND  = Animator.StringToHash("Attack2Ground");
    private static readonly int PARAM_ATTACK2_AIR     = Animator.StringToHash("Attack2Air");
    private static readonly int PARAM_DODGE           = Animator.StringToHash("Dodge");
    private static readonly int PARAM_HURT            = Animator.StringToHash("Hurt");
    private Hitbox[] hitboxes;

    public void SetOwner(Player player) { owner = player; }

    public void SetInitialFacing(int direction)
    {
        int clamped = direction >= 0 ? 1 : -1;
        facingDirection = 0;
        FaceDirection(clamped);
    }

    public virtual void Move(Vector2 direction)
    {
        moveInput = direction;
        isWalking = Mathf.Abs(direction.x) > 0.01f;

        if (direction.x > 0.01f)       FaceDirection(1);
        else if (direction.x < -0.01f) FaceDirection(-1);
    }

    public virtual void Jump()
    {
        if (isGrounded || jumpsRemaining > 0)
        {
            ReproduceJumpClip();
            isJumping = true;
            isGrounded = false;

            ClearAllTriggers(); // evita triggers acumulados

            if (jumpsRemaining == 2)
                animator.SetTrigger(PARAM_JUMP1);
            else if (jumpsRemaining == 1)
                animator.SetTrigger(PARAM_JUMP2);

            jumpsRemaining--;

            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public virtual void Attack1()
    {
        ReproduceAttack1Clip();
        lastAttackTime = Time.time;
        ClearAllTriggers();

        if (isGrounded)
        {
            animator.SetInteger(PARAM_ATTACK_COMBO, attackComboIndex);
            animator.SetTrigger(PARAM_ATTACK1_GROUND);
            attackComboIndex = attackComboIndex == 0 ? 1 : 0;
        }
        else
        {
            animator.SetTrigger(PARAM_ATTACK1_AIR);
        }
    }

    public virtual void Attack2()
    {
        ReproduceAttack2Clip();
        lastAttackTime = Time.time;
        ClearAllTriggers();

        if (isGrounded)
        {
            animator.SetInteger(PARAM_SPECIAL_COMBO, specialComboIndex);
            animator.SetTrigger(PARAM_ATTACK2_GROUND);
            specialComboIndex = specialComboIndex == 0 ? 1 : 0;
        }
        else
        {
            animator.SetTrigger(PARAM_ATTACK2_AIR);
        }
    }

    public void TakeDamage(float dmg, Vector3 attackerPosition)
    {
        if (isInvulnerable || isDead) return;

        ReproduceHurtClip();
        damageReceived += dmg;
        isHurt = true;
        ClearAllTriggers();
        animator.SetTrigger(PARAM_HURT);

        Debug.Log($"[{gameObject.name}] TakeDamage: +{dmg} | damageReceived total: {damageReceived}");

        ApplyKnockback(attackerPosition);
    }

    private void ApplyKnockback(Vector3 attackerPosition)
    {
        if (rb == null) return;

        // Dirección solo en X — el juego es lateral
        float directionX = transform.position.x - attackerPosition.x;
        directionX = directionX >= 0 ? 1f : -1f; // normaliza a exactamente 1 o -1

        float force = 15f + damageReceived * 0.25f;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(new Vector3(directionX * force, 0f, 0f), ForceMode.Impulse);
    }

    public virtual void Dodge()
    {
        ClearAllTriggers();
        animator.SetTrigger(PARAM_DODGE);
    }

    private void ClearAllTriggers()
    {
        animator.ResetTrigger(PARAM_JUMP1);
        animator.ResetTrigger(PARAM_JUMP2);
        animator.ResetTrigger(PARAM_ATTACK1_GROUND);
        animator.ResetTrigger(PARAM_ATTACK1_AIR);
        animator.ResetTrigger(PARAM_ATTACK2_GROUND);
        animator.ResetTrigger(PARAM_ATTACK2_AIR);
        animator.ResetTrigger(PARAM_DODGE);
        animator.ResetTrigger(PARAM_HURT);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (animator != null)
        {
            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        desiredRotation = transform.rotation;
        hitboxes = GetComponentsInChildren<Hitbox>(includeInactive: true);
        foreach (var hb in hitboxes)
        {
            hb.SetOwner(this);
            hb.Deactivate();
        }
        fixedZPosition = transform.position.z;
    }

    void Update()
    {
        if (Time.time - lastAttackTime > comboResetTime)
        {
            attackComboIndex = 0;
            specialComboIndex = 0;
        }
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();

        if (rb != null)
            rb.MoveRotation(desiredRotation);

        if (animator != null)
        {
            animator.SetBool(PARAM_IS_GROUNDED, isGrounded);
            animator.SetBool(PARAM_IS_WALKING, isWalking);
        }

        Vector3 pos = rb.position;
        pos.z = fixedZPosition;
        rb.MovePosition(pos);
    }
    public void ActivateHitboxByName(string hitboxName)
    {
        foreach (var hb in hitboxes)
        {
            if (hb.gameObject.name == hitboxName)
                hb.Activate();
        }
    }

    public void DeactivateAllHitboxes()
    {
        foreach (var hb in hitboxes)
            hb.Deactivate();
    }

    void OnAnimatorMove()
    {
        if (rb == null || animator == null || !animator.enabled) return;
        transform.position = rb.position;
        transform.rotation = desiredRotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Ground"))
        { Land(); return; }

        if (collision.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint c in collision.contacts)
                if (Vector3.Dot(c.normal, Vector3.up) > 0.5f) { Land(); break; }
        }

        if (collision.gameObject.CompareTag("Barrier"))
            Die();
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint c in collision.contacts)
                if (Vector3.Dot(c.normal, Vector3.up) > 0.5f) { isGrounded = true; return; }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        ReproduceDeathClip();
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        if (owner != null)
            owner.HandleCharacterDeath(onRespawnReady: () => StartCoroutine(RespawnSequence()));
        yield return new WaitForSeconds(0.5f);
        if (MusicManager.Instance != null) MusicManager.Instance.PlayCharacterDeath();
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(2.5f);
        Respawn();
    }

    private void Respawn()
    {
        isDead = false;
        isGrounded = true;
        jumpsRemaining = 2;
        isAttacking = false;
        isHurt = false;
        damageReceived = 0;

        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        if (owner != null) owner.SpawnCharacter(this);
        if (MusicManager.Instance != null) MusicManager.Instance.PlayCharacterRespawn();
    }

    private void ApplyHorizontalMovement()
    {
        if (rb == null) return;
        Vector3 vel = rb.linearVelocity;
        vel.x = moveInput.x * speed;
        rb.linearVelocity = vel;
    }

    private void FaceDirection(int direction)
    {
        if (facingDirection == direction) return;
        facingDirection = direction;
        float yRot = direction > 0 ? faceRightYRotation : faceLeftYRotation;
        desiredRotation = Quaternion.Euler(0f, yRot, 0f);
        if (rb != null) rb.MoveRotation(desiredRotation);
        else transform.rotation = desiredRotation;
    }

    void Land()
    {
        isJumping = false;
        isGrounded = true;
        jumpsRemaining = 2;
    }

    public void ReproduceJumpClip()    { if (jumpClip    != null) sfxSource.PlayOneShot(jumpClip); }
    public void ReproduceAttack1Clip() { if (attack1Clip != null) sfxSource.PlayOneShot(attack1Clip); }
    public void ReproduceAttack2Clip() { if (attack2Clip != null) sfxSource.PlayOneShot(attack2Clip); }
    public void ReproduceHurtClip()    { if (hurtClip    != null) sfxSource.PlayOneShot(hurtClip); }
    public void ReproduceDeathClip()   { if (deathClip   != null) sfxSource.PlayOneShot(deathClip); }
}