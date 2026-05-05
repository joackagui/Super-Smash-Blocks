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
    [SerializeField] private float rotationSpeed = 90f;

    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    [Header("Behavior")]
    [SerializeField] private float panicDistance = 3f;
    [SerializeField] private float panicSpeedMultiplier = 2f;

    [Header("Visual")]
    [SerializeField] private float hoverAmplitude = 0.25f;
    [SerializeField] private float hoverFrequency = 2f;
    private float baseY;

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
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
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

        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        if (distanceToPlayer < panicDistance)
        {
            agent.speed = 3f * panicSpeedMultiplier;
        }
        else
        {
            agent.speed = 3f;
        }

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathRate;

            Vector3 awayDirection = GetPlanarDirectionAwayFrom(playerPos);
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            randomOffset.z = 0f;
            awayDirection = (awayDirection + randomOffset).normalized;
            Vector3 strafe = Vector3.Cross(awayDirection, Vector3.up) * Random.Range(-2f, 2f);
            Vector3 targetPos = agent.nextPosition + awayDirection * fleeDistance + strafe;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        TryPickup(currentTarget, playerPos);
        HandleHover();
    }

    private void HandleHover()
    {
        float hover = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        agent.baseOffset = hoverHeight + hover;
    }

    private void TryPickup(Player player, Vector3 playerPos)
    {
        if (player == null) return;

        float distance = GetPlanarDistance(transform.position, playerPos);

        float planarDistance = GetPlanarDistance(transform.position, playerPos);
        float verticalDistance = Mathf.Abs(transform.position.y - playerPos.y);

        if (planarDistance < pickupRadius && verticalDistance < 2f)
        {
            if (player.lives < 3)
            {
                player.HealLife(healAmount);
            }

            collected = true;
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
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
        awayDirection.z = 0f;

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
        a.z = 0f;
        b.z = 0f;
        return Vector3.Distance(a, b);
    }
}
