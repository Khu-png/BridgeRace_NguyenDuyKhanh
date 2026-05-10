using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public const string IdleTriggerName = "Idle";
    public const string RunTriggerName = "Run";
    public const string FallTriggerName = "Fall";
    public const string DanceTriggerName = "Dance";

    [SerializeField] private Animator animator;

    private string currentAnimationName;

    public bool IsInTransition => animator.IsInTransition(0);

    public bool IsState(string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public void SetTimeScaleMode(AnimatorUpdateMode updateMode)
    {
        animator.updateMode = updateMode;
    }

    public void SetRootMotion(bool useRootMotion)
    {
        animator.applyRootMotion = useRootMotion;
    }

    public void SetMoving(bool isMoving)
    {
        ChangeAnimation(isMoving ? RunTriggerName : IdleTriggerName);
    }

    public void ChangeAnimation(string animationName)
    {
        if (string.IsNullOrEmpty(animationName) || currentAnimationName == animationName)
        {
            return;
        }

        if (!string.IsNullOrEmpty(currentAnimationName))
        {
            animator.ResetTrigger(currentAnimationName);
        }

        currentAnimationName = animationName;
        animator.SetTrigger(currentAnimationName);
    }
}
