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

    [SerializeField] private Vector2 xLimits = new Vector2(-10f, 10f);
    [SerializeField] private Vector2 yLimits = new Vector2(1f, 10f);
    [SerializeField] private float fixedZ = -10f;

    private Camera cam;

    private float refreshTimer = 0f;
    [SerializeField] private float refreshRate = 1f;

    private bool isIntroActive;

    void Start()
    {
        EnsureCamera();

        RefreshCharacters();
    }

    void LateUpdate()
    {
        EnsureCamera();

        if (isIntroActive)
        {
            return;
        }

        // Refresh characters only every X seconds
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshRate)
        {
            RefreshCharacters();
            refreshTimer = 0f;
        }

        if (characters == null || characters.Length == 0)
            return;

        Vector3 midpoint = GetMidpoint();

        MoveTowardsMidpoint(midpoint);
        AdjustZoom();
        transform.rotation = Quaternion.identity;
    }

    public void SetIntroActive(bool active)
    {
        isIntroActive = active;
    }

    public void SetIntroPose(Vector3 position, Quaternion rotation)
    {
        EnsureCamera();
        transform.position = position;
        transform.rotation = rotation;
        velocity = Vector3.zero;
    }

    public void RefreshCharacters()
    {
        RefreshCharactersInternal();
    }

    private void EnsureCamera()
    {
        if (cam != null)
        {
            return;
        }

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
    }

    private void RefreshCharactersInternal()
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

    private Vector3 velocity;

    private void MoveTowardsMidpoint(Vector3 midpoint)
    {
        Vector3 targetPos = midpoint + offset;

        // Clamp position so camera doesn't go out of bounds
        targetPos.x = Mathf.Clamp(targetPos.x, xLimits.x, xLimits.y);
        targetPos.y = Mathf.Clamp(targetPos.y, yLimits.x, yLimits.y);
        targetPos.z = fixedZ; // lock Z so it never shifts forward/back

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            1f / followSpeed
        );
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
        targetFOV = Mathf.Clamp(targetFOV, 50f, 80f);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }
}