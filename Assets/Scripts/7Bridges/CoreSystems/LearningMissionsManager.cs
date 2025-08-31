using UnityEngine;
using System.Collections;
using TMPro;

// ==============================================
// MISSION DEFINITION STRUCTURE
// ==============================================

[System.Serializable]
public class MissionDefinition
{
    public string missionName;
    public JourneyType targetType;
    public string learningObjective;
    public float completionDelay = 13f; // How long to show completion before next mission
    public bool showRealTimeFeedback = true;
}

// Enhanced LearningMissionsManager for Sprint 3 Task 1 - Mission State Isolation
public class LearningMissionsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private TextMeshProUGUI learningObjectiveText;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TextMeshProUGUI overallProgressText;

    [Header("Debug Settings - Sprint 3")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private float missionTransitionDelay = 0.5f; // Prevent rapid state changes

    [Header("Mission Definitions")]
    [SerializeField]
    private MissionDefinition[] missions = {
        new MissionDefinition {
            missionName = "Free Exploration",
            targetType = JourneyType.Walk,
            learningObjective = "Learn that any movement between connected vertices creates a 'walk' - the foundation of all journeys.",
            completionDelay = 12f
        },
        new MissionDefinition {
            missionName = "Bridge Management",
            targetType = JourneyType.Trail,
            learningObjective = "Discover how to create a 'trail' by using each bridge only once - vertices can be revisited.",
            completionDelay = 13f
        },
        new MissionDefinition {
            missionName = "Vertex Efficiency",
            targetType = JourneyType.Path,
            learningObjective = "Master the 'path' - the most efficient journey where each vertex is visited exactly once.",
            completionDelay = 13f
        },
        new MissionDefinition {
            missionName = "Closed Trails",
            targetType = JourneyType.Circuit,
            learningObjective = "Create a 'circuit' - a trail that returns to where you started, using each bridge only once.",
            completionDelay = 14f
        },
        new MissionDefinition {
            missionName = "Perfect Loops",
            targetType = JourneyType.Cycle,
            learningObjective = "Achieve a 'cycle' - a path that returns to start, visiting each vertex exactly once.",
            completionDelay = 14f
        }
    };

    private int currentMissionIndex = 0;
    private JourneyTracker journeyTracker;
    private bool missionCompleted = false;
    private bool missionTransitionInProgress = false;
    private Coroutine completionSequence;
    private float lastMissionStartTime = 0f;

    // Sprint 3 Debug tracking
    private int missionStartCount = 0;
    private int instantCompletionCount = 0;

    void Start()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        if (journeyTracker == null)
        {
            Debug.LogError("LearningMissionsManager requires a JourneyTracker component!");
            enabled = false;
            return;
        }

        // Enable debug mode on JourneyTracker if available
        if (enableDebugLogging && journeyTracker != null)
        {
            journeyTracker.EnableDebugMode(true);
        }

        StartMission(0);
    }

    void Update()
    {
        if (missionCompleted || missionTransitionInProgress) return;

        CheckMissionProgress();
        UpdateOverallProgress();
    }

    // SPRINT 3 TASK 1: Enhanced Mission Start with State Validation
    public void StartMission(int missionIndex)
    {
        if (missionIndex >= missions.Length)
        {
            ShowAllMissionsComplete();
            return;
        }

        // Prevent rapid mission transitions
        if (Time.time - lastMissionStartTime < missionTransitionDelay)
        {
            LogDebug($"Mission transition too soon - waiting {missionTransitionDelay}s between missions");
            StartCoroutine(DelayedMissionStart(missionIndex, missionTransitionDelay));
            return;
        }

        missionTransitionInProgress = true;
        missionStartCount++;
        lastMissionStartTime = Time.time;

        LogDebug($"=== STARTING MISSION {missionIndex + 1} ===");
        LogDebug($"Mission start #{missionStartCount} at time {Time.time:F2}");

        currentMissionIndex = missionIndex;
        var mission = missions[missionIndex];
        missionCompleted = false;

        // Stop any ongoing completion sequence
        if (completionSequence != null)
        {
            StopCoroutine(completionSequence);
            completionSequence = null;
            LogDebug("Stopped previous completion sequence");
        }

        // CRITICAL: Reset journey BEFORE setting mission type
        LogDebug("Resetting journey state...");
        
        // Pre-reset state logging
        if (journeyTracker != null)
        {
            LogDebug($"PRE-RESET: {journeyTracker.GetDebugInfo()}");
        }

        // Perform the reset
        journeyTracker?.ResetJourney();

        // Post-reset validation
        StartCoroutine(ValidateResetState(mission));
    }

    private IEnumerator DelayedMissionStart(int missionIndex, float delay)
    {
        LogDebug($"Delaying mission start for {delay}s");
        yield return new WaitForSeconds(delay);
        StartMission(missionIndex);
    }

    // SPRINT 3 TASK 1: State Validation Coroutine
    private IEnumerator ValidateResetState(MissionDefinition mission)
    {
        // Wait one frame to ensure reset operations complete
        yield return null;

        // Validate the reset worked properly
        bool resetValid = true;
        string validationErrors = "";

        if (journeyTracker != null)
        {
            LogDebug($"POST-RESET: {journeyTracker.GetDebugInfo()}");

            // Check journey length
            if (journeyTracker.GetCurrentJourneyLength() != 0)
            {
                resetValid = false;
                validationErrors += $"Journey not empty ({journeyTracker.GetCurrentJourneyLength()} steps); ";
            }

            // Additional validation could go here
            // For example, checking starting vertex is null, etc.
        }

        if (!resetValid)
        {
            LogError($"MISSION START VALIDATION FAILED: {validationErrors}");
            LogError("Attempting additional reset...");
            
            // Try additional reset
            journeyTracker?.ResetJourney();
            yield return null; // Wait another frame
            
            // Final validation
            if (journeyTracker.GetCurrentJourneyLength() != 0)
            {
                LogError("CRITICAL: Cannot clear journey state - serious bug detected!");
            }
        }

        // Now set the mission type
        LogDebug($"Setting mission type to: {mission.targetType}");
        journeyTracker?.SetMissionType(mission.targetType);

        // Update mission UI
        UpdateMissionUI(mission);

        // Hide completion panel
        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(false);

        missionTransitionInProgress = false;

        LogDebug($"Mission {currentMissionIndex + 1} ({mission.missionName}) started successfully");
        LogDebug($"=== MISSION START COMPLETE ===\n");
    }

    private void UpdateMissionUI(MissionDefinition mission)
    {
        if (missionTitleText != null)
        {
            missionTitleText.text = $"Mission {currentMissionIndex + 1}: {mission.missionName}";
        }

        if (learningObjectiveText != null)
        {
            learningObjectiveText.text = mission.learningObjective;
        }
    }

    // SPRINT 3 TASK 1: Enhanced Mission Progress Checking
    private void CheckMissionProgress()
    {
        // Don't check progress too soon after mission start
        if (Time.time - lastMissionStartTime < 0.1f)
        {
            return;
        }

        bool isComplete = IsMissionComplete();
        
        if (isComplete && !missionCompleted)
        {
            // Check for suspicious instant completion
            float timeSinceMissionStart = Time.time - lastMissionStartTime;
            if (timeSinceMissionStart < 1.0f) // Less than 1 second
            {
                instantCompletionCount++;
                LogWarning($"SUSPICIOUS: Mission completed in {timeSinceMissionStart:F3}s (instant completion #{instantCompletionCount})");
                
                if (journeyTracker != null)
                {
                    LogWarning($"Mission completion details: {journeyTracker.GetDebugInfo()}");
                }
            }
            
            CompleteMission();
        }
    }

    private bool IsMissionComplete()
    {
        if (journeyTracker == null) return false;
        
        bool complete = journeyTracker.IsMissionComplete();
        
        // Additional validation for Sprint 3
        if (complete)
        {
            LogDebug($"Mission completion detected: {journeyTracker.GetDebugInfo()}");
        }
        
        return complete;
    }

    private void CompleteMission()
    {
        missionCompleted = true;
        var mission = missions[currentMissionIndex];

        LogDebug($"=== MISSION {currentMissionIndex + 1} COMPLETED ===");
        LogDebug($"Mission: {mission.missionName}");
        LogDebug($"Time to complete: {Time.time - lastMissionStartTime:F2}s");
        
        if (journeyTracker != null)
        {
            LogDebug($"Final state: {journeyTracker.GetDebugInfo()}");
        }

        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
        }

        // Start completion sequence
        completionSequence = StartCoroutine(ShowCompletionSequence(mission));
    }

    private IEnumerator ShowCompletionSequence(MissionDefinition mission)
    {
        LogDebug($"Starting completion sequence for {mission.missionName}");
        
        // Show completion for specified duration
        yield return new WaitForSeconds(mission.completionDelay);

        // Auto-advance to next mission or show final completion
        if (currentMissionIndex < missions.Length - 1)
        {
            LogDebug($"Auto-advancing to mission {currentMissionIndex + 2}");
            StartMission(currentMissionIndex + 1);
        }
        else
        {
            LogDebug("All missions completed!");
            ShowAllMissionsComplete();
        }

        completionSequence = null;
    }

    private void ShowAllMissionsComplete()
    {
        LogDebug("=== ALL MISSIONS COMPLETED ===");
        
        if (missionTitleText != null)
        {
            missionTitleText.text = "All Missions Complete!";
        }

        if (learningObjectiveText != null)
        {
            learningObjectiveText.text = "You've mastered all journey types: walks, trails, paths, circuits, and cycles!\n\n" +
                                       "You now understand the mathematical foundations of graph theory and network analysis.";
        }

        if (overallProgressText != null)
        {
            overallProgressText.text = "Progress: 100% Complete";
            overallProgressText.color = Color.blue;
        }
    }

    private void UpdateOverallProgress()
    {
        if (overallProgressText != null)
        {
            float progress = (float)(currentMissionIndex + (missionCompleted ? 1 : 0)) / missions.Length;
            int percentage = Mathf.RoundToInt(progress * 100);

            overallProgressText.text = $"Overall Progress: {percentage}% ({currentMissionIndex + (missionCompleted ? 1 : 0)}/{missions.Length})";

            overallProgressText.color = percentage switch
            {
                100 => Color.blue,
                >= 80 => Color.green,
                >= 60 => Color.cyan,
                >= 40 => Color.yellow,
                _ => Color.white
            };
        }
    }

    // UI Button methods with enhanced validation
    public void NextMission()
    {
        LogDebug("Next mission requested by user");
        if (currentMissionIndex < missions.Length - 1)
        {
            StartMission(currentMissionIndex + 1);
        }
    }

    public void PreviousMission()
    {
        LogDebug("Previous mission requested by user");
        if (currentMissionIndex > 0)
        {
            StartMission(currentMissionIndex - 1);
        }
    }

    public void RestartCurrentMission()
    {
        LogDebug("Mission restart requested by user");
        StartMission(currentMissionIndex);
    }

    public void ShowHint()
    {
        var mission = missions[currentMissionIndex];
        var explanation = journeyTracker?.GetJourneyExplanation(mission.targetType) ?? "Journey tracker not available";

        LogDebug($"Hint requested for {mission.targetType}: {explanation}");
    }

    // Public methods for external control
    public void JumpToMission(int missionIndex)
    {
        LogDebug($"Jump to mission {missionIndex + 1} requested");
        if (missionIndex >= 0 && missionIndex < missions.Length)
        {
            StartMission(missionIndex);
        }
    }

    public int GetCurrentMissionIndex()
    {
        return currentMissionIndex;
    }

    public string GetCurrentMissionName()
    {
        return currentMissionIndex < missions.Length ? missions[currentMissionIndex].missionName : "Complete";
    }

    public JourneyType GetCurrentTargetType()
    {
        return currentMissionIndex < missions.Length ? missions[currentMissionIndex].targetType : JourneyType.Invalid;
    }

    // Sprint 3 Debug methods
    public string GetDebugInfo()
    {
        return $"Mission: {currentMissionIndex + 1}/{missions.Length}, " +
               $"Completed: {missionCompleted}, " +
               $"Transition: {missionTransitionInProgress}, " +
               $"Starts: {missionStartCount}, " +
               $"Instant completions: {instantCompletionCount}";
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[MissionsManager] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogging)
        {
            Debug.LogWarning($"[MissionsManager] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[MissionsManager] {message}");
    }
}