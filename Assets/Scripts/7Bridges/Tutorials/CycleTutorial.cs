// CycleTutorial.cs
// Tutorial implementation for teaching Cycle journey type - path that returns to start

using UnityEngine;

/// <summary>
/// Cycle tutorial implementation - teaches closed path concepts
/// Cycles are paths (no repeated vertices) that return to the starting vertex
/// The most restrictive and elegant journey type
/// </summary>
public class CycleTutorial : BaseTutorial
{
    /// <summary>
    /// Specify this tutorial's journey type
    /// </summary>
    protected override JourneyType GetTutorialType()
    {
        return JourneyType.Cycle;
    }

    /// <summary>
    /// Validate that the current journey meets cycle requirements
    /// Must be exactly a cycle - path that returns to start vertex
    /// </summary>
    protected override bool ValidateSpecificRequirements()
    {
        var currentType = journeyTracker.GetCurrentJourneyType();
        return currentType == JourneyType.Cycle;
    }

    /// <summary>
    /// Custom completion behavior for cycle tutorials
    /// Emphasizes the elegance and perfection of closed paths
    /// </summary>
    public override void OnTutorialComplete()
    {
        Debug.Log("[CycleTutorial] Cycle mastered! Student has conquered the most elegant journey type - the perfect closed path.");
    }
}