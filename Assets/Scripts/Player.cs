using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

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
    [Header("Spawn Points")]
    [SerializeField] private GameObject initialSpawnPoint;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private TextMeshProUGUI damageText;

    private InputActionMap actionMap;
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
        return ResolveSpawnPoint(false);
    }

    public GameObject GetInitialSpawnPoint()
    {
        return ResolveSpawnPoint(true);
    }

    public void SetSpawnPoint(GameObject newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void SetInitialSpawnPoint(GameObject newInitialSpawnPoint)
    {
        initialSpawnPoint = newInitialSpawnPoint;
    }

    public void SetCharacter(Character character)
    {
        SetCharacter(character, false);
    }

    public void SetCharacter(Character character, bool useInitialSpawnPoint)
    {
        this.character = character;

        if (this.character == null)
        {
            return;
        }

        this.character.SetOwner(this);
        SpawnCharacter(this.character, useInitialSpawnPoint);
    }

    public void SetKeybinds(bool enabled)
    {
        EnableActions(enabled);
    }

    public void SpawnCharacter(Character newCharacter)
    {
        SpawnCharacter(newCharacter, false);
    }

    public void SpawnCharacter(Character newCharacter, bool useInitialSpawnPoint)
    {
        if (newCharacter == null)
        {
            return;
        }

        character = newCharacter;
        GameObject resolvedSpawnPoint = ResolveSpawnPoint(useInitialSpawnPoint);
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        if (resolvedSpawnPoint != null)
        {
            spawnPosition = resolvedSpawnPoint.transform.position;

            if (!useInitialSpawnPoint)
            {
                spawnPosition += Vector3.up * GetSpawnHeightOffset(character);
            }

            spawnRotation = resolvedSpawnPoint.transform.rotation;

            if (playerSlot == PlayerSlot.Player2)
            {
                spawnRotation *= Quaternion.Euler(0f, 180f, 0f);
            }
        }

        Rigidbody characterBody = character.GetComponent<Rigidbody>();
        if (characterBody != null)
        {
            characterBody.linearVelocity = Vector3.zero;
            characterBody.angularVelocity = Vector3.zero;
            characterBody.position = spawnPosition;
            characterBody.rotation = spawnRotation;
        }
        else
        {
            character.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        }

        character.SetInitialFacing(playerSlot == PlayerSlot.Player1 ? 1 : -1);

        RefreshDamageText();
    }

    private GameObject ResolveSpawnPoint(bool useInitialSpawnPoint)
    {
        if (useInitialSpawnPoint && initialSpawnPoint != null)
        {
            return initialSpawnPoint;
        }

        if (spawnPoint != null)
        {
            return spawnPoint;
        }

        string preferredName = useInitialSpawnPoint
            ? (playerSlot == PlayerSlot.Player1 ? "Player1InitialSpawnPoint" : "Player2InitialSpawnPoint")
            : (playerSlot == PlayerSlot.Player1 ? "Player1SpawnPoint" : "Player2SpawnPoint");

        GameObject foundSpawnPoint = GameObject.Find(preferredName);
        if (foundSpawnPoint != null)
        {
            if (useInitialSpawnPoint)
            {
                initialSpawnPoint = foundSpawnPoint;
                return initialSpawnPoint;
            }

            spawnPoint = foundSpawnPoint;
            return spawnPoint;
        }

        if (useInitialSpawnPoint)
        {
            return spawnPoint;
        }

        return null;
    }

    private float GetSpawnHeightOffset(Character targetCharacter)
    {
        if (targetCharacter == null)
        {
            return 0f;
        }

        Collider characterCollider = targetCharacter.GetComponent<Collider>();
        if (characterCollider == null)
        {
            characterCollider = targetCharacter.GetComponentInChildren<Collider>();
        }

        if (characterCollider == null)
        {
            return 1f;
        }

        return characterCollider.bounds.extents.y + 0.1f;
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

        ApplyDeviceRouting();
        InputSystem.onDeviceChange += OnDeviceChange;
        EnableActions(true);
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterPlayer(this);
        }

        EnableActions(false);
    }

    private void Update()
    {
        if (PauseMenuManager.IsPaused)
        {
            return;
        }

        RefreshDamageText();

        if (character == null)
        {
            return;
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.IsSinglePlayer &&
            playerSlot == PlayerSlot.Player2)
        {
            return;
        }

        Vector2 move = ReadMovement();
        character.Move(move);

        if (!character.CanAcceptActions)
        {
            return;
        }

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

        actionMap = map;
        moveAction = map.FindAction("Move", false) ?? map.FindAction("Movement", false);
        leftAction = map.FindAction("Left", false);
        rightAction = map.FindAction("Right", false);
        jumpAction = map.FindAction("Jump", false) ?? map.FindAction("Up", false);
        attack1Action = map.FindAction("Action1", false);
        attack2Action = map.FindAction("Action2", false);
        dodgeAction = map.FindAction("Dodge", false);

        map.Enable();
    }

    private void ApplyDeviceRouting()
    {
        PlayerInputDeviceRouter.AssignDevices(actionMap, playerSlot);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad && device is not Keyboard && device is not Mouse)
        {
            return;
        }

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Reconnected:
            case InputDeviceChange.Enabled:
            case InputDeviceChange.Disabled:
            case InputDeviceChange.ConfigurationChanged:
                ApplyDeviceRouting();
                break;
        }
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

    public void SetDamageTextVisible(bool visible)
    {
        if (damageText == null)
        {
            return;
        }

        damageText.gameObject.SetActive(visible);
    }
}

