using UnityEngine;

public class Batman : Character
{
    [SerializeField] private GameObject batarangPrefab;
    [SerializeField] private float batarangDamage = 9f;
    [SerializeField] private float batarangImpulse = 1000f;
    [SerializeField] private float batarangLaunchAngleDegrees = 45f;
    [SerializeField] private Vector3 batarangScale = new Vector3(10f, 10f, 10f);

    protected override void LongDistanceAttack()
    {
        if (batarangPrefab == null) return;

        Vector3 horizontalForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (horizontalForward.sqrMagnitude <= 0f)
        {
            horizontalForward = transform.forward.normalized;
        }

        Vector3 spawnPos = transform.position + horizontalForward * 0.5f;
        GameObject go = Instantiate(batarangPrefab, spawnPos, Quaternion.LookRotation(horizontalForward, Vector3.up));
        go.transform.localScale = batarangScale;

        float launchAngleRadians = batarangLaunchAngleDegrees * Mathf.Deg2Rad;
        Vector3 launchDirection = (horizontalForward * Mathf.Cos(launchAngleRadians) + Vector3.up * Mathf.Sin(launchAngleRadians)).normalized;

        Batarang b = go.GetComponent<Batarang>();
        if (b != null)
        {
            b.Initialize(this, launchDirection, batarangDamage, batarangImpulse);
            b.StartMoving();
        }
    }
}
