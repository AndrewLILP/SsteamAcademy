// TutorialConfig.cs
// Configuration data and settings for all tutorial types

using UnityEngine;
using System;

/// <summary>
/// Configuration data for tutorial behavior and messaging
/// </summary>
[System.Serializable]
public class TutorialConfig
{
    public JourneyType journeyType;
    public string tutorialName;
    public int requiredSteps;
    public int minimumUniqueVertices;
    public bool requiresReturnsToStart;
    public string[] progressMessages;
    public string completionMessage;
    public string instructionMessage;

    /// <summary>
    /// Factory method to get pre-configured tutorial settings
    /// </summary>
    /// <param name="type">The journey type to get configuration for</param>
    /// <returns>Configured TutorialConfig for the specified type</returns>
    public static TutorialConfig GetConfig(JourneyType type)
    {
        return type switch
        {
            JourneyType.Walk => new TutorialConfig
            {
                journeyType = JourneyType.Walk,
                tutorialName = "Understanding Walks",
                requiredSteps = 3,
                minimumUniqueVertices = 2,
                requiresReturnsToStart = false,
                progressMessages = new[]
                {
                    "Step anywhere to begin your walk!",
                    "Great start! Move to any connected vertex.",
                    "Building your walk! One more step shows the freedom of walks.",
                    "Perfect! Walks allow complete movement freedom."
                },
                completionMessage = "Excellent! You've mastered walks - the foundation of all graph journeys.",
                instructionMessage = "Create a walk by moving between any connected vertices. Any movement is valid!"
            },

            JourneyType.Trail => new TutorialConfig
            {
                journeyType = JourneyType.Trail,
                tutorialName = "Creating Trails",
                requiredSteps = 4,
                minimumUniqueVertices = 3,
                requiresReturnsToStart = false,
                progressMessages = new[]
                {
                    "Begin your trail - remember: no bridge twice!",
                    "Good! You can revisit vertices, but not bridges.",
                    "Building your trail! Each bridge used only once.",
                    "Trail progress! Keep avoiding repeated bridges.",
                    "Perfect trail! No bridges were repeated."
                },
                completionMessage = "Outstanding! You've created a trail - vertices can repeat, but bridges cannot.",
                instructionMessage = "Create a trail by avoiding repeated bridges. You can revisit vertices!"
            },

            JourneyType.Path => new TutorialConfig
            {
                journeyType = JourneyType.Path,
                tutorialName = "Mastering Paths",
                requiredSteps = 4,
                minimumUniqueVertices = 4,
                requiresReturnsToStart = false,
                progressMessages = new[]
                {
                    "Start your path - each vertex only once!",
                    "Excellent! Keep visiting new vertices only.",
                    "Path building! No repeated vertices allowed.",
                    "Almost there! One more unique vertex.",
                    "Perfect path! Each vertex visited exactly once."
                },
                completionMessage = "Brilliant! You've mastered paths - the most efficient journey type.",
                instructionMessage = "Create a path by visiting each vertex exactly once. No repeats allowed!"
            },

            JourneyType.Circuit => new TutorialConfig
            {
                journeyType = JourneyType.Circuit,
                tutorialName = "Closing Circuits",
                requiredSteps = 5,
                minimumUniqueVertices = 3,
                requiresReturnsToStart = true,
                progressMessages = new[]
                {
                    "Begin your circuit - trail that returns home!",
                    "Good start! Build your trail, remember to return.",
                    "Circuit progress! No repeated bridges.",
                    "Almost ready to close your circuit!",
                    "Ready to return home? Complete the circuit!",
                    "Perfect circuit! You created a closed trail."
                },
                completionMessage = "Exceptional! You've mastered circuits - closed trails that return home.",
                instructionMessage = "Create a circuit: build a trail that returns to your starting vertex!"
            },

            JourneyType.Cycle => new TutorialConfig
            {
                journeyType = JourneyType.Cycle,
                tutorialName = "Perfect Cycles",
                requiredSteps = 5,
                minimumUniqueVertices = 4,
                requiresReturnsToStart = true,
                progressMessages = new[]
                {
                    "Begin your cycle - path that returns home!",
                    "Excellent! Each vertex once, then return.",
                    "Cycle building! No repeated vertices.",
                    "Perfect progress! Keep each vertex unique.",
                    "Almost home! Ready to complete the cycle?",
                    "Magnificent cycle! A perfect closed path."
                },
                completionMessage = "Outstanding! You've mastered cycles - the most elegant journey type.",
                instructionMessage = "Create a cycle: visit each vertex once and return to start!"
            },

            _ => throw new ArgumentException($"No configuration for journey type: {type}")
        };
    }
}