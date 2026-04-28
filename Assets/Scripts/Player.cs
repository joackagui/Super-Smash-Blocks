using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    public enum PlayerSlot
    {
        Player1,
        Player2
    }

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "New action map";
    [SerializeField] private PlayerSlot playerSlot;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private TextMeshProUGUI damageText;

    private InputAction moveAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction jumpAction;
    private InputAction attack1Action;
    private InputAction attack2Action;
    private InputAction dodgeAction;

    public Character character;
    public int lives = 3;

    public PlayerSlot Slot => playerSlot;

    private UIManager uiManager;

    public GameObject GetSpawnPoint()
    {
        return ResolveSpawnPoint();
    }

    public void SetSpawnPoint(GameObject newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void SetCharacter(Character character)
    {
        this.character = character;

        if (this.character == null)
        {
            return;
        }

        this.character.SetOwner(this);
        SpawnCharacter(this.character);
    }

    public void SpawnCharacter(Character newCharacter)
    {
        if (newCharacter == null)
        {
            return;
        }

        character = newCharacter;
        GameObject resolvedSpawnPoint = ResolveSpawnPoint();

        if (resolvedSpawnPoint != null)
        {
            Quaternion spawnRotation = resolvedSpawnPoint.transform.rotation;

            if (playerSlot == PlayerSlot.Player2)
            {
                spawnRotation *= Quaternion.Euler(0f, 180f, 0f);
            }

            character.transform.SetPositionAndRotation(resolvedSpawnPoint.transform.position, spawnRotation);
        }
        else
        {
            character.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        character.SetInitialFacing(playerSlot == PlayerSlot.Player1 ? 1 : -1);

        Rigidbody characterBody = character.GetComponent<Rigidbody>();
        if (characterBody != null)
        {
            characterBody.linearVelocity = Vector3.zero;
            characterBody.angularVelocity = Vector3.zero;
        }

        RefreshDamageText();
    }

    private GameObject ResolveSpawnPoint()
    {
        if (spawnPoint != null)
        {
            return spawnPoint;
        }

        string preferredName = playerSlot == PlayerSlot.Player1
            ? "Player1SpawnPoint"
            : "Player2SpawnPoint";

        GameObject foundSpawnPoint = GameObject.Find(preferredName);
        if (foundSpawnPoint != null)
        {
            spawnPoint = foundSpawnPoint;
            return spawnPoint;
        }

        return null;
    }

    public void HandleCharacterDeath(System.Action onRespawnReady = null)
    {
        lives--;

        if (uiManager != null)
        {
            uiManager.SetHearts(playerSlot, lives);
        }

        if (lives <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Victory(this);

            return;
        }

        onRespawnReady?.Invoke();
    }

    public void HealLife(int amount = 1)
    {
        lives = Mathf.Min(3, lives + amount);

        if (uiManager != null)
        {
            uiManager.SetHearts(playerSlot, lives);
        }
    }

    private void Awake()
    {
        ConfigureActions();
    }

    private void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.SetHearts(playerSlot, lives);
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }

        EnableActions(true);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterPlayer(this);
        }

        EnableActions(false);
    }

    private void Update()
    {
        RefreshDamageText();

        if (character == null)
        {
            return;
        }

        Vector2 move = ReadMovement();
        character.Move(move);

        if (jumpAction != null && jumpAction.triggered)
        {
            character.Jump();
        }

        if (attack1Action != null && attack1Action.triggered)
        {
            character.Attack1();
        }

        if (attack2Action != null && attack2Action.triggered)
        {
            character.Attack2();
        }

        if (dodgeAction != null && dodgeAction.triggered)
        {
            character.Dodge();
        }
    }

    private void ConfigureActions()
    {
        if (inputActions == null)
        {
            return;
        }

        InputActionMap map = !string.IsNullOrWhiteSpace(actionMapName)
            ? inputActions.FindActionMap(actionMapName, false)
            : null;

        if (map == null && inputActions.actionMaps.Count > 0)
        {
            map = inputActions.actionMaps[0];
        }

        if (map == null)
        {
            return;
        }

        moveAction = map.FindAction("Move", false) ?? map.FindAction("Movement", false);
        leftAction = map.FindAction("Left", false);
        rightAction = map.FindAction("Right", false);
        jumpAction = map.FindAction("Jump", false) ?? map.FindAction("Up", false);
        attack1Action = map.FindAction("Action1", false);
        attack2Action = map.FindAction("Action2", false);
        dodgeAction = map.FindAction("Dodge", false);

        map.Enable();
    }

    private void EnableActions(bool enable)
    {
        SetActionState(moveAction, enable);
        SetActionState(leftAction, enable);
        SetActionState(rightAction, enable);
        SetActionState(jumpAction, enable);
        SetActionState(attack1Action, enable);
        SetActionState(attack2Action, enable);
        SetActionState(dodgeAction, enable);
    }

    private static void SetActionState(InputAction action, bool enable)
    {
        if (action == null)
        {
            return;
        }

        if (enable)
        {
            action.Enable();
        }
        else
        {
            action.Disable();
        }
    }

    private Vector2 ReadMovement()
    {
        float axisX = 0f;

        if (leftAction != null || rightAction != null)
        {
            float left = leftAction != null && leftAction.IsPressed() ? 1f : 0f;
            float right = rightAction != null && rightAction.IsPressed() ? 1f : 0f;
            axisX = right - left;
        }
        else if (moveAction != null)
        {
            axisX = moveAction.ReadValue<float>();
            if (Mathf.Approximately(axisX, 0f))
            {
                Vector2 vectorMove = moveAction.ReadValue<Vector2>();
                axisX = vectorMove.x;
            }
        }

        return new Vector2(Mathf.Clamp(axisX, -1f, 1f), 0f);
    }

    private void RefreshDamageText()
    {
        if (damageText == null)
        {
            return;
        }

        if (character == null)
        {
            damageText.text = "0%";
            return;
        }

        damageText.text = character.GetDamageReceived().ToString("0") + "%";
    }
}