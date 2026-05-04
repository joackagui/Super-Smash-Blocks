using UnityEngine;

public class HeartSpawner : MonoBehaviour
{
    [SerializeField] private HeartPickup heartPrefab;
    [SerializeField] private Transform spawnPoint;

    private bool hasSpawned;

    private void Update()
    {

        if (hasSpawned)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        Player player1 = GameManager.Instance.GetPlayer1();
        Player player2 = GameManager.Instance.GetPlayer2();


        if (player1 == null || player2 == null)
        {
            return;
        }

        if (player1.lives <= 2 && player2.lives <= 2)
        {
            SpawnHeart();
        }
    }

    private void SpawnHeart()
    {
        if (heartPrefab == null)
        {
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;

        Instantiate(heartPrefab, position, Quaternion.identity);
        hasSpawned = true;
    }
}