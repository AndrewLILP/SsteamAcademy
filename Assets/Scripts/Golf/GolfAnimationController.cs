using UnityEngine;

public class GolfAnimationController : MonoBehaviour
{
    private Animator animator;
    private IGolfShotType currentShotType;
    private GolfManager golfManager;

    // Animation parameter IDs
    private int animIDGolfDrive;
    private int animIDGolfChip;
    private int animIDGolfPutt;
    private int animIDGolfIdle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        golfManager = GetComponent<GolfManager>();
        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        animIDGolfDrive = Animator.StringToHash("GolfDrive");
        animIDGolfChip = Animator.StringToHash("GolfChip");
        animIDGolfPutt = Animator.StringToHash("GolfPutt");
        animIDGolfIdle = Animator.StringToHash("GolfIdle");
    }

    public void SetShotType(IGolfShotType shotType)
    {
        currentShotType = shotType;
        // Set to golf idle when shot type is selected
        if (animator != null)
        {
            animator.SetBool(animIDGolfIdle, true);
        }
    }

    public void TriggerShotAnimation()
    {
        if (currentShotType == null || animator == null) return;

        // Get the appropriate animation trigger based on shot type
        string triggerName = currentShotType.AnimationTrigger;
        int animID = GetAnimationID(triggerName);

        if (animID != 0)
        {
            animator.SetTrigger(animID);
        }
    }

    private int GetAnimationID(string triggerName)
    {
        return triggerName switch
        {
            "GolfDrive" => animIDGolfDrive,
            "GolfChip" => animIDGolfChip,
            "GolfPutt" => animIDGolfPutt,
            _ => 0
        };
    }

    public void ExitGolfAnimations()
    {
        if (animator != null)
        {
            animator.SetBool(animIDGolfIdle, false);
        }
    }

    // Called by animation event when shot animation completes
    public void OnShotAnimationComplete()
    {
        // Return to idle and notify manager
        if (animator != null)
        {
            animator.SetBool(animIDGolfIdle, true);
        }

        // Notify manager shot is complete
        if (golfManager != null)
        {
            golfManager.OnShotAnimationComplete();
        }
    }
}