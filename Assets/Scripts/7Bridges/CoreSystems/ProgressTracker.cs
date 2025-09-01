// ProgressTracker.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks completion progress for tutorials and missions
/// Used by GameManager and future dashboard/main menu
/// </summary>
public class ProgressTracker : MonoBehaviour
{
    [Header("Progress Data")]
    [SerializeField] private List<TutorialCompletion> completedTutorials = new List<TutorialCompletion>();
    [SerializeField] private List<MissionCompletion> completedMissions = new List<MissionCompletion>();

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showProgressInInspector = true;

    // Events for UI to listen to
    public System.Action<JourneyType> OnTutorialCompleted;
    public System.Action<int, string> OnMissionCompleted;
    public System.Action OnProgressUpdated;

    void Awake()
    {
        // Make persistent across scenes (for future main menu integration)
        DontDestroyOnLoad(gameObject);
        LogDebug("ProgressTracker initialized");
    }

    #region Tutorial Progress

    /// <summary>
    /// Record a tutorial completion
    /// </summary>
    public void RecordTutorialCompletion(JourneyType tutorialType)
    {
        // Check if already completed
        if (IsTutorialCompleted(tutorialType))
        {
            LogDebug($"Tutorial {tutorialType} already recorded as completed");
            return;
        }

        var completion = new TutorialCompletion
        {
            tutorialType = tutorialType,
            completedTime = System.DateTime.Now,
            sessionTimestamp = Time.time
        };

        completedTutorials.Add(completion);

        LogDebug($"Recorded tutorial completion: {tutorialType}");

        // Notify listeners
        OnTutorialCompleted?.Invoke(tutorialType);
        OnProgressUpdated?.Invoke();
    }

    /// <summary>
    /// Check if a tutorial is completed
    /// </summary>
    public bool IsTutorialCompleted(JourneyType tutorialType)
    {
        return completedTutorials.Any(t => t.tutorialType == tutorialType);
    }

    /// <summary>
    /// Get all completed tutorial types
    /// </summary>
    public List<JourneyType> GetCompletedTutorials()
    {
        return completedTutorials.Select(t => t.tutorialType).ToList();
    }

    /// <summary>
    /// Get tutorial completion count
    /// </summary>
    public int GetTutorialCompletionCount()
    {
        return completedTutorials.Count;
    }

    /// <summary>
    /// Get total available tutorials
    /// </summary>
    public int GetTotalTutorialCount()
    {
        // All journey types except Invalid
        return System.Enum.GetValues(typeof(JourneyType)).Length - 1;
    }

    #endregion

    #region Mission Progress

    /// <summary>
    /// Record mission progress (highest mission reached)
    /// </summary>
    public void RecordMissionProgress(int missionIndex, string missionName)
    {
        // Check if mission already recorded
        var existingMission = completedMissions.FirstOrDefault(m => m.missionIndex == missionIndex);

        if (existingMission != null)
        {
            LogDebug($"Mission {missionIndex} ({missionName}) already recorded");
            return;
        }

        var completion = new MissionCompletion
        {
            missionIndex = missionIndex,
            missionName = missionName,
            completedTime = System.DateTime.Now,
            sessionTimestamp = Time.time
        };

        completedMissions.Add(completion);

        LogDebug($"Recorded mission progress: {missionIndex} - {missionName}");

        // Notify listeners
        OnMissionCompleted?.Invoke(missionIndex, missionName);
        OnProgressUpdated?.Invoke();
    }

    /// <summary>
    /// Check if a mission is completed
    /// </summary>
    public bool IsMissionCompleted(int missionIndex)
    {
        return completedMissions.Any(m => m.missionIndex == missionIndex);
    }

    /// <summary>
    /// Get highest completed mission index
    /// </summary>
    public int GetHighestCompletedMission()
    {
        if (completedMissions.Count == 0) return -1;
        return completedMissions.Max(m => m.missionIndex);
    }

    /// <summary>
    /// Get all completed missions
    /// </summary>
    public List<MissionCompletion> GetCompletedMissions()
    {
        return new List<MissionCompletion>(completedMissions);
    }

    /// <summary>
    /// Get mission completion count
    /// </summary>
    public int GetMissionCompletionCount()
    {
        return completedMissions.Count;
    }

    #endregion

    #region Overall Progress

    /// <summary>
    /// Get overall completion percentage
    /// </summary>
    public float GetOverallCompletionPercentage()
    {
        var tutorialProgress = GetTutorialCompletionCount() / (float)GetTotalTutorialCount();
        var missionProgress = GetMissionCompletionCount() / 5f; // Assuming 5 missions total

        return (tutorialProgress + missionProgress) / 2f * 100f;
    }

    /// <summary>
    /// Get progress summary for UI display
    /// </summary>
    public string GetProgressSummary()
    {
        var tutorialCount = GetTutorialCompletionCount();
        var totalTutorials = GetTotalTutorialCount();
        var missionCount = GetMissionCompletionCount();
        var overallPercentage = GetOverallCompletionPercentage();

        return $"Overall: {overallPercentage:F0}% | " +
               $"Tutorials: {tutorialCount}/{totalTutorials} | " +
               $"Missions: {missionCount}/5";
    }

    /// <summary>
    /// Reset all progress (for testing)
    /// </summary>
    public void ResetAllProgress()
    {
        completedTutorials.Clear();
        completedMissions.Clear();

        LogDebug("All progress reset");
        OnProgressUpdated?.Invoke();
    }

    #endregion

    #region Debug & Utility

    /// <summary>
    /// Force record completion for testing
    /// </summary>
    [ContextMenu("Test - Complete Walk Tutorial")]
    public void TestCompleteWalkTutorial()
    {
        RecordTutorialCompletion(JourneyType.Walk);
    }

    [ContextMenu("Test - Complete Trail Tutorial")]
    public void TestCompleteTrailTutorial()
    {
        RecordTutorialCompletion(JourneyType.Trail);
    }

    [ContextMenu("Test - Complete Mission 1")]
    public void TestCompleteMission1()
    {
        RecordMissionProgress(0, "Free Exploration");
    }

    [ContextMenu("Reset All Progress")]
    public void TestResetProgress()
    {
        ResetAllProgress();
    }

    [ContextMenu("Show Progress Summary")]
    public void ShowProgressSummary()
    {
        Debug.Log($"[ProgressTracker] {GetProgressSummary()}");

        if (completedTutorials.Count > 0)
        {
            Debug.Log($"Completed Tutorials: {string.Join(", ", completedTutorials.Select(t => t.tutorialType))}");
        }

        if (completedMissions.Count > 0)
        {
            Debug.Log($"Completed Missions: {string.Join(", ", completedMissions.Select(m => $"{m.missionIndex}: {m.missionName}"))}");
        }
    }

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[ProgressTracker] {message}");
    }

    // Update inspector display
    void Update()
    {
        if (!showProgressInInspector) return;

        // This helps see progress in inspector during development
        // Remove in production for performance
    }

    #endregion
}

// Data structures for storing completion info
[System.Serializable]
public class TutorialCompletion
{
    public JourneyType tutorialType;
    public System.DateTime completedTime;
    public float sessionTimestamp;

    public override string ToString()
    {
        return $"{tutorialType} (completed at {sessionTimestamp:F1}s)";
    }
}

[System.Serializable]
public class MissionCompletion
{
    public int missionIndex;
    public string missionName;
    public System.DateTime completedTime;
    public float sessionTimestamp;

    public override string ToString()
    {
        return $"Mission {missionIndex}: {missionName} (completed at {sessionTimestamp:F1}s)";
    }
}