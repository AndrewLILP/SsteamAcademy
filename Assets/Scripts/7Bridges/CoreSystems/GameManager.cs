// GameManager.cs - Updated for Resilient TutorialManager
using UnityEngine;

/// <summary>
/// Main game coordinator that manages Tutorial vs Mission modes
/// Updated to work with resilient TutorialManager
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private LearningMissionsManager missionManager;
    [SerializeField] private JourneyTracker journeyTracker;

    [Header("UI References")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private GameObject tutorialUIPanel;
    [SerializeField] private GameObject missionUIPanel;
    [SerializeField] private GameObject sharedUIPanel; // Back button, journey display, etc.

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Current game state
    private GameMode currentMode = GameMode.None;
    private ProgressTracker progressTracker;

    public enum GameMode
    {
        None,
        Tutorial,
        Mission
    }

    // Events for other systems to listen to
    public System.Action<GameMode> OnModeChanged;
    public System.Action OnGameplayStarted;

    void Awake()
    {
        // Get or create progress tracker
        progressTracker = FindFirstObjectByType<ProgressTracker>();
        if (progressTracker == null)
        {
            var progressGO = new GameObject("ProgressTracker");
            progressTracker = progressGO.AddComponent<ProgressTracker>();
        }

        ValidateReferences();

        // IMPORTANT: Disable systems early to prevent initialization conflicts
        DisableGameSystems();
    }

    void Start()
    {
        // Show initial mode selection
        ShowModeSelection();
        LogDebug("GameManager started - showing mode selection");
    }

    /// <summary>
    /// Disable game systems to prevent startup conflicts
    /// </summary>
    private void DisableGameSystems()
    {
        if (tutorialManager != null)
        {
            tutorialManager.enabled = false;
            LogDebug("TutorialManager disabled at startup");
        }

        if (missionManager != null)
        {
            missionManager.enabled = false;
            LogDebug("LearningMissionsManager disabled at startup");
        }
    }

    /// <summary>
    /// Show the initial mode selection panel
    /// </summary>
    public void ShowModeSelection()
    {
        currentMode = GameMode.None;

        // Show selection panel
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(true);

        // Hide game UI panels
        if (tutorialUIPanel != null)
            tutorialUIPanel.SetActive(false);
        if (missionUIPanel != null)
            missionUIPanel.SetActive(false);
        if (sharedUIPanel != null)
            sharedUIPanel.SetActive(false);

        // Disable game systems
        if (tutorialManager != null)
            tutorialManager.enabled = false;
        if (missionManager != null)
            missionManager.enabled = false;

        LogDebug("Mode selection shown, game systems disabled");
    }

    /// <summary>
    /// User selected Tutorial Mode
    /// </summary>
    public void SelectTutorialMode()
    {
        LogDebug("Tutorial mode selected");

        currentMode = GameMode.Tutorial;

        // Hide selection panel
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(false);

        // Show tutorial UI
        if (tutorialUIPanel != null)
            tutorialUIPanel.SetActive(true);
        if (sharedUIPanel != null)
            sharedUIPanel.SetActive(true);

        // Hide mission UI
        if (missionUIPanel != null)
            missionUIPanel.SetActive(false);

        // Enable and initialize tutorial system safely
        if (tutorialManager != null)
        {
            tutorialManager.enabled = true;

            // Use new initialization method for clean startup
            try
            {
                tutorialManager.InitializeForGameManager();
                tutorialManager.ResetTutorial();
                LogDebug("Tutorial system initialized successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Error initializing tutorial system: {ex.Message}");
                LogError("Tutorial mode may have limited functionality");
            }
        }
        else
        {
            LogError("TutorialManager not found - tutorial mode unavailable");
        }

        // Disable mission system
        if (missionManager != null)
            missionManager.enabled = false;

        // Reset journey for clean start
        if (journeyTracker != null)
            journeyTracker.ResetJourney();

        // Notify other systems
        OnModeChanged?.Invoke(currentMode);
        OnGameplayStarted?.Invoke();

        LogDebug("Tutorial mode activated");
    }

    /// <summary>
    /// User selected Mission Mode
    /// </summary>
    public void SelectMissionMode()
    {
        LogDebug("Mission mode selected");

        currentMode = GameMode.Mission;

        // Hide selection panel
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(false);

        // Show mission UI
        if (missionUIPanel != null)
            missionUIPanel.SetActive(true);
        if (sharedUIPanel != null)
            sharedUIPanel.SetActive(true);

        // Hide tutorial UI
        if (tutorialUIPanel != null)
            tutorialUIPanel.SetActive(false);

        // Enable mission system
        if (missionManager != null)
        {
            missionManager.enabled = true;

            try
            {
                // Start first mission
                missionManager.StartMission(0);
                LogDebug("Mission system started successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Error starting mission system: {ex.Message}");
                LogError("Mission mode may have limited functionality");
            }
        }
        else
        {
            LogError("LearningMissionsManager not found - mission mode unavailable");
        }

        // Disable tutorial system
        if (tutorialManager != null)
            tutorialManager.enabled = false;

        // Reset journey for clean start
        if (journeyTracker != null)
            journeyTracker.ResetJourney();

        // Notify other systems
        OnModeChanged?.Invoke(currentMode);
        OnGameplayStarted?.Invoke();

        LogDebug("Mission mode activated");
    }

    /// <summary>
    /// Return to mode selection (called by Back button)
    /// </summary>
    public void ReturnToModeSelection()
    {
        LogDebug("Returning to mode selection");

        // Record completion if applicable
        RecordCurrentProgress();

        ShowModeSelection();
    }

    /// <summary>
    /// Record progress for current mode
    /// </summary>
    private void RecordCurrentProgress()
    {
        if (progressTracker == null) return;

        try
        {
            if (currentMode == GameMode.Tutorial && tutorialManager != null)
            {
                // Check if current tutorial is complete
                if (tutorialManager.IsTutorialComplete)
                {
                    var tutorialType = tutorialManager.CurrentTutorialType;
                    progressTracker.RecordTutorialCompletion(tutorialType);
                    LogDebug($"Recorded tutorial completion: {tutorialType}");
                }
            }
            else if (currentMode == GameMode.Mission && missionManager != null)
            {
                // Record current mission progress
                var missionIndex = missionManager.GetCurrentMissionIndex();
                var missionName = missionManager.GetCurrentMissionName();

                // Check if current mission is complete (you may need to add this method)
                // For now, we'll record the highest mission reached
                progressTracker.RecordMissionProgress(missionIndex, missionName);
                LogDebug($"Recorded mission progress: {missionIndex} - {missionName}");
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error recording progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate all required references are assigned
    /// </summary>
    private void ValidateReferences()
    {
        if (modeSelectionPanel == null)
            LogError("Mode Selection Panel not assigned!");

        if (tutorialManager == null)
            tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager == null)
            LogWarning("TutorialManager not found in scene");

        if (missionManager == null)
            missionManager = FindFirstObjectByType<LearningMissionsManager>();
        if (missionManager == null)
            LogWarning("LearningMissionsManager not found in scene");

        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();
        if (journeyTracker == null)
            LogWarning("JourneyTracker not found in scene");
    }

    // Public getters
    public GameMode CurrentMode => currentMode;
    public bool IsGameplayActive => currentMode != GameMode.None;
    public ProgressTracker GetProgressTracker() => progressTracker;

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[GameManager] {message}");
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogging)
            Debug.LogWarning($"[GameManager] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[GameManager] {message}");
    }

    // Context menu for testing
    [ContextMenu("Show Mode Selection")]
    public void TestShowModeSelection()
    {
        ShowModeSelection();
    }

    [ContextMenu("Select Tutorial Mode")]
    public void TestSelectTutorialMode()
    {
        SelectTutorialMode();
    }

    [ContextMenu("Select Mission Mode")]
    public void TestSelectMissionMode()
    {
        SelectMissionMode();
    }
}