// CircuitTutorial.cs
// Tutorial implementation for teaching Circuit journey type - trail that returns to start

using UnityEngine;

/// <summary>
/// Circuit tutorial implementation - teaches closed trail concepts
/// Circuits are trails (no repeated edges) that return to the starting vertex
/// </summary>
public class CircuitTutorial : BaseTutorial
{
    /// <summary>
    /// Specify this tutorial's journey type
    /// </summary>
    protected override JourneyType GetTutorialType()
    {
        return JourneyType.Circuit;
    }

    /// <summary>
    /// Validate that the current journey meets circuit requirements
    /// Must be exactly a circuit - trail that returns to start vertex
    /// </summary>
    protected override bool ValidateSpecificRequirements()
    {
        var currentType = journeyTracker.GetCurrentJourneyType();
        return currentType == JourneyType.Circuit;
    }

    /// <summary>
    /// Custom completion behavior for circuit tutorials
    /// Emphasizes the concept of closed trails and returning home
    /// </summary>
    public override void OnTutorialComplete()
    {
        Debug.Log("[CircuitTutorial] Circuit mastered! Student understands closed trails that return to origin.");
    }
}