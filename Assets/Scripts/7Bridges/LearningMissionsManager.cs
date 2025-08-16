using UnityEngine;
using System.Collections;
using TMPro;

// ==============================================
// LEARNING MISSIONS SYSTEM
// ==============================================

[System.Serializable]
public class MissionDefinition
{
    public string missionName;
    public JourneyType targetType;
    public string instructionText;
    public string hintText;
    public int minimumSteps;
    public bool showRealTimeFeedback;
}

public class LearningMissionsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject missionCompletePanel;

    [Header("Mission Definitions")]
    [SerializeField]
    private MissionDefinition[] missions = {
        new MissionDefinition {
            missionName = "Understanding Walks",
            targetType = JourneyType.Walk,
            instructionText = "Move between any 3 vertices in any pattern. This creates a 'walk' - the most basic journey type.",
            hintText = "You can repeat vertices and edges freely in a walk.",
            minimumSteps = 3,
            showRealTimeFeedback = true
        },
        new MissionDefinition {
            missionName = "Creating a Trail",
            targetType = JourneyType.Trail,
            instructionText = "Visit vertices without using the same bridge twice. This creates a 'trail'.",
            hintText = "You can revisit vertices, but don't cross the same bridge twice!",
            minimumSteps = 4,
            showRealTimeFeedback = true
        },
        new MissionDefinition {
            missionName = "Following a Path",
            targetType = JourneyType.Path,
            instructionText = "Visit each vertex only once to create a 'path' - the most restrictive journey type.",
            hintText = "No repeated vertices allowed in a path!",
            minimumSteps = 4,
            showRealTimeFeedback = true
        }
    };

    private int currentMissionIndex = 0;
    private JourneyTracker journeyTracker;
    private bool missionCompleted = false;

    void Start()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        if (journeyTracker == null)
        {
            Debug.LogError("LearningMissionsManager requires a JourneyTracker component!");
            return;
        }

        StartMission(0);
    }

    public void StartMission(int missionIndex)
    {
        if (missionIndex >= missions.Length) return;

        currentMissionIndex = missionIndex;
        var mission = missions[missionIndex];
        missionCompleted = false;

        // Reset journey tracker
        journeyTracker.ResetJourney();
        journeyTracker.SetMissionType(mission.targetType);

        // Update UI
        if (missionTitleText != null)
            missionTitleText.text = mission.missionName;

        if (instructionText != null)
            instructionText.text = mission.instructionText;

        if (hintText != null)
            hintText.text = $"💡 {mission.hintText}";

        if (feedbackText != null)
            feedbackText.text = "";

        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(false);

        Debug.Log($"Started mission: {mission.missionName}");
    }

    void Update()
    {
        if (missionCompleted) return;

        CheckMissionProgress();
    }

    private void CheckMissionProgress()
    {
        var mission = missions[currentMissionIndex];

        // Check if current journey matches mission requirements
        var currentType = GetCurrentJourneyType();
        bool correctType = currentType == mission.targetType;
        bool enoughSteps = GetCurrentJourneyLength() >= mission.minimumSteps;

        // Provide real-time feedback
        if (mission.showRealTimeFeedback && feedbackText != null)
        {
            if (!enoughSteps)
            {
                feedbackText.text = $"Keep going! Need {mission.minimumSteps - GetCurrentJourneyLength()} more steps.";
                feedbackText.color = Color.yellow;
            }
            else if (!correctType)
            {
                feedbackText.text = GetFeedbackForCurrentType(currentType, mission.targetType);
                feedbackText.color = Color.blue;
            }
            else
            {
                feedbackText.text = "Perfect! You've created the correct journey type!";
                feedbackText.color = Color.green;
            }
        }

        // Check for mission completion
        if (correctType && enoughSteps && !missionCompleted)
        {
            CompleteMission();
        }
    }

    private void CompleteMission()
    {
        missionCompleted = true;
        var mission = missions[currentMissionIndex];

        Debug.Log($"Mission Complete: {mission.missionName}");

        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
        }

        StartCoroutine(ShowCompletionSequence());
    }

    private IEnumerator ShowCompletionSequence()
    {
        var mission = missions[currentMissionIndex];

        // Show explanation
        if (feedbackText != null)
        {
            feedbackText.text = $"✅ Mission Complete!\n\n{journeyTracker.GetJourneyExplanation(mission.targetType)}";
            feedbackText.color = Color.green;
        }

        yield return new WaitForSeconds(3f);

        // Auto-advance to next mission or show completion
        if (currentMissionIndex < missions.Length - 1)
        {
            if (feedbackText != null)
            {
                feedbackText.text = "Get ready for the next mission...";
            }

            yield return new WaitForSeconds(2f);
            StartMission(currentMissionIndex + 1);
        }
        else
        {
            // All missions complete
            if (feedbackText != null)
            {
                feedbackText.text = "🎉 All missions complete! You now understand walks, trails, and paths!";
            }
        }
    }

    private string GetFeedbackForCurrentType(JourneyType current, JourneyType target)
    {
        return (current, target) switch
        {
            (JourneyType.Walk, JourneyType.Trail) =>
                "You've created a walk, but you're repeating edges. Try a different route!",
            (JourneyType.Walk, JourneyType.Path) =>
                "You've created a walk, but you're repeating vertices. Each vertex should be visited only once!",
            (JourneyType.Trail, JourneyType.Path) =>
                "Good trail! But you're repeating vertices. For a path, visit each vertex only once.",
            (JourneyType.Path, JourneyType.Trail) =>
                "Excellent path! You're actually exceeding the trail requirement.",
            _ => $"Current: {current}. Target: {target}. Keep trying!"
        };
    }

    // Helper methods to interface with JourneyTracker
    private JourneyType GetCurrentJourneyType()
    {
        // This would need to be exposed by JourneyTracker
        // For now, using reflection or add public method
        return journeyTracker.GetType()
            .GetMethod("AnalyzeCurrentJourney", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(journeyTracker, null) as JourneyType? ?? JourneyType.Walk;
    }

    private int GetCurrentJourneyLength()
    {
        // This would need to be exposed by JourneyTracker
        var field = journeyTracker.GetType()
            .GetField("currentJourney", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (field?.GetValue(journeyTracker) as System.Collections.IList)?.Count ?? 0;
    }

    // UI Button methods
    public void NextMission()
    {
        if (currentMissionIndex < missions.Length - 1)
        {
            StartMission(currentMissionIndex + 1);
        }
    }

    public void RestartCurrentMission()
    {
        StartMission(currentMissionIndex);
    }

    public void ShowHint()
    {
        var mission = missions[currentMissionIndex];
        if (feedbackText != null)
        {
            feedbackText.text = $"💡 Hint: {mission.hintText}";
            feedbackText.color = Color.blue;
        }
    }
}