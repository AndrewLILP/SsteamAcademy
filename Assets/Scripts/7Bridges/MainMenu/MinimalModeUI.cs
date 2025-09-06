// MinimalModeUI.cs - Ultra-clean tutorial progression system
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal UI for clean tutorial progression
/// Auto-advances through tutorial sequence: Walk → Trail → Path → Circuit → Cycle
/// </summary>
public class MinimalModeUI : MonoBehaviour
{
    [Header("Main Mode Selection")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private Button practiceButton;
    [SerializeField] private Button missionsButton;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Tutorial Selection")]
    [SerializeField] private GameObject tutorialSelectionPanel;
    [SerializeField] private Button walkButton;
    [SerializeField] private Button trailButton;
    [SerializeField] private Button pathButton;
    [SerializeField] private Button circuitButton;
    [SerializeField] private Button cycleButton;
    [SerializeField] private Button backButton;

    [Header("Active Tutorial")]
    [SerializeField] private GameObject tutorialActivePanel;
    [SerializeField] private TextMeshProUGUI tutorialNameText;
    [SerializeField] private TextMeshProUGUI stepCounterText;
    [SerializeField] private Button exitTutorialButton;

    private GameManager gameManager;
    private ProgressTracker progressTracker;
    private JourneyTracker journeyTracker;
    private bool tutorialCompleted = false;

    // Tutorial sequence
    private readonly JourneyType[] tutorialSequence = {
        JourneyType.Walk,
        JourneyType.Trail,
        JourneyType.Path,
        JourneyType.Circuit,
        JourneyType.Cycle
    };

    void Start()
    {
        gameManager = GameManager.Instance;
        progressTracker = ProgressTracker.Instance;
        journeyTracker = FindFirstObjectByType<JourneyTracker>();

        SetupButtons();
        ShowModeSelection();
    }

    void Update()
    {
        UpdateActiveTutorialDisplay();
        CheckTutorialCompletion();
    }

    private void SetupButtons()
    {
        practiceButton?.onClick.AddListener(ShowTutorialSelection);
        missionsButton?.onClick.AddListener(StartMissions);
        backButton?.onClick.AddListener(ShowModeSelection);
        exitTutorialButton?.onClick.AddListener(ExitTutorial);

        // Tutorial buttons
        walkButton?.onClick.AddListener(() => StartTutorial(JourneyType.Walk));
        trailButton?.onClick.AddListener(() => StartTutorial(JourneyType.Trail));
        pathButton?.onClick.AddListener(() => StartTutorial(JourneyType.Path));
        circuitButton?.onClick.AddListener(() => StartTutorial(JourneyType.Circuit));
        cycleButton?.onClick.AddListener(() => StartTutorial(JourneyType.Cycle));
    }

    /// <summary>
    /// Show main mode selection
    /// </summary>
    public void ShowModeSelection()
    {
        modeSelectionPanel?.SetActive(true);
        tutorialSelectionPanel?.SetActive(false);
        tutorialActivePanel?.SetActive(false);

        DisableGameSystems();
        UpdateProgressSummary();
    }

    /// <summary>
    /// Show tutorial selection with completion indicators
    /// </summary>
    private void ShowTutorialSelection()
    {
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(true);
        tutorialActivePanel?.SetActive(false);

        UpdateTutorialButtons();
    }

    /// <summary>
    /// Start specific tutorial
    /// </summary>
    private void StartTutorial(JourneyType type)
    {
        tutorialCompleted = false;

        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(false);
        tutorialActivePanel?.SetActive(true);

        // Update tutorial name display
        if (tutorialNameText != null)
        {
            tutorialNameText.text = $"{type} Tutorial";
        }

        // DISABLE the old tutorial system to prevent conflicts
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.enabled = false;
        }

        // Set up JourneyTracker directly instead of going through GameManager
        if (journeyTracker != null)
        {
            journeyTracker.ResetJourney();
            journeyTracker.SetMissionType(type);
        }

        Debug.Log($"[MinimalModeUI] Started {type} tutorial with step counter");
    }

