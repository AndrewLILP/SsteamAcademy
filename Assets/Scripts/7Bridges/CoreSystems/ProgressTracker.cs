// ProgressTracker.cs - Enhanced with Persistence
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks completion progress for tutorials and missions
/// Now includes persistence between game sessions
/// </summary>
public class ProgressTracker : MonoBehaviour
{
    [Header("Progress Data")]
    [SerializeField] private List<TutorialCompletion> completedTutorials = new List<TutorialCompletion>();
    [SerializeField] private List<MissionCompletion> completedMissions = new List<MissionCompletion>();

    [Header("Persistence Settings")]
    [SerializeField] private bool enablePersistence = true;
    [SerializeField] private bool saveImmediately = true; // Save after each completion

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showProgressInInspector = true;

    // Events for UI to listen to
    public System.Action<JourneyType> OnTutorialCompleted;
    public System.Action<int, string> OnMissionCompleted;
    public System.Action OnProgressUpdated;

    // Static reference for easy access
    public static ProgressTracker Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // DEMO: Always start with 0% completion
            ClearSavedProgress();

            LogDebug("ProgressTracker initialized - Demo mode: fresh start every launch");
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        // Auto-save every 30 seconds as backup
        if (enablePersistence)
        {
            InvokeRepeating(nameof(SaveProgress), 30f, 30f);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && enablePersistence)
        {
            SaveProgress();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && enablePersistence)
        {
            SaveProgress();
        }
    }

    void OnDestroy()
    {
        if (enablePersistence)
        {
            SaveProgress();
        }
    }

    #region Persistence Methods

    /// <summary>
    /// Save progress to PlayerPrefs
    /// </summary>
    public void SaveProgress()
    {
        if (!enablePersistence) return;

        try
        {
            // Save tutorial completions
            string tutorialData = "";
            foreach (var tutorial in completedTutorials)
            {
                tutorialData += (int)tutorial.tutorialType + ",";
            }
            PlayerPrefs.SetString("CompletedTutorials", tutorialData);

            // Save mission completions
            string missionData = "";
            foreach (var mission in completedMissions)
            {
                missionData += mission.missionIndex + ":" + mission.missionName + ",";
            }
            PlayerPrefs.SetString("CompletedMissions", missionData);

            // Save total stats
            PlayerPrefs.SetInt("TotalTutorials", completedTutorials.Count);
            PlayerPrefs.SetInt("TotalMissions", completedMissions.Count);
            PlayerPrefs.SetFloat("LastSaved", Time.time);

            PlayerPrefs.Save();
            LogDebug($"Progress saved - Tutorials: {completedTutorials.Count}, Missions: {completedMissions.Count}");
        }
        catch (System.Exception ex)
        {
            LogError($"Failed to save progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Load progress from PlayerPrefs
    /// </summary>
    public void LoadProgress()
    {
        if (!enablePersistence) return;

        try
        {
            // Load tutorial completions
            string tutorialData = PlayerPrefs.GetString("CompletedTutorials", "");
            if (!string.IsNullOrEmpty(tutorialData))
            {
                string[] tutorialTypes = tutorialData.Split(',');
                foreach (string typeStr in tutorialTypes)
                {
                    if (int.TryParse(typeStr, out int typeInt))
                    {
                        JourneyType type = (JourneyType)typeInt;
                        if (!completedTutorials.Any(t => t.tutorialType == type))
                        {
                            completedTutorials.Add(new TutorialCompletion
                            {
                                tutorialType = type,
                                completedTime = System.DateTime.Now, // Approximate
                                sessionTimestamp = Time.time
                            });
                        }
                    }
                }
            }

            // Load mission completions
            string missionData = PlayerPrefs.GetString("CompletedMissions", "");
            if (!string.IsNullOrEmpty(missionData))
            {
                string[] missions = missionData.Split(',');
                foreach (string missionStr in missions)
                {
                    if (!string.IsNullOrEmpty(missionStr) && missionStr.Contains(":"))
                    {
                        string[] parts = missionStr.Split(':');
                        if (int.TryParse(parts[0], out int index))
                        {
                            string name = parts.Length > 1 ? parts[1] : $"Mission {index + 1}";
                            if (!completedMissions.Any(m => m.missionIndex == index))
                            {
                                completedMissions.Add(new MissionCompletion
                                {
                                    missionIndex = index,
                                    missionName = name,
                                    completedTime = System.DateTime.Now, // Approximate
                                    sessionTimestamp = Time.time
                                });
                            }
                        }
                    }
                }
            }

            LogDebug($"Progress loaded - Tutorials: {completedTutorials.Count}, Missions: {completedMissions.Count}");
        }
        catch (System.Exception ex)
        {
            LogError($"Failed to load progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear all saved progress
    /// </summary>
    public void ClearSavedProgress()
    {
        PlayerPrefs.DeleteKey("CompletedTutorials");
        PlayerPrefs.DeleteKey("CompletedMissions");
        PlayerPrefs.DeleteKey("TotalTutorials");
        PlayerPrefs.DeleteKey("TotalMissions");
        PlayerPrefs.DeleteKey("LastSaved");
        PlayerPrefs.Save();

        completedTutorials.Clear();
        completedMissions.Clear();

        LogDebug("All saved progress cleared");
        OnProgressUpdated?.Invoke();
    }

    #endregion

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

        // Save immediately if enabled
        if (saveImmediately)
        {
            SaveProgress();
        }

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

        // Save immediately if enabled
        if (saveImmediately)
        {
            SaveProgress();
        }

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
    /// Reset all progress (current session only, doesn't clear saved data)
    /// </summary>
    public void ResetSessionProgress()
    {
        completedTutorials.Clear();
        completedMissions.Clear();

        LogDebug("Session progress reset (saved progress preserved)");
        OnProgressUpdated?.Invoke();
    }

    /// <summary>
    /// Reset all progress including saved data
    /// </summary>
    public void ResetAllProgress()
    {
        ClearSavedProgress();
        LogDebug("All progress reset completely");
    }

    #endregion

    #region Debug & Utility

    [ContextMenu("Test - Complete Walk Tutorial")]
    public void TestCompleteWalkTutorial()
    {
        RecordTutorialCompletion(JourneyType.Walk);
    }

    [ContextMenu("Test - Complete All Tutorials")]
    public void TestCompleteAllTutorials()
    {
        RecordTutorialCompletion(JourneyType.Walk);
        RecordTutorialCompletion(JourneyType.Trail);
        RecordTutorialCompletion(JourneyType.Path);
        RecordTutorialCompletion(JourneyType.Circuit);
        RecordTutorialCompletion(JourneyType.Cycle);
    }

    [ContextMenu("Test - Complete Mission 1")]
    public void TestCompleteMission1()
    {
        RecordMissionProgress(0, "Free Exploration");
    }

    [ContextMenu("Save Progress Now")]
    public void ForceSaveProgress()
    {
        SaveProgress();
    }

    [ContextMenu("Load Progress Now")]
    public void ForceLoadProgress()
    {
        LoadProgress();
    }

    [ContextMenu("Clear All Saved Progress")]
    public void TestClearProgress()
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

    private void LogError(string message)
    {
        Debug.LogError($"[ProgressTracker] {message}");
    }

    #endregion
}

// Data structures remain the same
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