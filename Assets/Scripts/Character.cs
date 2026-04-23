using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string action1Trigger = "Action1";
    [SerializeField] private string action2Trigger = "Action2";

    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public virtual void OnMove(float horizontalSpeed)
    {
        if (animator == null || string.IsNullOrEmpty(moveSpeedParameter))
        {
            return;
        }

        animator.SetFloat(moveSpeedParameter, Mathf.Abs(horizontalSpeed));
    }

    public virtual void OnJump()
    {
        Trigger(jumpTrigger);
    }

    public virtual void OnAction1()
    {
        Trigger(action1Trigger);
    }

    public virtual void OnAction2()
    {
        Trigger(action2Trigger);
    }

    protected void Trigger(string triggerName)
    {
        if (animator == null || string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        animator.SetTrigger(triggerName);
    }
}
