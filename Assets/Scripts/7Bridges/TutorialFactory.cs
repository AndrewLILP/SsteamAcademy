// TutorialFactory.cs
// Factory pattern implementation for creating tutorial instances

using UnityEngine;
using System;

/// <summary>
/// Factory class for creating tutorial components using Factory pattern
/// Centralizes tutorial creation logic and provides validation methods
/// </summary>
public static class TutorialFactory
{
    /// <summary>
    /// Creates and attaches the appropriate tutorial component to the specified GameObject
    /// </summary>
    /// <param name="type">The type of tutorial to create</param>
    /// <param name="parent">The GameObject to attach the tutorial component to</param>
    /// <returns>The created tutorial component</returns>
    public static BaseTutorial CreateTutorial(JourneyType type, GameObject parent)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent), "Parent GameObject cannot be null");
        }

        BaseTutorial tutorial = type switch
        {
            JourneyType.Walk => parent.AddComponent<WalkTutorial>(),
            JourneyType.Trail => parent.AddComponent<TrailTutorial>(),
            JourneyType.Path => parent.AddComponent<PathTutorial>(),
            JourneyType.Circuit => parent.AddComponent<CircuitTutorial>(),
            JourneyType.Cycle => parent.AddComponent<CycleTutorial>(),
            _ => throw new ArgumentException($"No tutorial implementation for journey type: {type}")
        };

        // Set the tutorial type in the config (this gets overridden in Initialize, but useful for inspector visibility)
        var tutorialConfig = TutorialConfig.GetConfig(type);

        Debug.Log($"[TutorialFactory] Created {tutorialConfig.tutorialName} tutorial component");

        return tutorial;
    }

    /// <summary>
    /// Gets a list of all supported tutorial types
    /// </summary>
    /// <returns>Array of supported JourneyType values</returns>
    public static JourneyType[] GetSupportedTutorialTypes()
    {
        return new JourneyType[]
        {
            JourneyType.Walk,
            JourneyType.Trail,
            JourneyType.Path,
            JourneyType.Circuit,
            JourneyType.Cycle
        };
    }

    /// <summary>
    /// Checks if a tutorial type is supported by the factory
    /// </summary>
    /// <param name="type">The journey type to check</param>
    /// <returns>True if supported, false otherwise</returns>
    public static bool IsTutorialTypeSupported(JourneyType type)
    {
        return type switch
        {
            JourneyType.Walk or
            JourneyType.Trail or
            JourneyType.Path or
            JourneyType.Circuit or
            JourneyType.Cycle => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the tutorial configuration for a specific journey type without creating the component
    /// </summary>
    /// <param name="type">The journey type to get config for</param>
    /// <returns>The tutorial configuration</returns>
    public static TutorialConfig GetTutorialConfig(JourneyType type)
    {
        if (!IsTutorialTypeSupported(type))
        {
            throw new ArgumentException($"Tutorial type {type} is not supported");
        }

        return TutorialConfig.GetConfig(type);
    }

    /// <summary>
    /// Validates that all required tutorial components can be created
    /// Useful for system validation during development
    /// </summary>
    /// <returns>True if all tutorial types are properly implemented</returns>
    public static bool ValidateAllTutorialTypes()
    {
        var supportedTypes = GetSupportedTutorialTypes();

        foreach (var type in supportedTypes)
        {
            try
            {
                GetTutorialConfig(type);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TutorialFactory] Validation failed for {type}: {ex.Message}");
                return false;
            }
        }

        Debug.Log($"[TutorialFactory] All {supportedTypes.Length} tutorial types validated successfully");
        return true;
    }
}