    /// <summary>
    /// Start mission mode
    /// </summary>
    private void StartMissions()
    {
        gameManager?.SelectMissionMode();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Exit current tutorial
    /// </summary>
    private void ExitTutorial()
    {
        ShowTutorialSelection();
    }

    /// <summary>
    /// Update progress summary on main screen
    /// </summary>
    private void UpdateProgressSummary()
    {
        if (progressText == null || progressTracker == null) return;

        int completed = progressTracker.GetTutorialCompletionCount();
        progressText.text = $"Progress: {completed}/5 tutorials completed";
    }

    /// <summary>
    /// Update tutorial buttons with completion status
    /// </summary>
    private void UpdateTutorialButtons()
    {
        if (progressTracker == null) return;

        UpdateButton(walkButton, JourneyType.Walk);
        UpdateButton(trailButton, JourneyType.Trail);
        UpdateButton(pathButton, JourneyType.Path);
        UpdateButton(circuitButton, JourneyType.Circuit);
        UpdateButton(cycleButton, JourneyType.Cycle);
    }

    private void UpdateButton(Button button, JourneyType type)
    {
        if (button == null) return;

        var text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            bool completed = progressTracker.IsTutorialCompleted(type);
            text.text = completed ? $"{type} ✓" : type.ToString();
            text.color = completed ? Color.green : Color.white;
        }
    }

    /// <summary>
    /// Update step counter during active tutorial
    /// </summary>
    private void UpdateActiveTutorialDisplay()
    {
        Debug.Log($"[DEBUG] Panel active: {tutorialActivePanel.activeInHierarchy}");
        Debug.Log($"[DEBUG] JourneyTracker: {journeyTracker != null}");
        Debug.Log($"[DEBUG] StepCounter: {stepCounterText != null}");

        if (!tutorialActivePanel.activeInHierarchy || journeyTracker == null) return;

        if (stepCounterText != null)
        {
            int steps = journeyTracker.GetCurrentJourneyLength();
            int required = GetRequiredSteps(journeyTracker.GetTargetJourneyType());

            stepCounterText.text = $"Steps: {steps}/{required}";
            stepCounterText.color = steps >= required ? Color.green : Color.white;
        }
    }

    /// <summary>
    /// Check for tutorial completion and auto-advance
    /// </summary>
    private void CheckTutorialCompletion()
    {
        if (tutorialCompleted || !tutorialActivePanel.activeInHierarchy || journeyTracker == null)
            return;

        if (journeyTracker.IsMissionComplete())
        {
            tutorialCompleted = true;
            OnTutorialCompleted();
        }
    }

    /// <summary>
    /// Handle tutorial completion - record progress and auto-advance
    /// </summary>
    private void OnTutorialCompleted()
    {
        var completedType = journeyTracker.GetTargetJourneyType();

        // Record completion
        progressTracker?.RecordTutorialCompletion(completedType);

        // Auto-advance to next tutorial
        var nextTutorial = GetNextTutorial(completedType);

        if (nextTutorial.HasValue)
        {
            // Brief pause then start next tutorial
            Invoke(nameof(StartNextTutorial), 2f);
        }
        else
        {
            // All tutorials complete - return to selection
            Invoke(nameof(AllTutorialsComplete), 2f);
        }
    }

    /// <summary>
    /// Start the next tutorial in sequence
    /// </summary>
    private void StartNextTutorial()
    {
        var currentType = journeyTracker.GetTargetJourneyType();
        var nextType = GetNextTutorial(currentType);

        if (nextType.HasValue)
        {
            StartTutorial(nextType.Value);
        }
    }

    /// <summary>
    /// Handle completion of all tutorials
    /// </summary>
    private void AllTutorialsComplete()
    {
        ShowTutorialSelection();
        // Could show special "all complete" message here
    }

    /// <summary>
    /// Get next tutorial in the sequence
    /// </summary>
    private JourneyType? GetNextTutorial(JourneyType current)
    {
        for (int i = 0; i < tutorialSequence.Length - 1; i++)
        {
            if (tutorialSequence[i] == current)
            {
                return tutorialSequence[i + 1];
            }
        }
        return null; // Last tutorial completed
    }

    /// <summary>
    /// Get required steps for tutorial completion
    /// </summary>
    private int GetRequiredSteps(JourneyType type)
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

    /// <summary>
    /// Disable game systems for clean state
    /// </summary>
    private void DisableGameSystems()
    {
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        var missionManager = FindFirstObjectByType<LearningMissionsManager>();

        if (tutorialManager != null)
            tutorialManager.enabled = false;
        if (missionManager != null)
            missionManager.enabled = false;
    }

    // Keyboard shortcuts
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialActivePanel.activeInHierarchy)
                ExitTutorial();
            else if (tutorialSelectionPanel.activeInHierarchy)
                ShowModeSelection();
            else
                gameManager?.ReturnToMainMenu();
        }
    }
}