using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public enum HitboxType { Hand, Foot }

    [SerializeField] private HitboxType hitboxType = HitboxType.Hand;

    private const float HAND_DAMAGE = 7f;
    private const float FOOT_DAMAGE = 10f;
    private const float KNOCKBACK_BASE = 8f;
    private const float KNOCKBACK_DAMAGE_MULTIPLIER = 0.15f;

    private Character ownerCharacter;
    private bool isActive = false;

    public void SetOwner(Character owner)
    {
        ownerCharacter = owner;

        string n = gameObject.name.ToLower();
        if (n.Contains("hand"))
            hitboxType = HitboxType.Hand;
        else if (n.Contains("foot"))   // foot en lugar de leg
            hitboxType = HitboxType.Foot;
    }

    public void Activate()   { isActive = true; }
    public void Deactivate() { isActive = false; }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (ownerCharacter == null) return;
        if (!other.CompareTag("Character")) return;

        // Busca el Character en el objeto o en su padre
        Character target = other.GetComponent<Character>();
        if (target == null)
            target = other.GetComponentInParent<Character>();

        if (target == null) return;
        if (target == ownerCharacter) return;

        float damage = hitboxType == HitboxType.Hand ? HAND_DAMAGE : FOOT_DAMAGE;
        target.TakeDamage(damage, ownerCharacter.transform.position);
    }
}