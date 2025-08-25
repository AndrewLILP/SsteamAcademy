// ITutorial.cs
// Contains all tutorial-related interfaces for the tutorial system

using UnityEngine;

/// <summary>
/// Main interface for all tutorial implementations
/// </summary>
public interface ITutorial
{
    bool IsComplete { get; }
    string TutorialName { get; }
    JourneyType TargetType { get; }
    void Initialize(JourneyTracker tracker);
    void CheckProgress();
    void Reset();
    void OnTutorialComplete();
}

/// <summary>
/// Interface for tutorial UI management
/// </summary>
public interface ITutorialUI
{
    void UpdateProgressMessage(string message);
    void UpdateStatusMessage(string message);
    void ShowCompletion(string completionMessage);
    void ShowExitOption();
}

/// <summary>
/// Interface for handling tutorial exit behavior
/// </summary>
public interface ITutorialExitHandler
{
    void HandleExit();
}