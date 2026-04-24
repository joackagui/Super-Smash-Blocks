using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int totalDamageReceived;
    public int totalDamageDealt;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "New action map";

    private InputAction moveAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction jumpAction;
    private InputAction attack1Action;
    private InputAction attack2Action;

    public Character character;

    public void SetCharacter(Character character)
    {
        this.character = character;
    }

    void Awake()
    {
        ConfigureActions();
    }

    void OnEnable()
    {
        EnableActions(true);
    }

    void OnDisable()
    {
        EnableActions(false);
    }

    void Update()
    {
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
    }

    void Start()
    {
    }

    private void ConfigureActions()
    {
        if (inputActions == null)
        {
            Debug.LogWarning($"{name}: No InputActionAsset assigned.");
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
            Debug.LogWarning($"{name}: No action map found in InputActionAsset.");
            return;
        }

        moveAction = map.FindAction("Move", false) ?? map.FindAction("Movement", false);
        leftAction = map.FindAction("Left", false);
        rightAction = map.FindAction("Right", false);
        jumpAction = map.FindAction("Jump", false) ?? map.FindAction("Up", false);
        attack1Action = map.FindAction("Action1", false);
        attack2Action = map.FindAction("Action2", false);

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
}
