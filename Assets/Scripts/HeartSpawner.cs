using UnityEngine;

public class HeartSpawner : MonoBehaviour
{
    [SerializeField] private HeartPickup heartPrefab;
    [SerializeField] private Transform spawnPoint;

    private bool hasSpawned;

    private void Update()
    {
        Debug.Log("HeartSpawner corriendo");

        if (hasSpawned)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.Log("GameManager NULL");
            return;
        }

        Player player1 = GameManager.Instance.GetPlayer1();
        Player player2 = GameManager.Instance.GetPlayer2();

        Debug.Log("P1: " + player1 + " | P2: " + player2);

        if (player1 == null || player2 == null)
        {
            Debug.Log("ALGUN PLAYER ES NULL");
            return;
        }

        Debug.Log("Vidas -> P1: " + player1.lives + " | P2: " + player2.lives);

        if (player1.lives <= 2 && player2.lives <= 2)
        {
            Debug.Log("CONDICION CUMPLIDA → SPAWN");
            SpawnHeart();
        }
    }

    private void SpawnHeart()
    {
        if (heartPrefab == null)
        {
            Debug.LogError("NO HAY HEART PREFAB ASIGNADO");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;

        Debug.Log("Spawneando corazón en: " + position);

        Instantiate(heartPrefab, position, Quaternion.identity);
        hasSpawned = true;
    }
}