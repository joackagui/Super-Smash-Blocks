using UnityEngine;
using UnityEngine.AI;

public class NavMeshEnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private float retargetInterval = 0.12f;
    [SerializeField] private float heartSearchInterval = 0.25f;

    [Header("Chase")]
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float jumpHeightThreshold = 0.85f;
    [SerializeField] private float jumpCooldown = 0.45f;

    [Header("Attacks")]
    [SerializeField] private float attackCooldown = 0.9f;
    [SerializeField] private float specialAttackChance = 0.35f;

    [Header("Raycast")]
    [SerializeField] private float visionDistance = 5f;

    [Header("Return To Platform")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float platformCenterArriveDistance = 0.75f;
    [SerializeField] private float platformNavMeshSampleRadius = 6f;

    [Header("Chase Offset")]
    [SerializeField] private float targetSideOffset = 0.6f;

    [Header("Head Escape")]
    [SerializeField] private float headAlignThreshold = 0.15f;
    [SerializeField] private float headHeightThreshold = 0.5f;
    [SerializeField] private float headPushOffset = 1.2f;
    [SerializeField] private float headEscapeDuration = 0.4f;

    [Header("Random Retreat")]
    [SerializeField] private float retreatIntervalMin = 6f;
    [SerializeField] private float retreatIntervalMax = 12f;
    [SerializeField] private float retreatDuration = 0.4f;
    [SerializeField] private float retreatOffset = 1.0f;

    private NavMeshAgent agent;
    private Character character;
    private Transform target;
    private Transform heartTarget;

    private float nextRetargetTime;
    private float nextHeartSearchTime;
    private float nextAttackTime;
    private float nextJumpTime;

    private bool returningToPlatform;
    private bool hasMainPlatformCenter;
    private Vector3 mainPlatformCenter;
    private Vector3 mainPlatformNavPoint;

    private float headEscapeUntil;
    private float headEscapeDir;

    private float retreatUntil;
    private float retreatDir;
    private float nextRetreatTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        character = GetComponent<Character>();
        CacheMainPlatformCenter();
        nextRetreatTime = Time.time + Random.Range(retreatIntervalMin, retreatIntervalMax);
    }

    private void OnEnable()
    {
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.autoTraverseOffMeshLink = false;
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsSinglePlayer)
        {
            Destroy(this);
            return;
        }

        TrySnapToNavMesh();
        CacheMainPlatformCenter();
        nextRetreatTime = Time.time + Random.Range(retreatIntervalMin, retreatIntervalMax);
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsSinglePlayer) return;
        if (agent == null || character == null) return;

        ResolveTarget();
        UpdateReturnState();

        if (returningToPlatform)
            heartTarget = null;
        else
            FindHeart();

        Vector3 currentTargetPosition;
        Transform currentTarget = null;

        if (returningToPlatform && hasMainPlatformCenter)
        {
            currentTargetPosition = mainPlatformNavPoint;
        }
        else
        {
            currentTarget = heartTarget != null ? heartTarget : target;
            if (currentTarget == null) return;
            currentTargetPosition = currentTarget.position;
        }

        if (!agent.isOnNavMesh)
        {
            TrySnapToNavMesh();
            if (!agent.isOnNavMesh) return;
        }

        agent.nextPosition = transform.position;

        float deltaX = currentTargetPosition.x - transform.position.x;

        bool targetAbove =
            Mathf.Abs(deltaX) < headAlignThreshold &&
            currentTargetPosition.y > transform.position.y + headHeightThreshold;

        bool aiAbove =
            Mathf.Abs(deltaX) < headAlignThreshold &&
            transform.position.y > currentTargetPosition.y + headHeightThreshold;

        bool isStacked = targetAbove || aiAbove;

        if (!returningToPlatform && Time.time >= nextRetreatTime && Time.time >= retreatUntil)
        {
            retreatUntil = Time.time + retreatDuration;

            float dir = Mathf.Sign(transform.position.x - currentTargetPosition.x);
            if (dir == 0f)
                dir = Random.value < 0.5f ? -1f : 1f;

            retreatDir = dir;
            nextRetreatTime = Time.time + Random.Range(retreatIntervalMin, retreatIntervalMax);
        }

        if (isStacked && Time.time >= headEscapeUntil)
        {
            headEscapeUntil = Time.time + headEscapeDuration;

            float dir = Mathf.Sign(transform.position.x - currentTargetPosition.x);
            if (dir == 0f)
                dir = Random.value < 0.5f ? -1f : 1f;

            headEscapeDir = dir;
        }

        bool retreating = Time.time < retreatUntil;
        bool escapingHead = Time.time < headEscapeUntil;

        if (Time.time >= nextRetargetTime)
        {
            Vector3 offsetTarget = currentTargetPosition;

            if (retreating)
            {
                offsetTarget.x += retreatDir * retreatOffset;
            }
            else if (escapingHead)
            {
                offsetTarget.x += headEscapeDir * headPushOffset;
            }
            else if (!returningToPlatform)
            {
                float dir = Mathf.Sign(transform.position.x - currentTargetPosition.x);
                offsetTarget.x += dir * targetSideOffset;
            }

            agent.SetDestination(offsetTarget);
            nextRetargetTime = Time.time + retargetInterval;
        }

        float moveX;

        if (retreating)
        {
            moveX = retreatDir;
        }
        else if (escapingHead)
        {
            moveX = headEscapeDir;
        }
        else if (isStacked)
        {
            float dir = Mathf.Sign(transform.position.x - currentTargetPosition.x);
            if (dir == 0f)
                dir = Random.value < 0.5f ? -1f : 1f;
            moveX = dir;
        }
        else
        {
            moveX = Mathf.Abs(deltaX) > 0.1f ? Mathf.Sign(deltaX) : 0f;
        }

        character.Move(new Vector2(moveX, 0f));

        if (!retreating && !escapingHead && ShouldJumpTowardsTarget(currentTargetPosition, returningToPlatform) && Time.time >= nextJumpTime)
        {
            character.Jump();
            nextJumpTime = Time.time + jumpCooldown;
        }

        if (!retreating && !escapingHead && !returningToPlatform && heartTarget == null && currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange && HasLineOfSight(currentTarget))
            {
                character.Move(Vector2.zero);
                TryAttack(distanceToTarget);
            }
        }
    }

    private void ResolveTarget()
    {
        if (GameManager.Instance == null)
        {
            target = null;
            return;
        }

        Player p1 = GameManager.Instance.GetPlayer1();

        if (p1 == null)
        {
            target = null;
            return;
        }

        if (p1.character != null)
            target = p1.character.transform;
        else
            target = p1.transform;
    }

    private void FindHeart()
    {
        if (Time.time < nextHeartSearchTime) return;

        nextHeartSearchTime = Time.time + heartSearchInterval;
        heartTarget = null;

        HeartPickup heart = FindAnyObjectByType<HeartPickup>();
        if (heart != null)
        {
            heartTarget = heart.transform;
        }
    }

    private void UpdateReturnState()
    {
        if (!hasMainPlatformCenter)
        {
            returningToPlatform = false;
            return;
        }

        bool isOffNavMesh = !NavMesh.SamplePosition(transform.position, out _, 0.5f, NavMesh.AllAreas);

        float verticalDrop = transform.position.y - mainPlatformCenter.y;
        bool isFallingTooLow = verticalDrop < -0.5f;

        if (returningToPlatform)
        {
            float distToCenter = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(mainPlatformCenter.x, mainPlatformCenter.y)
            );

            if (distToCenter <= platformCenterArriveDistance)
                returningToPlatform = false;
        }
        else
        {
            if (isOffNavMesh || isFallingTooLow)
                returningToPlatform = true;
        }
    }

    private bool HasLineOfSight(Transform currentTarget)
    {
        if (currentTarget == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 direction = (currentTarget.position - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, visionDistance))
        {
            return hit.transform == currentTarget || hit.transform.IsChildOf(currentTarget);
        }

        return false;
    }

    private bool ShouldJumpTowardsTarget(Vector3 currentTargetPosition, bool returnMode)
    {
        float verticalDelta = currentTargetPosition.y - transform.position.y;

        if (returnMode)
        {
            float horizontalDelta = Mathf.Abs(currentTargetPosition.x - transform.position.x);
            return horizontalDelta > 0.1f;
        }

        return verticalDelta > jumpHeightThreshold;
    }

    private void TryAttack(float distanceToTarget)
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (Random.value < specialAttackChance || distanceToTarget <= attackRange * 0.75f)
            character.Attack2();
        else
            character.Attack1();
    }

    private void TrySnapToNavMesh()
    {
        if (agent == null) return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void CacheMainPlatformCenter()
    {
        Collider[] colliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude);

        bool found = false;
        float bestArea = -1f;
        Bounds bestBounds = default;

        int mask = groundLayerMask.value;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (((1 << col.gameObject.layer) & mask) == 0)
                continue;

            Bounds bounds = col.bounds;
            float area = bounds.size.x * bounds.size.y;

            if (!found || area > bestArea)
            {
                found = true;
                bestArea = area;
                bestBounds = bounds;
            }
        }

        if (!found)
        {
            hasMainPlatformCenter = false;
            return;
        }

        hasMainPlatformCenter = true;
        mainPlatformCenter = bestBounds.center;

        if (NavMesh.SamplePosition(mainPlatformCenter, out NavMeshHit hit, platformNavMeshSampleRadius, NavMesh.AllAreas))
            mainPlatformNavPoint = hit.position;
        else
            mainPlatformNavPoint = mainPlatformCenter;
    }

    private float HorizontalDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
    }
}