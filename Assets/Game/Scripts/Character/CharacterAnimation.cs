using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private const string IdleTriggerName = "Idle";
    private const string RunTriggerName = "Run";
    private const string FallTriggerName = "Fall";
    private const string DanceTriggerName = "Dance";

    [SerializeField] private Animator animator;

    public AnimatorStateInfo CurrentState => animator.GetCurrentAnimatorStateInfo(0);
    public bool IsInTransition => animator.IsInTransition(0);

    public void SetTimeScaleMode(AnimatorUpdateMode updateMode)
    {
        animator.updateMode = updateMode;
    }

    public void SetRootMotion(bool useRootMotion)
    {
        animator.applyRootMotion = useRootMotion;
    }

    public void PlayFall()
    {
        animator.ResetTrigger(IdleTriggerName);
        animator.ResetTrigger(RunTriggerName);
        animator.SetTrigger(FallTriggerName);
    }

    public void PlayDance()
    {
        animator.ResetTrigger(IdleTriggerName);
        animator.ResetTrigger(RunTriggerName);
        animator.ResetTrigger(FallTriggerName);
        animator.SetTrigger(DanceTriggerName);
    }

    public void SetMoving(bool isMoving)
    {
        animator.SetTrigger(isMoving ? RunTriggerName : IdleTriggerName);
    }
}
