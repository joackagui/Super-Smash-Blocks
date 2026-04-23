using UnityEngine;

public class Batman : Character
{
    [Header("Batman Animation Overrides")]
    [SerializeField] private Animator batmanAnimator;
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string action1Trigger = "Action1";
    [SerializeField] private string action2Trigger = "Action2";

    protected override void Awake()
    {
        base.Awake();

        if (batmanAnimator == null)
        {
            batmanAnimator = GetComponentInChildren<Animator>();
        }
    }

    public override void OnMove(float horizontalSpeed)
    {
        if (batmanAnimator == null || string.IsNullOrEmpty(moveSpeedParameter))
        {
            base.OnMove(horizontalSpeed);
            return;
        }

        batmanAnimator.SetFloat(moveSpeedParameter, Mathf.Abs(horizontalSpeed));
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
        if (batmanAnimator == null || string.IsNullOrEmpty(triggerName))
        {
            fallback?.Invoke();
            return;
        }

        batmanAnimator.SetTrigger(triggerName);
    }
}
