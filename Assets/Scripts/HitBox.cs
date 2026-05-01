using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    public enum HitboxType { Hand, Foot }

    [SerializeField] private HitboxType hitboxType = HitboxType.Hand;

    private const float HAND_DAMAGE = 7f;
    private const float FOOT_DAMAGE = 10f;

    private Character ownerCharacter;
    private Collider hitboxCollider;
    private bool isActive = false;

    private HashSet<Character> hitTargets = new HashSet<Character>();

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
    }

    public void SetOwner(Character owner)
    {
        ownerCharacter = owner;

        string n = gameObject.name.ToLower();
        if (n.Contains("hand"))
            hitboxType = HitboxType.Hand;
        else if (n.Contains("foot"))
            hitboxType = HitboxType.Foot;
    }

    public void Activate()
    {
        isActive = true;
        hitTargets.Clear();
        DetectOverlappingTargets();
    }

    public void Deactivate()
    {
        isActive = false;
        hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!isActive) return;
        if (ownerCharacter == null) return;

        Character target = other.GetComponentInParent<Character>();

        if (target == null) return;
        if (target == ownerCharacter) return;

        if (hitTargets.Contains(target)) return;

        hitTargets.Add(target);

        float damage = hitboxType == HitboxType.Hand ? HAND_DAMAGE : FOOT_DAMAGE;
        target.TakeDamage(damage, ownerCharacter.transform.position);
    }

    private void DetectOverlappingTargets()
    {
        if (hitboxCollider == null) return;

        Bounds bounds = hitboxCollider.bounds;
        Collider[] overlaps = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            transform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider overlap in overlaps)
        {
            if (overlap == hitboxCollider) continue;
            TryHit(overlap);
        }
    }
}
