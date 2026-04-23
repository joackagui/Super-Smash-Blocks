using UnityEngine;
using UnityEngine.InputSystem;

public class Character
{
    public int damageReceived;
    public int damageDealt;
    public int lives;
    private bool isMoving = false;

    private bool isJumping;
    private bool isAttacking;

    public virtual void OnMovement(InputValue value)
    {
        // Handle movement logic here
    }
    void Start()
    {
        //new GameObject("Hitbox").AddComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
