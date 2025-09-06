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
        LogDebug("GameManager Awake - Starting immediate system disable");

        // CRITICAL: Disable systems immediately before they can start
        DisableGameSystemsImmediate();

        // Get or create progress tracker
        progressTracker = FindFirstObjectByType<ProgressTracker>();
        if (progressTracker == null)
        {
            var progressGO = new GameObject("ProgressTracker");
            progressTracker = progressGO.AddComponent<ProgressTracker>();
        }

        ValidateReferences();
        LogDebug("GameManager Awake complete");
    }

    /// <summary>
    /// Immediately disable all game systems to prevent startup conflicts
    /// </summary>
    private void DisableGameSystemsImmediate()
    {
        // Method 1: Disable components immediately
        var allTutorialManagers = FindObjectsByType<TutorialManager>(FindObjectsSortMode.None);
        foreach (var tm in allTutorialManagers)
        {
            tm.enabled = false;
            LogDebug($"TutorialManager component disabled immediately");
        }

        var allMissionManagers = FindObjectsByType<LearningMissionsManager>(FindObjectsSortMode.None);
        foreach (var mm in allMissionManagers)
        {
            mm.enabled = false;
            LogDebug($"LearningMissionsManager component disabled immediately");
        }

        // Method 2: Also disable GameObjects to be absolutely sure
        var tutorialGO = GameObject.Find("TutorialManager");
        if (tutorialGO != null)
        {
            tutorialGO.SetActive(false);
            LogDebug("TutorialManager GameObject disabled");
        }

        // Store references for later re-enabling
        tutorialManager = allTutorialManagers.Length > 0 ? allTutorialManagers[0] : null;
        missionManager = allMissionManagers.Length > 0 ? allMissionManagers[0] : null;
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
        Debug.Log("[GameManager] SelectTutorialMode() called");

        if (currentMode == GameMode.Tutorial)
        {
            Debug.LogWarning("[GameManager] Already in tutorial mode");
            return;
        }

        try
        {
            currentMode = GameMode.Tutorial;

            // Hide selection panel
            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(false);
                Debug.Log("[GameManager] Mode selection panel hidden");
            }

            // Show tutorial UI
            if (tutorialUIPanel != null)
            {
                tutorialUIPanel.SetActive(true);
                Debug.Log("[GameManager] Tutorial UI panel shown");
            }
            if (sharedUIPanel != null)
            {
                sharedUIPanel.SetActive(true);
                Debug.Log("[GameManager] Shared UI panel shown");
            }

            // Hide mission UI
            if (missionUIPanel != null)
            {
                missionUIPanel.SetActive(false);
                Debug.Log("[GameManager] Mission UI panel hidden");
            }

            // Enable tutorial system
            if (tutorialManager != null)
            {
                tutorialManager.enabled = true;
                Debug.Log("[GameManager] Tutorial system enabled");

                // Initialize tutorial system
                tutorialManager.InitializeForGameManager();
            }
            else
            {
                Debug.LogError("[GameManager] TutorialManager not found!");
            }

            // Disable mission system
            if (missionManager != null)
            {
                missionManager.enabled = false;
                Debug.Log("[GameManager] Mission system disabled");
            }

            // Reset journey for clean start
            if (journeyTracker != null)
            {
                journeyTracker.ResetJourney();
                Debug.Log("[GameManager] Journey tracker reset");
            }

            // Notify other systems
            OnModeChanged?.Invoke(currentMode);
            OnGameplayStarted?.Invoke();

            Debug.Log("[GameManager] Tutorial mode activated successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error in SelectTutorialMode: {e.Message}");
        }
    }

    public void SelectMissionMode()
    {
        Debug.Log("[GameManager] SelectMissionMode() called");

        if (currentMode == GameMode.Mission)
        {
            Debug.LogWarning("[GameManager] Already in mission mode");
            return;
        }

        try
        {
            currentMode = GameMode.Mission;

            // Hide selection panel
            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(false);
                Debug.Log("[GameManager] Mode selection panel hidden");
            }

            // Show mission UI
            if (missionUIPanel != null)
            {
                missionUIPanel.SetActive(true);
                Debug.Log("[GameManager] Mission UI panel shown");
            }
            if (sharedUIPanel != null)
            {
                sharedUIPanel.SetActive(true);
                Debug.Log("[GameManager] Shared UI panel shown");
            }

            // Hide tutorial UI
            if (tutorialUIPanel != null)
            {
                tutorialUIPanel.SetActive(false);
                Debug.Log("[GameManager] Tutorial UI panel hidden");
            }

            // Enable mission system
            if (missionManager != null)
            {
                missionManager.enabled = true;
                Debug.Log("[GameManager] Mission system enabled");

                // Start first mission
                missionManager.StartMission(0);
            }
            else
            {
                Debug.LogError("[GameManager] LearningMissionsManager not found!");
            }

            // Disable tutorial system
            if (tutorialManager != null)
            {
                tutorialManager.enabled = false;
                Debug.Log("[GameManager] Tutorial system disabled");
            }

            // Reset journey for clean start
            if (journeyTracker != null)
            {
                journeyTracker.ResetJourney();
                Debug.Log("[GameManager] Journey tracker reset");
            }

            // Notify other systems
            OnModeChanged?.Invoke(currentMode);
            OnGameplayStarted?.Invoke();

            Debug.Log("[GameManager] Mission mode activated successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error in SelectMissionMode: {e.Message}");
        }
    }

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