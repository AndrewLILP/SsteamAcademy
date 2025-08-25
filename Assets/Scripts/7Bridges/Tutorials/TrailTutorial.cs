// TrailTutorial.cs
// Tutorial implementation for teaching Trail journey type - no repeated edges allowed

using UnityEngine;

/// <summary>
/// Trail tutorial implementation - teaches edge constraint concepts
/// Trails allow revisiting vertices but forbid using the same edge (bridge) twice
/// </summary>
public class TrailTutorial : BaseTutorial
{
    /// <summary>
    /// Specify this tutorial's journey type
    /// </summary>
    protected override JourneyType GetTutorialType()
    {
        return JourneyType.Trail;
    }

    /// <summary>
    /// Validate that the current journey meets trail requirements
    /// Trails are valid if they don't repeat edges - this includes paths, circuits, and cycles
    /// </summary>
    protected override bool ValidateSpecificRequirements()
    {
        var currentType = journeyTracker.GetCurrentJourneyType();
        
        // Trail allows Path, Circuit, and Cycle (all are valid trails)
        return currentType == JourneyType.Trail || 
               currentType == JourneyType.Path || 
               currentType == JourneyType.Circuit || 
               currentType == JourneyType.Cycle;
    }

    /// <summary>
    /// Custom completion behavior for trail tutorials
    /// Emphasizes the edge constraint concept
    /// </summary>
    public override void OnTutorialComplete()
    {
        Debug.Log("[TrailTutorial] Trail mastered! Student understands edge constraints while allowing vertex revisits.");
    }
}