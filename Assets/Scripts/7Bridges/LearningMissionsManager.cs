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
    [SerializeField] private TextMeshProUGUI progressText;  // NEW: For showing progress
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

        // 🔥 ADD EVENT SUBSCRIPTIONS HERE:
        journeyTracker.OnMissionCompleted += OnMissionComplete;
        journeyTracker.OnJourneyTypeChanged += OnJourneyTypeChanged;
        journeyTracker.OnStepAdded += OnStepAdded;

        StartMission(0);
    }

    // 🔥 ADD THESE EVENT HANDLER METHODS:

    /// <summary>
    /// Called when the player completes a mission
    /// </summary>
    private void OnMissionComplete(JourneyType completedType, int journeyLength)
    {
        Debug.Log($"🎉 Mission Complete Event Received: {completedType} with {journeyLength} steps");

        missionCompleted = true;

        // Update UI to show completion
        if (feedbackText != null)
        {
            feedbackText.text = $"🎉 Mission Complete! You created a {completedType}!";
            feedbackText.color = Color.green;
        }

        // Show mission complete panel
        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
        }

        // Start the completion sequence
        StartCoroutine(ShowCompletionSequence());
    }

    /// <summary>
    /// Called when the journey type changes (e.g., Walk -> Trail)
    /// </summary>
    private void OnJourneyTypeChanged(JourneyType newType)
    {
        Debug.Log($"Journey type changed to: {newType}");

        var mission = missions[currentMissionIndex];

        // Provide immediate feedback about the type change
        if (feedbackText != null && mission.showRealTimeFeedback)
        {
            if (newType == mission.targetType)
            {
                feedbackText.text = $"✅ Perfect! You're creating a {newType}!";
                feedbackText.color = Color.green;
            }
            else
            {
                feedbackText.text = $"You're creating a {newType}. Target: {mission.targetType}";
                feedbackText.color = Color.blue;
            }
        }
    }

    /// <summary>
    /// Called every time the player takes a step
    /// </summary>
    private void OnStepAdded(JourneyStep step)
    {
        Debug.Log($"Step added: {step.vertexId}");
        UpdateProgressDisplay();
    }

    public void StartMission(int missionIndex)
    {
        if (missionIndex >= missions.Length) return;

        currentMissionIndex = missionIndex;
        var mission = missions[missionIndex];
        missionCompleted = false;

        // Reset journey tracker
        journeyTracker.ResetJourney();
        journeyTracker.SetMissionType(mission.targetType, mission.minimumSteps);

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

        UpdateProgressDisplay();

        Debug.Log($"Started mission: {mission.missionName}");
    }

    void Update()
    {
        if (missionCompleted) return;

        // 🔥 USE PUBLIC METHODS HERE:
        UpdateRealTimeFeedback();
    }

    /// <summary>
    /// Uses the new public methods to provide real-time feedback
    /// </summary>
    private void UpdateRealTimeFeedback()
    {
        var mission = missions[currentMissionIndex];
        if (!mission.showRealTimeFeedback) return;

        // 🔥 GET DATA USING NEW PUBLIC METHODS:
        var currentType = journeyTracker.GetCurrentJourneyType();
        var progress = journeyTracker.GetStepProgress();
        var journeyLength = journeyTracker.GetJourneyLength();
        var isComplete = journeyTracker.IsMissionComplete();

        // Update progress display
        UpdateProgressDisplay();

        // Only update feedback if mission isn't complete (to avoid overriding completion messages)
        if (!isComplete && feedbackText != null)
        {
            if (journeyLength < mission.minimumSteps)
            {
                int remaining = mission.minimumSteps - journeyLength;
                feedbackText.text = $"Keep going! Need {remaining} more steps.";
                feedbackText.color = Color.yellow;
            }
            else if (currentType != mission.targetType)
            {
                feedbackText.text = GetFeedbackForCurrentType(currentType, mission.targetType);
                feedbackText.color = Color.blue;
            }
        }
    }

    /// <summary>
    /// Updates the progress display using public methods
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressText == null) return;

        // 🔥 USE PUBLIC METHODS FOR PROGRESS DISPLAY:
        var journeyLength = journeyTracker.GetJourneyLength();
        var progress = journeyTracker.GetStepProgress();
        var currentType = journeyTracker.GetCurrentJourneyType();
        var mission = missions[currentMissionIndex];

        progressText.text = $"Progress: {journeyLength}/{mission.minimumSteps} steps\n" +
                           $"Current: {currentType} | Target: {mission.targetType}";

        // Color code based on progress
        if (progress >= 1f && currentType == mission.targetType)
        {
            progressText.color = Color.green;
        }
        else if (progress >= 0.5f)
        {
            progressText.color = Color.yellow;
        }
        else
        {
            progressText.color = Color.white;
        }
    }

    private void CheckMissionProgress()
    {
        // 🔥 REPLACE OLD LOGIC WITH NEW PUBLIC METHODS:
        var currentType = journeyTracker.GetCurrentJourneyType();
        var isComplete = journeyTracker.IsMissionComplete();

        // The mission completion is now handled by the OnMissionComplete event
        // so we don't need to check manually here anymore
    }

    private IEnumerator ShowCompletionSequence()
    {
        var mission = missions[currentMissionIndex];

        // 🔥 USE PUBLIC METHOD FOR EXPLANATION:
        var explanation = journeyTracker.GetJourneyExplanation(mission.targetType);

        // Show explanation
        if (feedbackText != null)
        {
            feedbackText.text = $"✅ Mission Complete!\n\n{explanation}";
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

    // 🔥 CLEANUP EVENTS WHEN DESTROYED:
    private void OnDestroy()
    {
        if (journeyTracker != null)
        {
            journeyTracker.OnMissionCompleted -= OnMissionComplete;
            journeyTracker.OnJourneyTypeChanged -= OnJourneyTypeChanged;
            journeyTracker.OnStepAdded -= OnStepAdded;
        }
    }
}