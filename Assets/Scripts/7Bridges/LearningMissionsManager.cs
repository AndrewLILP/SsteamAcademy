using UnityEngine;
using System.Collections;
using TMPro;

[System.Serializable]
public class MissionDefinition
{
    public string missionName;
    public JourneyType targetType;
    public string learningObjective;
    public float completionDelay = 3f; // How long to show completion before next mission
    public bool showRealTimeFeedback = true;
}

public class LearningMissionsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private TextMeshProUGUI learningObjectiveText;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TextMeshProUGUI overallProgressText;

    [Header("Mission Definitions")]
    [SerializeField]
    private MissionDefinition[] missions = {
        new MissionDefinition {
            missionName = "Free Exploration",
            targetType = JourneyType.Walk,
            learningObjective = "Learn that any movement between connected vertices creates a 'walk' - the foundation of all journeys.",
            completionDelay = 2f
        },
        new MissionDefinition {
            missionName = "Bridge Management",
            targetType = JourneyType.Trail,
            learningObjective = "Discover how to create a 'trail' by using each bridge only once - vertices can be revisited.",
            completionDelay = 3f
        },
        new MissionDefinition {
            missionName = "Vertex Efficiency",
            targetType = JourneyType.Path,
            learningObjective = "Master the 'path' - the most efficient journey where each vertex is visited exactly once.",
            completionDelay = 3f
        },
        new MissionDefinition {
            missionName = "Closed Trails",
            targetType = JourneyType.Circuit,
            learningObjective = "Create a 'circuit' - a trail that returns to where you started, using each bridge only once.",
            completionDelay = 4f
        },
        new MissionDefinition {
            missionName = "Perfect Loops",
            targetType = JourneyType.Cycle,
            learningObjective = "Achieve a 'cycle' - a path that returns to start, visiting each vertex exactly once.",
            completionDelay = 4f
        }
    };

    private int currentMissionIndex = 0;
    private JourneyTracker journeyTracker;
    private bool missionCompleted = false;
    private Coroutine completionSequence;

    void Start()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        if (journeyTracker == null)
        {
            Debug.LogError("LearningMissionsManager requires a JourneyTracker component!");
            enabled = false;
            return;
        }

        StartMission(0);
    }

    void Update()
    {
        if (missionCompleted) return;

        CheckMissionProgress();
        UpdateOverallProgress();
    }

    public void StartMission(int missionIndex)
    {
        if (missionIndex >= missions.Length)
        {
            ShowAllMissionsComplete();
            return;
        }

        currentMissionIndex = missionIndex;
        var mission = missions[missionIndex];
        missionCompleted = false;

        // Stop any ongoing completion sequence
        if (completionSequence != null)
        {
            StopCoroutine(completionSequence);
            completionSequence = null;
        }

        // Reset and configure journey tracker for this mission
        journeyTracker.ResetJourney();
        journeyTracker.SetMissionType(mission.targetType);

        // Update mission UI
        UpdateMissionUI(mission);

        // Hide completion panel
        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(false);

        Debug.Log($"Started mission {missionIndex + 1}: {mission.missionName}");
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

    private void CheckMissionProgress()
    {
        // The JourneyTracker now handles all the feedback, we just check for completion
        if (IsMissionComplete() && !missionCompleted)
        {
            CompleteMission();
        }
    }

    private bool IsMissionComplete()
    {
        // Delegate to JourneyTracker's completion logic
        return journeyTracker.IsMissionComplete();
    }

    private void CompleteMission()
    {
        missionCompleted = true;
        var mission = missions[currentMissionIndex];

        Debug.Log($"Mission {currentMissionIndex + 1} Complete: {mission.missionName}");

        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
        }

        // Start completion sequence
        completionSequence = StartCoroutine(ShowCompletionSequence(mission));
    }

    private IEnumerator ShowCompletionSequence(MissionDefinition mission)
    {
        // Show completion for specified duration
        yield return new WaitForSeconds(mission.completionDelay);

        // Auto-advance to next mission or show final completion
        if (currentMissionIndex < missions.Length - 1)
        {
            StartMission(currentMissionIndex + 1);
        }
        else
        {
            ShowAllMissionsComplete();
        }

        completionSequence = null;
    }

    private void ShowAllMissionsComplete()
    {
        if (missionTitleText != null)
        {
            missionTitleText.text = "🎉 All Missions Complete!";
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

    // UI Button methods
    public void NextMission()
    {
        if (currentMissionIndex < missions.Length - 1)
        {
            StartMission(currentMissionIndex + 1);
        }
    }

    public void PreviousMission()
    {
        if (currentMissionIndex > 0)
        {
            StartMission(currentMissionIndex - 1);
        }
    }

    public void RestartCurrentMission()
    {
        StartMission(currentMissionIndex);
    }

    public void ShowHint()
    {
        var mission = missions[currentMissionIndex];
        var explanation = journeyTracker.GetJourneyExplanation(mission.targetType);

        Debug.Log($"Hint for {mission.targetType}: {explanation}");

        // Could show this in a hint panel or temporary UI element
    }

    // Public methods for external control
    public void JumpToMission(int missionIndex)
    {
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
}