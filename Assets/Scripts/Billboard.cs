using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = cam.transform.position - transform.position;
        dir.y = 0f;

        transform.rotation = Quaternion.LookRotation(dir);
    }
}