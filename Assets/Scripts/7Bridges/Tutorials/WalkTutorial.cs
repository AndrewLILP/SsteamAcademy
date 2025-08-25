// WalkTutorial.cs
// Tutorial implementation for teaching Walk journey type - any sequence of vertices is valid

using UnityEngine;

/// <summary>
/// Walk tutorial implementation - teaches basic graph traversal concepts
/// Walks are the most permissive journey type: any movement between connected vertices is valid
/// </summary>
public class WalkTutorial : BaseTutorial
{
    /// <summary>
    /// Specify this tutorial's journey type
    /// </summary>
    protected override JourneyType GetTutorialType()
    {
        return JourneyType.Walk;
    }
    /// <summary>
    /// Validate that the current journey meets walk requirements
    /// For walks, any sequence is valid - just check minimum steps
    /// </summary>
    protected override bool ValidateSpecificRequirements()
    {
        // For walks, any sequence is valid - just check minimum steps and vertex variety
        return journeyTracker.GetCurrentJourneyLength() >= config.requiredSteps;
    }

    /// <summary>
    /// Provide walk-specific feedback during tutorial progression
    /// Shows students how their walk might also classify as other journey types
    /// </summary>
    protected override void UpdateProgressFeedback()
    {
        base.UpdateProgressFeedback();
        
        // Additional walk-specific feedback - show classification discovery
        var currentType = journeyTracker.GetCurrentJourneyType();
        if (currentType != JourneyType.Walk && journeyTracker.GetCurrentJourneyLength() >= 2)
        {
            var ui = GetComponent<ITutorialUI>();
            ui?.UpdateStatusMessage($"Your walk is also classified as: {currentType}!");
        }
    }

    /// <summary>
    /// Custom completion behavior for walk tutorials
    /// Emphasizes that walks are the foundation for all other journey types
    /// </summary>
    public override void OnTutorialComplete()
    {
        Debug.Log("[WalkTutorial] Walk mastered! Student ready for more restrictive journey types.");
    }
}