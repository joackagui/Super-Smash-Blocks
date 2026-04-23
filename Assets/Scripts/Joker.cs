using UnityEngine;

public class Joker : Character
{
    [Header("Joker Animation Overrides")]
    [SerializeField] private Animator jokerAnimator;
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string action1Trigger = "Action1";
    [SerializeField] private string action2Trigger = "Action2";

    protected override void Awake()
    {
        base.Awake();

        if (jokerAnimator == null)
        {
            jokerAnimator = GetComponentInChildren<Animator>();
        }
    }

    public override void OnMove(float horizontalSpeed)
    {
        if (jokerAnimator == null || string.IsNullOrEmpty(moveSpeedParameter))
        {
            base.OnMove(horizontalSpeed);
            return;
        }

        jokerAnimator.SetFloat(moveSpeedParameter, Mathf.Abs(horizontalSpeed));
    }

    public override void OnJump()
    {
        TriggerOverride(jumpTrigger, base.OnJump);
    }

    public override void OnAction1()
    {
        TriggerOverride(action1Trigger, base.OnAction1);
    }

    public override void OnAction2()
    {
        TriggerOverride(action2Trigger, base.OnAction2);
    }

    private void TriggerOverride(string triggerName, System.Action fallback)
    {
        if (jokerAnimator == null || string.IsNullOrEmpty(triggerName))
        {
            fallback?.Invoke();
            return;
        }

        jokerAnimator.SetTrigger(triggerName);
    }
}
