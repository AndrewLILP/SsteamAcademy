// ModeSelectionUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the initial mode selection UI panel
/// Shows Tutorial/Mission mode buttons and basic progress info
/// </summary>
public class ModeSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button tutorialModeButton;
    [SerializeField] private Button missionModeButton;
    [SerializeField] private TextMeshProUGUI progressSummaryText;
    [SerializeField] private TextMeshProUGUI tutorialProgressText;
    [SerializeField] private TextMeshProUGUI missionProgressText;

    [Header("Optional References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;

    private GameManager gameManager;
    private ProgressTracker progressTracker;

    void Start()
    {
        // Find required references
        gameManager = FindFirstObjectByType<GameManager>();
        progressTracker = FindFirstObjectByType<ProgressTracker>();

        if (gameManager == null)
        {
            Debug.LogError("ModeSelectionUI: GameManager not found!");
            return;
        }

        // Setup button listeners
        SetupButtons();

        // Setup UI text
        SetupInitialText();

        // Update progress display
        UpdateProgressDisplay();

        // Listen for progress updates
        if (progressTracker != null)
        {
            progressTracker.OnProgressUpdated += UpdateProgressDisplay;
        }

        Debug.Log("[ModeSelectionUI] Initialized");
    }

    void OnDestroy()
    {
        // Cleanup listeners
        if (progressTracker != null)
        {
            progressTracker.OnProgressUpdated -= UpdateProgressDisplay;
        }
    }

    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtons()
    {
        if (tutorialModeButton != null && gameManager != null)
        {
            tutorialModeButton.onClick.RemoveAllListeners();
            tutorialModeButton.onClick.AddListener(() => {
                Debug.Log("[ModeSelectionUI] Tutorial mode button clicked");
                gameManager.SelectTutorialMode();
            });
        }
        else if (tutorialModeButton == null)
        {
            Debug.LogWarning("ModeSelectionUI: Tutorial Mode Button not assigned!");
        }

        if (missionModeButton != null && gameManager != null)
        {
            missionModeButton.onClick.RemoveAllListeners();
            missionModeButton.onClick.AddListener(() => {
                Debug.Log("[ModeSelectionUI] Mission mode button clicked");
                gameManager.SelectMissionMode();
            });
        }
        else if (missionModeButton == null)
        {
            Debug.LogWarning("ModeSelectionUI: Mission Mode Button not assigned!");
        }
    }

    /// <summary>
    /// Setup initial text content
    /// </summary>
    private void SetupInitialText()
    {
        if (titleText != null)
        {
            titleText.text = "7 Bridges - Graph Theory Explorer";
        }

        if (instructionText != null)
        {
            instructionText.text = "Choose your learning mode:\n\n" +
                                 "• Tutorial Mode: Learn individual concepts (Walk, Trail, Path, etc.)\n" +
                                 "• Mission Mode: Complete guided learning missions";
        }
    }

    /// <summary>
    /// Update progress display texts
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressTracker == null) return;

        // Overall progress summary
        if (progressSummaryText != null)
        {
            progressSummaryText.text = progressTracker.GetProgressSummary();
        }

        // Tutorial progress details
        if (tutorialProgressText != null)
        {
            var completedTutorials = progressTracker.GetCompletedTutorials();
            var totalTutorials = progressTracker.GetTotalTutorialCount();

            string tutorialDetails = $"Tutorials Completed: {completedTutorials.Count}/{totalTutorials}\n";

            if (completedTutorials.Count > 0)
            {
                tutorialDetails += "✓ " + string.Join(" ✓ ", completedTutorials);
            }
            else
            {
                tutorialDetails += "No tutorials completed yet";
            }

            tutorialProgressText.text = tutorialDetails;
        }

        // Mission progress details
        if (missionProgressText != null)
        {
            var completedMissions = progressTracker.GetCompletedMissions();
            var missionCount = progressTracker.GetMissionCompletionCount();

            string missionDetails = $"Missions Completed: {missionCount}/5\n";

            if (completedMissions.Count > 0)
            {
                missionDetails += "Completed:\n";
                foreach (var mission in completedMissions)
                {
                    missionDetails += $"✓ {mission.missionName}\n";
                }
            }
            else
            {
                missionDetails += "No missions completed yet";
            }

            missionProgressText.text = missionDetails.TrimEnd('\n');
        }
    }

    /// <summary>
    /// Called when panel becomes active
    /// </summary>
    void OnEnable()
    {
        // Refresh progress when panel shows
        UpdateProgressDisplay();
    }

    // Public methods for external control

    /// <summary>
    /// Enable/disable tutorial mode button
    /// </summary>
    public void SetTutorialButtonEnabled(bool enabled)
    {
        if (tutorialModeButton != null)
            tutorialModeButton.interactable = enabled;
    }

    /// <summary>
    /// Enable/disable mission mode button
    /// </summary>
    public void SetMissionButtonEnabled(bool enabled)
    {
        if (missionModeButton != null)
            missionModeButton.interactable = enabled;
    }

    /// <summary>
    /// Update button text (optional customization)
    /// </summary>
    public void SetButtonTexts(string tutorialText, string missionText)
    {
        if (tutorialModeButton != null)
        {
            var buttonText = tutorialModeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = tutorialText;
        }

        if (missionModeButton != null)
        {
            var buttonText = missionModeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = missionText;
        }
    }
}