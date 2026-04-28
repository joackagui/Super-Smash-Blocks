using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private Collider platformCollider;

    void Awake()
    {
        platformCollider = GetComponent<Collider>();
    }

    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Character")) return;

        Collider playerCollider = collision.collider;

        float playerBottom = playerCollider.bounds.min.y;
        float platformTop = platformCollider.bounds.max.y;

        if (playerBottom < platformTop)
        {
            Physics.IgnoreCollision(playerCollider, platformCollider, true);
        }
        else
        {
            Physics.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Character")) return;

        Physics.IgnoreCollision(collision.collider, platformCollider, false);
    }
}