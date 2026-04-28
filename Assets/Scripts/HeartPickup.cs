using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HeartPickup : MonoBehaviour
{
    [SerializeField] private float fleeDistance = 6f;
    [SerializeField] private float repathRate = 0.2f;
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float minimumHoverHeight = 1f;
    [SerializeField] private float navMeshSampleRadius = 12f;

    private NavMeshAgent agent;
    private float nextRepathTime;
    private bool collected = false;

    private Player currentTarget;
    private float targetLockTime = 1.5f;
    private float nextTargetSwitch;
    private float hoverHeight;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 3f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        SnapToNavMeshAtSpawnHeight();
    }

    private void Update()
    {
        if (collected || !agent.isOnNavMesh) return;

        if (Time.time > nextTargetSwitch || currentTarget == null)
        {
            currentTarget = GetNearestPlayer();
            nextTargetSwitch = Time.time + targetLockTime;
        }

        if (currentTarget == null) return;

        Vector3 playerPos = currentTarget.character != null 
            ? currentTarget.character.transform.position 
            : currentTarget.transform.position;

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathRate;

            Vector3 awayDirection = GetPlanarDirectionAwayFrom(playerPos);
            Vector3 targetPos = agent.nextPosition + awayDirection * fleeDistance;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        TryPickup(currentTarget, playerPos);
    }

    private void TryPickup(Player player, Vector3 playerPos)
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, playerPos);

        if (distance < pickupRadius)
        {
            if (player.lives < 3)
            {
                player.HealLife(healAmount);
            }

            collected = true;
            Destroy(gameObject);
        }
    }

    private Player GetNearestPlayer()
    {
        if (GameManager.Instance == null) return null;

        Player p1 = GameManager.Instance.GetPlayer1();
        Player p2 = GameManager.Instance.GetPlayer2();

        if (p1 == null) return p2;
        if (p2 == null) return p1;

        Vector3 p1Pos = p1.character != null ? p1.character.transform.position : p1.transform.position;
        Vector3 p2Pos = p2.character != null ? p2.character.transform.position : p2.transform.position;

        float d1 = GetPlanarDistance(transform.position, p1Pos);
        float d2 = GetPlanarDistance(transform.position, p2Pos);

        return d1 <= d2 ? p1 : p2;
    }

    private void SnapToNavMeshAtSpawnHeight()
    {
        Vector3 spawnPosition = transform.position;

        if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            hoverHeight = Mathf.Max(minimumHoverHeight, agent.baseOffset);
            agent.baseOffset = hoverHeight;
            return;
        }

        hoverHeight = Mathf.Max(minimumHoverHeight, spawnPosition.y - hit.position.y);
        agent.baseOffset = hoverHeight;
        agent.Warp(hit.position);
    }

    private Vector3 GetPlanarDirectionAwayFrom(Vector3 targetPosition)
    {
        Vector3 awayDirection = transform.position - targetPosition;
        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.0001f)
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0f;
            return randomDirection.sqrMagnitude < 0.0001f ? Vector3.right : randomDirection.normalized;
        }

        return awayDirection.normalized;
    }

    private float GetPlanarDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
