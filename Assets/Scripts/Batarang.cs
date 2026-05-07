using UnityEngine;

public class Batarang : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private Collider projectileCollider;
    private Character owner;
    private bool hasImpacted;
    private float damage = 9f;
    private float impulse = 10f;
    private Vector3 launchDirection = Vector3.forward;
    [SerializeField] private float lifeTimeSeconds = 6f;
    [SerializeField] private string spinStateName = "Base Layer.Spin";
    [SerializeField] private float overlapHitRadius = 0.35f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        projectileCollider = GetComponent<Collider>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Destroy(gameObject, lifeTimeSeconds);
    }

    void FixedUpdate()
    {
        if (hasImpacted)
        {
            return;
        }

        ScanOverlaps();
    }

    public void Initialize(Character ownerCharacter, Vector3 direction, float dmg = 9f, float imp = 30f)
    {
        owner = ownerCharacter;
        damage = dmg;
        impulse = imp;
        launchDirection = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;

        IgnoreOwnerCollisions();
    }

    public void StartMoving()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(launchDirection * impulse, ForceMode.Impulse);
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Spin", true);

            int spinStateHash = Animator.StringToHash(spinStateName);
            if (animator.HasState(0, spinStateHash))
            {
                animator.Play(spinStateHash, 0, 0f);
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        HandleImpact(collision.collider);
    }

    public void OnTriggerEnter(Collider other)
    {
        HandleImpact(other);
    }

    private void HandleImpact(Collider other)
    {
        if (hasImpacted || other == null)
        {
            return;
        }

        Character character = other.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character == owner)
            {
                return;
            }

            Vector3 attackerPos = owner != null ? owner.transform.position : transform.position;
            character.TakeDamage(damage, attackerPos);
            hasImpacted = true;
            Destroy(gameObject);
            return;
        }

        GameObject otherGameObject = other.gameObject;
        if (otherGameObject.CompareTag("Ground") || otherGameObject.CompareTag("Platform") || otherGameObject.CompareTag("Barrier"))
        {
            hasImpacted = true;
            Destroy(gameObject);
            return;
        }
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null)
        {
            return;
        }

        if (projectileCollider == null)
        {
            projectileCollider = GetComponent<Collider>();
        }

        if (projectileCollider == null)
        {
            return;
        }

        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();
        foreach (Collider ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, ownerCollider, true);
            }
        }
    }

    private void ScanOverlaps()
    {
        Collider[] overlaps = Physics.OverlapSphere(transform.position, overlapHitRadius, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider other in overlaps)
        {
            if (other == null || other == projectileCollider)
            {
                continue;
            }

            HandleImpact(other);
            if (hasImpacted)
            {
                return;
            }
        }
    }
}