public static class PlayerInputDeviceRouter
{
    public static void AssignDevices(InputActionMap actionMap, Player.PlayerSlot playerSlot, bool includeKeyboardAndMouseForPlayerOne = true)
    {
        if (actionMap == null)
        {
            return;
        }

        actionMap.devices = GetDevicesForPlayer(playerSlot, includeKeyboardAndMouseForPlayerOne);
    }

    public static void AssignDevices(InputAction action, Player.PlayerSlot playerSlot, bool includeKeyboardAndMouseForPlayerOne = true)
    {
        if (action == null)
        {
            return;
        }

        AssignDevices(action.actionMap, playerSlot, includeKeyboardAndMouseForPlayerOne);
    }

    public static void AssignDevices(InputActionReference actionReference, Player.PlayerSlot playerSlot, bool includeKeyboardAndMouseForPlayerOne = true)
    {
        if (actionReference == null)
        {
            return;
        }

        AssignDevices(actionReference.action, playerSlot, includeKeyboardAndMouseForPlayerOne);
    }

    public static InputDevice[] GetDevicesForPlayer(Player.PlayerSlot playerSlot, bool includeKeyboardAndMouseForPlayerOne = true)
    {
        List<InputDevice> devices = new List<InputDevice>();
        int gamepadCount = Gamepad.all.Count;

        if (gamepadCount >= 2)
        {
            int gamepadIndex = playerSlot == Player.PlayerSlot.Player1 ? 0 : 1;
            Gamepad assignedGamepad = Gamepad.all[gamepadIndex];
            if (assignedGamepad != null)
            {
                devices.Add(assignedGamepad);
            }
        }
        else if (gamepadCount == 1)
        {
            if (playerSlot == Player.PlayerSlot.Player1)
            {
                devices.Add(Gamepad.all[0]);
                
                if (includeKeyboardAndMouseForPlayerOne)
                {
                    if (Keyboard.current != null)
                    {
                        devices.Add(Keyboard.current);
                    }

                    if (Mouse.current != null)
                    {
                        devices.Add(Mouse.current);
                    }
                }
            }
            else
            {
                if (Keyboard.current != null)
                {
                    devices.Add(Keyboard.current);
                }

                if (Mouse.current != null)
                {
                    devices.Add(Mouse.current);
                }
            }
        }
        else
        {
            if (Keyboard.current != null)
            {
                devices.Add(Keyboard.current);
            }

            if (Mouse.current != null)
            {
                devices.Add(Mouse.current);
            }
        }

        return devices.ToArray();
    }
}
