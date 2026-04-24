using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int totalDamageReceived;
    public int totalDamageDealt;

    private PlayerInput playerInput;
    private InputAction moveAction;
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
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attack1Action = playerInput.actions["Action1"];
        attack2Action = playerInput.actions["Action2"];
    }

    void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        character.Move(move);
        if (jumpAction.triggered)
        {
            character.Jump();
        }
    }

    void Start()
    {
        
    }
}
