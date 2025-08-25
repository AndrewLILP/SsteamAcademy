// PathTutorial.cs
// Tutorial implementation for teaching Path journey type - no repeated vertices allowed

using UnityEngine;

/// <summary>
/// Path tutorial implementation - teaches vertex constraint concepts
/// Paths require visiting each vertex at most once - the most restrictive non-closed journey type
/// </summary>
public class PathTutorial : BaseTutorial
{
    /// <summary>
    /// Specify this tutorial's journey type
    /// </summary>
    protected override JourneyType GetTutorialType()
    {
        return JourneyType.Path;
    }

    /// <summary>
    /// Validate that the current journey meets path requirements
    /// Paths are valid if they don't repeat vertices - this includes cycles (paths that return)
    /// </summary>
    protected override bool ValidateSpecificRequirements()
    {
        var currentType = journeyTracker.GetCurrentJourneyType();
        
        // Path allows Cycle (cycle is a valid path that returns)
        return currentType == JourneyType.Path || currentType == JourneyType.Cycle;
    }

    /// <summary>
    /// Custom completion behavior for path tutorials
    /// Emphasizes the vertex constraint and efficiency concepts
    /// </summary>
    public override void OnTutorialComplete()
    {
        Debug.Log("[PathTutorial] Path mastered! Student understands the most efficient vertex-restricted journey type.");
    }
}