using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HeartPickup : MonoBehaviour
{
    [SerializeField] private float fleeDistance = 6f;
    [SerializeField] private float repathRate = 0.2f;
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private int healAmount = 1;

    private NavMeshAgent agent;
    private float nextRepathTime;
    private bool collected = false;

    private Player currentTarget;
    private float targetLockTime = 1.5f;
    private float nextTargetSwitch;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 3f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
    }

    private void Update()
    {
        if (collected) return;

        if (Time.time > nextTargetSwitch || currentTarget == null)
        {
            currentTarget = GetNearestPlayer();
            nextTargetSwitch = Time.time + targetLockTime;
            Debug.Log("Nuevo target: " + (currentTarget != null ? currentTarget.name : "null"));
        }

        if (currentTarget == null) return;

        Vector3 playerPos = currentTarget.character != null 
            ? currentTarget.character.transform.position 
            : currentTarget.transform.position;

        float distance = Vector3.Distance(transform.position, playerPos);
        Debug.Log("Distancia: " + distance);

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathRate;

            Vector3 awayDirection = (transform.position - playerPos).normalized;
            Vector3 targetPos = transform.position + awayDirection * fleeDistance;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
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
            Debug.Log("RECOGIDO por: " + player.name);

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

        float d1 = Vector3.Distance(transform.position, p1Pos);
        float d2 = Vector3.Distance(transform.position, p2Pos);

        return d1 <= d2 ? p1 : p2;
    }
}