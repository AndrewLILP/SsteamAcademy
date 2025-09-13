// TutorialProgressUI.cs - Clean Progress Display for Active Tutorials
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple, clean progress UI that shows during tutorial play
/// Displays current progress, tutorial type, and completion status
/// </summary>
public class TutorialProgressUI : MonoBehaviour
{
    [Header("Progress Display")]
    [SerializeField] private TextMeshProUGUI tutorialTitleText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI journeyPathText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Action Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button nextTutorialButton;
    [SerializeField] private Button startMissionButton;

    [Header("Completion Panel")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private Button continueButton;

    private JourneyTracker journeyTracker;
    private TutorialManager tutorialManager;
    private ModeSelectorUI modeSelectorUI;
    private bool tutorialCompleted = false;

    void Start()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        tutorialManager = FindFirstObjectByType<TutorialManager>();
        modeSelectorUI = FindFirstObjectByType<ModeSelectorUI>();

        SetupButtons();
        HideCompletionPanel();

        Debug.Log("[TutorialProgressUI] Initialized");
    }

    void Update()
    {
        UpdateProgressDisplay();
        CheckForCompletion();
    }

    private void SetupButtons()
    {
        resetButton?.onClick.AddListener(ResetTutorial);
        exitButton?.onClick.AddListener(ExitToModeSelection);
        nextTutorialButton?.onClick.AddListener(ShowNextTutorialOptions);
        startMissionButton?.onClick.AddListener(StartMissionMode);
        continueButton?.onClick.AddListener(HideCompletionPanel);
    }

    /// <summary>
    /// Update the progress display with current tutorial state
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (journeyTracker == null) return;

        // Tutorial title
        if (tutorialTitleText != null && tutorialManager != null)
        {
            tutorialTitleText.text = $"Tutorial: {tutorialManager.CurrentTutorialName}";
        }

        // Progress status
        if (progressText != null)
        {
            var steps = journeyTracker.GetCurrentJourneyLength();
            var currentType = journeyTracker.GetCurrentJourneyType();
            var targetType = journeyTracker.GetTargetJourneyType();

            progressText.text = $"Steps: {steps} | Creating: {currentType}";

            // Color coding
            if (currentType == targetType && steps >= GetMinimumSteps(targetType))
            {
                progressText.color = Color.green;
            }
            else if (steps > 0)
            {
                progressText.color = Color.yellow;
            }
            else
            {
                progressText.color = Color.white;
            }
        }

        // Journey path
        if (journeyPathText != null)
        {
            var steps = journeyTracker.GetCurrentJourneyLength();
            if (steps > 0)
            {
                journeyPathText.text = GetJourneyPathString();
            }
            else
            {
                journeyPathText.text = "Ready to start - move to any vertex!";
            }
        }

        // Instructions
        if (instructionText != null)
        {
            instructionText.text = GetInstructionText();
        }
    }

    /// <summary>
    /// Check if tutorial is completed and show completion UI
    /// </summary>
    private void CheckForCompletion()
    {
        if (tutorialCompleted || journeyTracker == null) return;

        bool isComplete = journeyTracker.IsMissionComplete();

        if (isComplete)
        {
            tutorialCompleted = true;
            ShowCompletionPanel();
            RecordProgress();
        }
    }

    /// <summary>
    /// Show completion panel with options
    /// </summary>
    private void ShowCompletionPanel()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);

            if (completionText != null)
            {
                var tutorialType = journeyTracker.GetTargetJourneyType();
                completionText.text = $"{tutorialType} Tutorial Complete!\n\nYou successfully created a {tutorialType.ToString().ToLower()}.\n\nWhat would you like to do next?";
            }
        }

        Debug.Log("[TutorialProgressUI] Tutorial completed!");
    }

    /// <summary>
    /// Hide completion panel
    /// </summary>
    private void HideCompletionPanel()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    /// <summary>
    /// Record tutorial completion
    /// </summary>
    private void RecordProgress()
    {
        var progressTracker = ProgressTracker.Instance;
        if (progressTracker != null)
        {
            var tutorialType = journeyTracker.GetTargetJourneyType();
            progressTracker.RecordTutorialCompletion(tutorialType);
            Debug.Log($"[TutorialProgressUI] Recorded completion: {tutorialType}");
        }
    }

    /// <summary>
    /// Reset current tutorial
    /// </summary>
    private void ResetTutorial()
    {
        tutorialCompleted = false;
        tutorialManager?.ResetTutorial();
        HideCompletionPanel();
        Debug.Log("[TutorialProgressUI] Tutorial reset");
    }

    /// <summary>
    /// Exit to mode selection
    /// </summary>
    private void ExitToModeSelection()
    {
        gameObject.SetActive(false);
        modeSelectorUI?.ShowModeSelection();
        Debug.Log("[TutorialProgressUI] Exited to mode selection");
    }

    /// <summary>
    /// Show options for next tutorial
    /// </summary>
    private void ShowNextTutorialOptions()
    {
        gameObject.SetActive(false);
        modeSelectorUI?.ShowModeSelection();
        Debug.Log("[TutorialProgressUI] Showing tutorial options");
    }

    /// <summary>
    /// Start mission mode
    /// </summary>
    private void StartMissionMode()
    {
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameObject.SetActive(false);
            gameManager.SelectStoryMode();
            Debug.Log("[TutorialProgressUI] Starting mission mode");
        }
    }

    /// <summary>
    /// Get journey path as readable string
    /// </summary>
    private string GetJourneyPathString()
    {
        // This would need to be implemented based on your JourneyTracker's data structure
        // For now, return a simple representation
        return "Path: A → B → C"; // Placeholder - replace with actual path tracking
    }

    /// <summary>
    /// Get instruction text for current tutorial
    /// </summary>
    private string GetInstructionText()
    {
        if (journeyTracker == null) return "";

        var targetType = journeyTracker.GetTargetJourneyType();
        var steps = journeyTracker.GetCurrentJourneyLength();
        var minSteps = GetMinimumSteps(targetType);

        if (steps < minSteps)
        {
            return GetTutorialInstruction(targetType);
        }
        else
        {
            return "Keep going or complete the tutorial!";
        }
    }

    /// <summary>
    /// Get tutorial-specific instructions
    /// </summary>
    private string GetTutorialInstruction(JourneyType type)
    {
        return type switch
        {
            JourneyType.Walk => "Move freely between vertices. Any path is valid!",
            JourneyType.Trail => "Don't use the same bridge twice. Vertices can be revisited.",
            JourneyType.Path => "Visit each vertex only once. Most efficient journey!",
            JourneyType.Circuit => "Create a trail that returns to your starting point.",
            JourneyType.Cycle => "Create a path that returns to your starting point.",
            _ => "Explore the network!"
        };
    }

    /// <summary>
    /// Get minimum steps required for tutorial completion
    /// </summary>
    private int GetMinimumSteps(JourneyType type)
    {
        return type switch
        {
            JourneyType.Walk => 3,
            JourneyType.Trail => 4,
            JourneyType.Path => 4,
            JourneyType.Circuit => 5,
            JourneyType.Cycle => 5,
            _ => 3
        };
    }

    // Public methods for external control

    /// <summary>
    /// Show this UI (called when tutorial starts)
    /// </summary>
    public void ShowTutorialUI()
    {
        gameObject.SetActive(true);
        tutorialCompleted = false;
        HideCompletionPanel();
    }

    /// <summary>
    /// Hide this UI (called when tutorial ends)
    /// </summary>
    public void HideTutorialUI()
    {
        gameObject.SetActive(false);
    }
}