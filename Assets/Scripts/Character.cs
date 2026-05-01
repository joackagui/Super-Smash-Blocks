using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    private enum QueuedAttackType
    {
        None,
        Attack1,
        Attack2
    }

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
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
    private RigidbodyConstraints defaultConstraints;
    private int attackComboIndex = 0;
    private int specialComboIndex = 0;
    private float comboResetTime = 1.2f;
    private float lastAttackTime = 0f;
    private Quaternion desiredRotation = Quaternion.identity;
    private QueuedAttackType queuedAttack = QueuedAttackType.None;
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
    private static readonly int PARAM_DEATH           = Animator.StringToHash("Death");
    private static readonly int STATE_BASE_ATTACK_GROUND_1 = Animator.StringToHash("Base Layer.BaseAttackGround1");
    private static readonly int STATE_BASE_ATTACK_GROUND_2 = Animator.StringToHash("Base Layer.BaseAttackGround2");
    private static readonly int STATE_BASE_ATTACK_AIR      = Animator.StringToHash("Base Layer.BaseAttackAir");
    private static readonly int STATE_SPECIAL_ATTACK_GROUND_1 = Animator.StringToHash("Base Layer.SpecialAttackGround1");
    private static readonly int STATE_SPECIAL_ATTACK_GROUND_2 = Animator.StringToHash("Base Layer.SpecialAttackGround2");
    private static readonly int STATE_SPECIAL_ATTACK_AIR      = Animator.StringToHash("Base Layer.SpecialAttackAir");
    private static readonly int STATE_DEATH                = Animator.StringToHash("Base Layer.Death");
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

            ClearAllTriggers();

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
        if (isDead || animator == null) return;

        if (isAttacking)
        {
            queuedAttack = QueuedAttackType.Attack1;
            return;
        }

        ExecuteAttack1();
    }

    public virtual void Attack2()
    {
        if (isDead || animator == null) return;

        if (isAttacking)
        {
            queuedAttack = QueuedAttackType.Attack2;
            return;
        }

        ExecuteAttack2();
    }

    public void TakeDamage(float dmg, Vector3 attackerPosition)
    {
        if (isInvulnerable || isDead) return;

        queuedAttack = QueuedAttackType.None;
        isAttacking = false;
        DeactivateAllHitboxes();
        ReproduceHurtClip();
        damageReceived += dmg;
        isHurt = true;
        isInvulnerable = true;
        ClearAllTriggers();
        animator.SetTrigger(PARAM_HURT);

        Debug.Log($"[{gameObject.name}] TakeDamage: +{dmg} | damageReceived total: {damageReceived}");

        ApplyKnockback(attackerPosition);
        StartCoroutine(HurtRecoverySequence());
    }

    public float GetDamageReceived()
    {
        return damageReceived;
    }

    private void ApplyKnockback(Vector3 attackerPosition)
    {
        if (rb == null) return;

        float directionX = transform.position.x - attackerPosition.x;
        directionX = directionX >= 0f ? 1f : -1f;

        float force = 0.25f * damageReceived;
        float angle = 30f * Mathf.Deg2Rad;

        Vector3 knockbackDirection = new Vector3(
            Mathf.Cos(angle) * directionX,
            Mathf.Sin(angle),
            0f
        ).normalized;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockbackDirection * force, ForceMode.Impulse);
    }


    public virtual void Dodge()
    {
        queuedAttack = QueuedAttackType.None;
        isAttacking = false;
        DeactivateAllHitboxes();
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
        animator.ResetTrigger(PARAM_DEATH);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;

        defaultConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;
        rb.constraints = defaultConstraints;

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
        UpdateAttackState();

        if (Time.time - lastAttackTime > comboResetTime)
        {
            attackComboIndex = 0;
            specialComboIndex = 0;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

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
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint c in collision.contacts)
                if (Vector3.Dot(c.normal, Vector3.up) > 0.5f) { isGrounded = true; return; }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.gameObject.CompareTag("Barrier"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        queuedAttack = QueuedAttackType.None;
        isAttacking = false;
        isHurt = false;
        isInvulnerable = true;
        moveInput = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        DeactivateAllHitboxes();
        float deathAnimationDuration = TriggerDeathAnimationIfAvailable();
        ReproduceDeathClip();
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayCharacterDeath();
        }
        StartCoroutine(DeathSequence(deathAnimationDuration));
    }

    private IEnumerator DeathSequence(float deathAnimationDuration)
    {
        if (deathAnimationDuration > 0f)
        {
            yield return new WaitForSeconds(deathAnimationDuration);
        }

        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;

        if (owner != null)
        {
            owner.HandleCharacterDeath(onRespawnReady: () => StartCoroutine(RespawnSequence()));
        }
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(0.5f);
        Respawn();
    }

    private void Respawn()
    {
        isDead = false;
        isGrounded = true;
        jumpsRemaining = 2;
        isAttacking = false;
        queuedAttack = QueuedAttackType.None;
        isHurt = false;
        isInvulnerable = false;
        damageReceived = 0;
        DeactivateAllHitboxes();
        moveInput = Vector2.zero;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = defaultConstraints;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        if (owner != null) owner.SpawnCharacter(this);
        if (MusicManager.Instance != null) MusicManager.Instance.PlayCharacterRespawn();
    }

    private void ApplyHorizontalMovement()
    {
        if (rb == null) return;
        if (isHurt) return;

        Vector3 vel = rb.linearVelocity;
        vel.x = moveInput.x * speed;
        rb.linearVelocity = vel;
    }

    private IEnumerator HurtRecoverySequence()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
        isHurt = false;
        isInvulnerable = false;
    }

    private float TriggerDeathAnimationIfAvailable()
    {
        if (animator == null || !HasAnimatorTrigger(PARAM_DEATH))
        {
            return 0f;
        }

        ClearAllTriggers();
        animator.SetTrigger(PARAM_DEATH);

        float deathDuration = GetAnimationClipLength("Death");
        return deathDuration;
    }

    private void ExecuteAttack1()
    {
        ReproduceAttack1Clip();
        lastAttackTime = Time.time;
        isAttacking = true;
        queuedAttack = QueuedAttackType.None;
        DeactivateAllHitboxes();
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

    private void ExecuteAttack2()
    {
        ReproduceAttack2Clip();
        lastAttackTime = Time.time;
        isAttacking = true;
        queuedAttack = QueuedAttackType.None;
        DeactivateAllHitboxes();
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

    private void UpdateAttackState()
    {
        if (animator == null) return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        bool attackStateActive = IsAttackState(currentState);

        if (!attackStateActive && animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            attackStateActive = IsAttackState(nextState);
        }

        if (attackStateActive)
        {
            isAttacking = true;
            return;
        }

        if (!isAttacking) return;

        isAttacking = false;

        if (queuedAttack == QueuedAttackType.Attack1)
        {
            ExecuteAttack1();
        }
        else if (queuedAttack == QueuedAttackType.Attack2)
        {
            ExecuteAttack2();
        }
    }

    private static bool IsAttackState(AnimatorStateInfo stateInfo)
    {
        int stateHash = stateInfo.fullPathHash;
        return stateHash == STATE_BASE_ATTACK_GROUND_1
            || stateHash == STATE_BASE_ATTACK_GROUND_2
            || stateHash == STATE_BASE_ATTACK_AIR
            || stateHash == STATE_SPECIAL_ATTACK_GROUND_1
            || stateHash == STATE_SPECIAL_ATTACK_GROUND_2
            || stateHash == STATE_SPECIAL_ATTACK_AIR;
    }

    private bool HasAnimatorTrigger(int parameterHash)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }

    private float GetAnimationClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip.length;
            }
        }

        return 0f;
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
