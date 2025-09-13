// ModeSelectorUI.cs - Clean Tutorial/Mission Selection Interface
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple, clear mode selection interface
/// Shows when entering MVP2 scene - lets user choose Tutorial or Mission mode
/// </summary>
public class ModeSelectorUI : MonoBehaviour
{
    [Header("Main Mode Selection")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private Button tutorialModeButton;
    [SerializeField] private Button missionModeButton;
    [SerializeField] private Button backToMainMenuButton;

    [Header("Tutorial Selection (shows after clicking Tutorial Mode)")]
    [SerializeField] private GameObject tutorialSelectionPanel;
    [SerializeField] private Button walkTutorialButton;
    [SerializeField] private Button trailTutorialButton;
    [SerializeField] private Button pathTutorialButton;
    [SerializeField] private Button circuitTutorialButton;
    [SerializeField] private Button cycleTutorialButton;
    [SerializeField] private Button backToModeSelectButton;

    [Header("Progress Display")]
    [SerializeField] private TextMeshProUGUI progressSummaryText;

    private GameManager gameManager;
    private ProgressTracker progressTracker;

    void Start()
    {
        gameManager = GameManager.Instance;
        progressTracker = ProgressTracker.Instance;

        SetupButtons();
        ShowModeSelection();
        UpdateProgressDisplay();
    }

    private void SetupButtons()
    {
        // Main mode buttons
        tutorialModeButton?.onClick.AddListener(ShowTutorialSelection);
        missionModeButton?.onClick.AddListener(StartMissionMode);
        backToMainMenuButton?.onClick.AddListener(ReturnToMainMenu);

        // Tutorial selection buttons
        walkTutorialButton?.onClick.AddListener(() => StartTutorial(JourneyType.Walk));
        trailTutorialButton?.onClick.AddListener(() => StartTutorial(JourneyType.Trail));
        pathTutorialButton?.onClick.AddListener(() => StartTutorial(JourneyType.Path));
        circuitTutorialButton?.onClick.AddListener(() => StartTutorial(JourneyType.Circuit));
        cycleTutorialButton?.onClick.AddListener(() => StartTutorial(JourneyType.Cycle));
        backToModeSelectButton?.onClick.AddListener(ShowModeSelection);
    }

    /// <summary>
    /// Show the main mode selection (Tutorial Mode vs Mission Mode)
    /// </summary>
    public void ShowModeSelection()
    {
        modeSelectionPanel?.SetActive(true);
        tutorialSelectionPanel?.SetActive(false);

        // Ensure no game systems are running
        DisableAllGameSystems();

        Debug.Log("[ModeSelectorUI] Showing mode selection");
    }

    /// <summary>
    /// Show tutorial type selection
    /// </summary>
    private void ShowTutorialSelection()
    {
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(true);

        UpdateTutorialButtons();

        Debug.Log("[ModeSelectorUI] Showing tutorial selection");
    }

    /// <summary>
    /// Start specific tutorial
    /// </summary>
    private void StartTutorial(JourneyType tutorialType)
    {
        Debug.Log($"[ModeSelectorUI] Starting {tutorialType} tutorial");

        // Hide all selection UI
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(false);

        // Start tutorial mode via GameManager
        gameManager?.LaunchSpecificTutorial(tutorialType);
    }

    /// <summary>
    /// Start mission mode
    /// </summary>
    private void StartMissionMode()
    {
        Debug.Log("[ModeSelectorUI] Starting mission mode");

        // Hide selection UI
        modeSelectionPanel?.SetActive(false);
        tutorialSelectionPanel?.SetActive(false);

        // Start mission mode via GameManager
        gameManager.SelectStoryMode();
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    private void ReturnToMainMenu()
    {
        Debug.Log("[ModeSelectorUI] Returning to main menu");
        gameManager?.ReturnToMainMenu();
    }

    /// <summary>
    /// Update tutorial buttons with completion status
    /// </summary>
    private void UpdateTutorialButtons()
    {
        if (progressTracker == null) return;

        UpdateTutorialButton(walkTutorialButton, JourneyType.Walk, "Walk Tutorial\nAny movement allowed");
        UpdateTutorialButton(trailTutorialButton, JourneyType.Trail, "Trail Tutorial\nNo repeated bridges");
        UpdateTutorialButton(pathTutorialButton, JourneyType.Path, "Path Tutorial\nNo repeated vertices");
        UpdateTutorialButton(circuitTutorialButton, JourneyType.Circuit, "Circuit Tutorial\nTrail that returns home");
        UpdateTutorialButton(cycleTutorialButton, JourneyType.Cycle, "Cycle Tutorial\nPath that returns home");
    }

    private void UpdateTutorialButton(Button button, JourneyType type, string description)
    {
        if (button == null) return;

        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            bool completed = progressTracker.IsTutorialCompleted(type);

            buttonText.text = description + (completed ? " ✓" : "");
            buttonText.color = completed ? Color.green : Color.white;
        }
    }

    /// <summary>
    /// Update progress summary display
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressSummaryText != null && progressTracker != null)
        {
            progressSummaryText.text = progressTracker.GetProgressSummary();
        }
    }

    /// <summary>
    /// Disable all game systems when showing selection UI
    /// </summary>
    private void DisableAllGameSystems()
    {
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        var missionManager = FindFirstObjectByType<LearningMissionsManager>();

        if (tutorialManager != null)
            tutorialManager.enabled = false;

        if (missionManager != null)
            missionManager.enabled = false;

        Debug.Log("[ModeSelectorUI] Disabled game systems for clean selection");
    }

    /// <summary>
    /// Called by other systems when they complete
    /// </summary>
    public void OnActivityCompleted()
    {
        ShowModeSelection();
        UpdateProgressDisplay();
    }

    // Keyboard shortcuts
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialSelectionPanel.activeInHierarchy)
                ShowModeSelection();
            else
                ReturnToMainMenu();
        }

        // Quick tutorial shortcuts (1-5 keys)
        if (tutorialSelectionPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) StartTutorial(JourneyType.Walk);
            if (Input.GetKeyDown(KeyCode.Alpha2)) StartTutorial(JourneyType.Trail);
            if (Input.GetKeyDown(KeyCode.Alpha3)) StartTutorial(JourneyType.Path);
            if (Input.GetKeyDown(KeyCode.Alpha4)) StartTutorial(JourneyType.Circuit);
            if (Input.GetKeyDown(KeyCode.Alpha5)) StartTutorial(JourneyType.Cycle);
        }
    }
}