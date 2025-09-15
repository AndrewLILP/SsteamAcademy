// MinimalModeUI.cs - Ultra-clean tutorial progression system with educational feedback
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Minimal UI for clean tutorial progression with educational feedback
/// Auto-advances through tutorial sequence: Walk → Trail → Path → Circuit → Cycle
/// Integrates with EducationalFeedbackManager for contextual learning messages
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
    private EducationalFeedbackManager educationalFeedback;
    private bool tutorialCompleted = false;
    private bool showingEducationalMessage = false;

    // Emergency backup system
    private int lastKnownStepCount = -1;
    private Coroutine uiUpdateCoroutine;

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

        // Initialize educational feedback
        educationalFeedback = GetComponent<EducationalFeedbackManager>();
        if (educationalFeedback == null)
        {
            educationalFeedback = gameObject.AddComponent<EducationalFeedbackManager>();
        }

        SetupButtons();
        ShowModeSelection();

        Debug.Log("[MinimalModeUI] Initialized with all systems including educational feedback");
    }

    // Bulletproof Update method - no conditions, no gates
    void Update()
    {
        // Force UI update every frame if components exist
        ForceUpdateStepCounter();
        CheckTutorialCompletion();
    }



    /// <summary>
    /// Hide all UI panels but keep the Canvas active for other systems
    /// </summary>
    public void HideAllPanels()
    {
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(false);
        tutorialActivePanel?.SetActive(false);

        // Hide StoryModeUI panels
        var storyModeUI = FindFirstObjectByType<StoryModeUI>();
        if (storyModeUI != null)
        {
            storyModeUI.SetUIVisible(false);
        }

        // Hide dialogue system
        var dialogueSystem = FindFirstObjectByType<DialogueSystem>();
        if (dialogueSystem != null)
        {
            dialogueSystem.HideDialogue();
        }

        Debug.Log("[MinimalModeUI] All panels hidden including story panels");

        Debug.Log("[MinimalModeUI] All panels hidden for story mode");
    }

    /// <summary>
    /// Show tutorial selection directly (skip mode selection)
    /// </summary>
    public void ShowTutorialSelectionDirectly()
    {
        // Hide all other panels first
        HideAllPanels();

        // Show only tutorial selection
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(true);
        tutorialActivePanel?.SetActive(false);

        UpdateTutorialButtons();
        Debug.Log("[MinimalModeUI] Showing tutorial selection directly");
    }

    // Simple, bulletproof UI update method
    private void ForceUpdateStepCounter()
    {
        // Skip updating if showing educational message
        if (showingEducationalMessage) return;

        // Bypass ALL conditions - just update if components exist
        if (journeyTracker != null && stepCounterText != null)
        {
            try
            {
                int steps = journeyTracker.GetCurrentJourneyLength();
                int required = GetRequiredSteps(journeyTracker.GetTargetJourneyType());

                string expectedText = $"Steps: {steps}/{required}";

                // Force update the text
                stepCounterText.text = expectedText;
                stepCounterText.color = steps >= required ? Color.green : Color.white;

                // Debug only when text changes to avoid spam
                //if (Time.frameCount % 60 == 0) // Every second
                //{
                //  Debug.Log($"[MinimalModeUI] UI Status: {expectedText} (Frame {Time.frameCount})");
                //}
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MinimalModeUI] Exception in ForceUpdateStepCounter: {ex.Message}");
            }
        }
        else
        {
            if (Time.frameCount % 60 == 0) // Every second  
            {
                //Debug.LogError($"[MinimalModeUI] Missing components - JourneyTracker: {journeyTracker != null}, StepText: {stepCounterText != null}");
            }
        }
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

        // Stop any running UI update coroutines
        if (uiUpdateCoroutine != null)
        {
            StopCoroutine(uiUpdateCoroutine);
            uiUpdateCoroutine = null;
        }

        // End educational feedback
        if (educationalFeedback != null)
        {
            educationalFeedback.EndTutorialFeedback();
        }

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

        // Stop any running UI update coroutines
        if (uiUpdateCoroutine != null)
        {
            StopCoroutine(uiUpdateCoroutine);
            uiUpdateCoroutine = null;
        }

        // End educational feedback
        if (educationalFeedback != null)
        {
            educationalFeedback.EndTutorialFeedback();
        }

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

        // COMPLETELY DISABLE the old tutorial system to prevent conflicts
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.enabled = false;
            tutorialManager.gameObject.SetActive(false);
        }

        var missionManager = FindFirstObjectByType<LearningMissionsManager>();
        if (missionManager != null)
        {
            missionManager.enabled = false;
        }

        // Set up JourneyTracker directly
        if (journeyTracker != null)
        {
            Debug.Log($"[MinimalModeUI] Setting up tutorial: {type}");
            journeyTracker.ResetJourney();
            journeyTracker.SetMissionType(type);

            // Verify the setup worked
            Debug.Log($"[MinimalModeUI] Journey length after reset: {journeyTracker.GetCurrentJourneyLength()}");
            Debug.Log($"[MinimalModeUI] Target type set to: {journeyTracker.GetTargetJourneyType()}");
        }
        else
        {
            //Debug.LogError("[MinimalModeUI] JourneyTracker is null!");
        }

        // Start educational feedback
        if (educationalFeedback != null)
        {
            educationalFeedback.StartTutorialFeedback(type);
        }

        // Reset step counter
        lastKnownStepCount = -1;

        // Start emergency backup UI update system
        if (uiUpdateCoroutine != null)
        {
            StopCoroutine(uiUpdateCoroutine);
        }
        uiUpdateCoroutine = StartCoroutine(ForceUIUpdates());

        Debug.Log($"[MinimalModeUI] Started {type} tutorial with EMERGENCY UI system and educational feedback");
    }

    /// <summary>
    /// Emergency backup UI update system via coroutine
    /// </summary>
    private IEnumerator ForceUIUpdates()
    {
        while (gameObject.activeInHierarchy)
        {
            // Force update every 0.1 seconds regardless of conditions
            if (journeyTracker != null && stepCounterText != null && !showingEducationalMessage)
            {
                int currentSteps = journeyTracker.GetCurrentJourneyLength();

                // Only update if steps actually changed
                if (currentSteps != lastKnownStepCount)
                {
                    lastKnownStepCount = currentSteps;
                    int required = GetRequiredSteps(journeyTracker.GetTargetJourneyType());

                    string newText = $"Steps: {currentSteps}/{required}";
                    stepCounterText.text = newText;
                    stepCounterText.color = currentSteps >= required ? Color.green : Color.white;

                    // Debug.Log($"[MinimalModeUI] FORCED UPDATE: {newText}");
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Start mission mode
    /// </summary>
    private void StartMissions()
    {
        gameManager.SelectStoryMode();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hide only tutorial/mode selection panels (not story panels)
    /// </summary>
    public void HideTutorialPanels()
    {
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(false);
        tutorialActivePanel?.SetActive(false);

        Debug.Log("[MinimalModeUI] Tutorial panels hidden (story panels preserved)");
    }

    /// <summary>
    /// Exit current tutorial
    /// </summary>
    private void ExitTutorial()
    {
        if (uiUpdateCoroutine != null)
        {
            StopCoroutine(uiUpdateCoroutine);
            uiUpdateCoroutine = null;
        }

        // End educational feedback
        if (educationalFeedback != null)
        {
            educationalFeedback.EndTutorialFeedback();
        }

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

        // Notify educational feedback system
        if (educationalFeedback != null)
        {
            educationalFeedback.OnTutorialCompleted(completedType);
        }

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

    /// <summary>
    /// Show educational message in step counter area
    /// Called by EducationalFeedbackManager
    /// </summary>
    public void ShowEducationalMessage(string message)
    {
        if (stepCounterText != null)
        {
            showingEducationalMessage = true;
            stepCounterText.text = message;
            stepCounterText.color = Color.cyan; // Different color for educational messages
            Debug.Log($"[MinimalModeUI] Showing educational message: {message}");
        }
    }

    /// <summary>
    /// Revert to normal step counter display
    /// Called by EducationalFeedbackManager
    /// </summary>
    public void RevertToStepCounter()
    {
        showingEducationalMessage = false;
        Debug.Log("[MinimalModeUI] Reverted to step counter display");
        // ForceUpdateStepCounter will be called on next Update
    }

    // Keyboard shortcuts
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialActivePanel != null && tutorialActivePanel.activeInHierarchy)
                ExitTutorial();
            else if (tutorialSelectionPanel != null && tutorialSelectionPanel.activeInHierarchy)
                ShowModeSelection();
            else
                gameManager?.ReturnToMainMenu();
        }
    }

    // Debug and validation methods
    [ContextMenu("Validate All UI References")]
    public void ValidateUIReferences()
    {
        Debug.Log("=== MinimalModeUI UI Validation ===");

        // Check main panels
        Debug.Log($"modeSelectionPanel: {(modeSelectionPanel != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"tutorialSelectionPanel: {(tutorialSelectionPanel != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"tutorialActivePanel: {(tutorialActivePanel != null ? "ASSIGNED" : "NULL")}");

        if (tutorialActivePanel != null)
        {
            Debug.Log($"tutorialActivePanel name: {tutorialActivePanel.name}");
            Debug.Log($"tutorialActivePanel active: {tutorialActivePanel.activeSelf}");
            Debug.Log($"tutorialActivePanel activeInHierarchy: {tutorialActivePanel.activeInHierarchy}");
        }

        // Check text components
        Debug.Log($"tutorialNameText: {(tutorialNameText != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"stepCounterText: {(stepCounterText != null ? "ASSIGNED" : "NULL")}");

        if (stepCounterText != null)
        {
            Debug.Log($"stepCounterText name: {stepCounterText.gameObject.name}");
            Debug.Log($"stepCounterText active: {stepCounterText.gameObject.activeInHierarchy}");
            Debug.Log($"stepCounterText current text: '{stepCounterText.text}'");
            Debug.Log($"stepCounterText font: {stepCounterText.font}");
            Debug.Log($"stepCounterText fontSize: {stepCounterText.fontSize}");
            Debug.Log($"stepCounterText color: {stepCounterText.color}");
        }

        // Check systems
        Debug.Log($"gameManager: {(gameManager != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"progressTracker: {(progressTracker != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"journeyTracker: {(journeyTracker != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"educationalFeedback: {(educationalFeedback != null ? "ASSIGNED" : "NULL")}");

        if (journeyTracker != null)
        {
            Debug.Log($"journeyTracker current length: {journeyTracker.GetCurrentJourneyLength()}");
            Debug.Log($"journeyTracker target type: {journeyTracker.GetTargetJourneyType()}");
        }

        Debug.Log("=== End UI Validation ===");
    }

    [ContextMenu("Force UI Update")]
    public void ForceUIUpdate()
    {
        Debug.Log("[MinimalModeUI] Forcing UI update...");
        ForceUpdateStepCounter();
    }

    [ContextMenu("Test Tutorial Start")]
    public void TestTutorialStart()
    {
        Debug.Log("[MinimalModeUI] Testing tutorial start...");
        StartTutorial(JourneyType.Walk);
    }

    void OnDestroy()
    {
        // Clean up coroutines
        if (uiUpdateCoroutine != null)
        {
            StopCoroutine(uiUpdateCoroutine);
        }
    }


}