using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private string characterTag = "Character";
    private Character[] characters;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float minFOVPercent = 0.75f;
    [SerializeField] private float maxFOVPercent = 2.0f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float zoomDistanceMin = 3f;
    [SerializeField] private float zoomDistanceMax = 20f;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -10f);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = gameObject.AddComponent<Camera>();

        RefreshCharacters();
    }

    void LateUpdate()
    {
        RefreshCharacters();

        if (characters == null || characters.Length == 0)
            return;

        Vector3 midpoint = GetMidpoint();
        MoveTowardsMidpoint(midpoint);
        AdjustZoom();
    }

    private void RefreshCharacters()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(characterTag);
        characters = new Character[objs.Length];
        for (int i = 0; i < objs.Length; i++)
            characters[i] = objs[i].GetComponent<Character>();
    }

    private Vector3 GetMidpoint()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Character c in characters)
        {
            if (c != null)
            {
                sum += c.transform.position;
                count++;
            }
        }

        return count > 0 ? sum / count : transform.position;
    }

    private float GetMaxDistance()
    {
        float maxDist = 0f;

        for (int i = 0; i < characters.Length; i++)
        {
            for (int j = i + 1; j < characters.Length; j++)
            {
                if (characters[i] != null && characters[j] != null)
                {
                    float d = Vector3.Distance(
                        characters[i].transform.position,
                        characters[j].transform.position
                    );
                    if (d > maxDist) maxDist = d;
                }
            }
        }

        return maxDist;
    }

    private void MoveTowardsMidpoint(Vector3 midpoint)
    {
        Vector3 targetPos = midpoint + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    private void AdjustZoom()
    {
        float distance = GetMaxDistance();

        float t = Mathf.InverseLerp(zoomDistanceMin, zoomDistanceMax, distance);

        float targetFOV = Mathf.Lerp(
            baseFOV * minFOVPercent,
            baseFOV * maxFOVPercent,
            t
        );

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }
}