// GameManager.cs - Enhanced for Scene Management
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Main game coordinator that manages scene transitions and game modes
/// Now works across MainMenu and Game scenes
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "MVP2";

    [Header("System References")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private LearningMissionsManager missionManager;
    [SerializeField] private JourneyTracker journeyTracker;

    [Header("UI References")]
    [SerializeField] private GameObject modeSelectionPanel;
    [SerializeField] private GameObject tutorialUIPanel;
    [SerializeField] private GameObject missionUIPanel;
    [SerializeField] private GameObject sharedUIPanel;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Current game state
    private GameMode currentMode = GameMode.None;
    private ProgressTracker progressTracker;
    private SceneTransition sceneTransition;

    // Static reference for cross-scene access
    public static GameManager Instance { get; private set; }

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
        // Singleton pattern for persistence across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize scene transition system
            if (GetComponent<SceneTransition>() == null)
            {
                gameObject.AddComponent<SceneTransition>();
            }
            sceneTransition = SceneTransition.Instance;

            LogDebug("GameManager initialized as persistent singleton");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get or create progress tracker
        InitializeProgressTracker();
    }
    // Add this to your GameManager.cs - replace the existing Start() method

    void Start()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Handle current scene
        HandleSceneInitialization(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called whenever a new scene loads
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogDebug($"Scene loaded: {scene.name}");
        HandleSceneInitialization(scene.name);
    }

    /// <summary>
    /// Handle scene-specific initialization
    /// </summary>
    private void HandleSceneInitialization(string sceneName)
    {
        LogDebug($"GameManager handling scene: {sceneName}");

        if (sceneName == gameSceneName)
        {
            // Wait one frame for scene objects to initialize
            StartCoroutine(InitializeGameSceneDelayed());
        }
        // MainMenu scene initialization handled by MainMenuManager
    }

    /// <summary>
    /// Initialize game scene with slight delay to ensure all objects are ready
    /// </summary>
    private IEnumerator InitializeGameSceneDelayed()
    {
        yield return null; // Wait one frame

        LogDebug("Initializing Game scene components");

        // Find scene-specific references
        if (tutorialManager == null)
            tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (missionManager == null)
            missionManager = FindFirstObjectByType<LearningMissionsManager>();
        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();

        // Disable systems initially to prevent conflicts
        DisableGameSystems();

        // Check for pending tutorial/mission requests
        if (PlayerPrefs.HasKey("TargetTutorialType"))
        {
            // Auto-start tutorial mode
            LogDebug("Auto-starting tutorial mode from main menu request");
            SelectTutorialMode();
        }
        else if (PlayerPrefs.HasKey("TargetMissionIndex"))
        {
            // Auto-start mission mode  
            LogDebug("Auto-starting mission mode from main menu request");
            SelectMissionMode();
        }
        else
        {
            // Show mode selection by default
            ShowModeSelection();
        }
    }

    void Update()
    {
        // Global keyboard shortcuts (work in any scene)
        HandleGlobalInput();
    }

    /// <summary>
    /// Initialize progress tracker (persistent across scenes)
    /// </summary>
    private void InitializeProgressTracker()
    {
        progressTracker = FindFirstObjectByType<ProgressTracker>();
        if (progressTracker == null)
        {
            var progressGO = new GameObject("ProgressTracker");
            //progressGO.transform.SetParent(transform); // Child of persistent GameManager
            progressTracker = progressGO.AddComponent<ProgressTracker>();
        }
    }

    /// <summary>
    /// Initialize game scene specific components
    /// </summary>
    private void InitializeGameScene()
    {
        LogDebug("Initializing Game scene components");

        // Find scene-specific references
        if (tutorialManager == null)
            tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (missionManager == null)
            missionManager = FindFirstObjectByType<LearningMissionsManager>();
        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();

        // Disable systems initially to prevent conflicts
        DisableGameSystems();

        // NEW: Check for pending tutorial/mission requests
        if (PlayerPrefs.HasKey("TargetTutorialType"))
        {
            // Auto-start tutorial mode
            LogDebug("Auto-starting tutorial mode from main menu request");
            SelectTutorialMode();
        }
        else if (PlayerPrefs.HasKey("TargetMissionIndex"))
        {
            // Auto-start mission mode  
            LogDebug("Auto-starting mission mode from main menu request");
            SelectMissionMode();
        }
        else
        {
            // Show mode selection by default
            ShowModeSelection();
        }
    }

    /// <summary>
    /// Handle global keyboard shortcuts
    /// </summary>
    private void HandleGlobalInput()
    {
        // Only process input if not transitioning
        if (SceneTransition.Instance != null && SceneTransition.Instance.isActiveAndEnabled)
        {
            // Main menu shortcuts
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMainMenu();
            }

            // Game mode shortcuts (only in game scene)
            if (SceneManager.GetActiveScene().name == gameSceneName)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    LogDebug("Keyboard shortcut: Tutorial mode");
                    SelectTutorialMode();
                }

                if (Input.GetKeyDown(KeyCode.M))
                {
                    LogDebug("Keyboard shortcut: Mission mode");
                    SelectMissionMode();
                }

                // Direct tutorial shortcuts
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LaunchSpecificTutorial(JourneyType.Walk);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LaunchSpecificTutorial(JourneyType.Trail);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    LaunchSpecificTutorial(JourneyType.Path);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    LaunchSpecificTutorial(JourneyType.Circuit);
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    LaunchSpecificTutorial(JourneyType.Cycle);
                }
            }
        }
    }

    #region Scene Transition Methods



    /// <summary>
    /// Return to main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        LogDebug("Returning to main menu");

        // Record current progress before leaving
        RecordCurrentProgress();

        // Reset current mode
        currentMode = GameMode.None;

        // Load main menu scene
        if (sceneTransition != null)
        {
            sceneTransition.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Return to mode selection (backward compatibility)
    /// If in game scene, shows mode selection; otherwise returns to main menu
    /// </summary>
    public void ReturnToModeSelection()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == gameSceneName)
        {
            // In game scene - show mode selection
            LogDebug("Returning to mode selection in game scene");
            ShowModeSelection();
        }
        else
        {
            // In other scene - return to main menu
            LogDebug("Returning to main menu from non-game scene");
            ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Load game scene and enter tutorial mode
    /// </summary>
    public void LoadGameForTutorial(JourneyType tutorialType = JourneyType.Walk)
    {
        LogDebug($"Loading game for tutorial: {tutorialType}");

        // Store the target tutorial type
        PlayerPrefs.SetInt("TargetTutorialType", (int)tutorialType);

        // Load game scene
        if (sceneTransition != null)
        {
            sceneTransition.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    /// <summary>
    /// Load game scene and enter mission mode
    /// </summary>
    public void LoadGameForMission(int missionIndex = 0)
    {
        LogDebug($"Loading game for mission: {missionIndex}");

        // Store the target mission
        PlayerPrefs.SetInt("TargetMissionIndex", missionIndex);
        PlayerPrefs.SetString("TargetMode", "Mission");

        // Load game scene
        if (sceneTransition != null)
        {
            sceneTransition.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    #endregion

    #region Game Mode Management (existing methods updated)

    /// <summary>
    /// Show mode selection in game scene
    /// </summary>
    public void ShowModeSelection()
    {
        // Only works in game scene
        if (SceneManager.GetActiveScene().name != gameSceneName) return;

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
        DisableGameSystems();

        LogDebug("Mode selection shown");
    }

    /// <summary>
    /// Select tutorial mode
    /// </summary>
    public void SelectTutorialMode()
    {
        // If not in game scene, load it first
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            LoadGameForTutorial();
            return;
        }

        LogDebug("Selecting tutorial mode");

        currentMode = GameMode.Tutorial;

        // UI Management
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(false);
        if (tutorialUIPanel != null)
            tutorialUIPanel.SetActive(true);
        if (sharedUIPanel != null)
            sharedUIPanel.SetActive(true);
        if (missionUIPanel != null)
            missionUIPanel.SetActive(false);

        // Enable tutorial system
        if (tutorialManager != null)
        {
            tutorialManager.enabled = true;

            // Check if specific tutorial was requested
            if (PlayerPrefs.HasKey("TargetTutorialType"))
            {
                JourneyType targetType = (JourneyType)PlayerPrefs.GetInt("TargetTutorialType");
                tutorialManager.ChangeTutorialType(targetType);
                PlayerPrefs.DeleteKey("TargetTutorialType");
            }
        }

        // Disable mission system
        if (missionManager != null)
            missionManager.enabled = false;

        // Reset journey for clean start
        if (journeyTracker != null)
            journeyTracker.ResetJourney();

        OnModeChanged?.Invoke(currentMode);
        OnGameplayStarted?.Invoke();
    }

    /// <summary>
    /// Select mission mode
    /// </summary>
    public void SelectMissionMode()
    {
        // If not in game scene, load it first
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            LoadGameForMission();
            return;
        }

        LogDebug("Selecting mission mode");

        currentMode = GameMode.Mission;

        // UI Management
        if (modeSelectionPanel != null)
            modeSelectionPanel.SetActive(false);
        if (missionUIPanel != null)
            missionUIPanel.SetActive(true);
        if (sharedUIPanel != null)
            sharedUIPanel.SetActive(true);
        if (tutorialUIPanel != null)
            tutorialUIPanel.SetActive(false);

        // Enable mission system
        if (missionManager != null)
        {
            missionManager.enabled = true;

            // Check if specific mission was requested
            if (PlayerPrefs.HasKey("TargetMissionIndex"))
            {
                int targetMission = PlayerPrefs.GetInt("TargetMissionIndex");
                missionManager.JumpToMission(targetMission);
                PlayerPrefs.DeleteKey("TargetMissionIndex");
                PlayerPrefs.DeleteKey("TargetMode");
            }
            else
            {
                missionManager.StartMission(0);
            }
        }

        // Disable tutorial system
        if (tutorialManager != null)
            tutorialManager.enabled = false;

        // Reset journey for clean start
        if (journeyTracker != null)
            journeyTracker.ResetJourney();

        OnModeChanged?.Invoke(currentMode);
        OnGameplayStarted?.Invoke();
    }

    /// <summary>
    /// Launch specific tutorial directly
    /// </summary>
    public void LaunchSpecificTutorial(JourneyType tutorialType)
    {
        LogDebug($"Launching specific tutorial: {tutorialType}");

        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            LoadGameForTutorial(tutorialType);
        }
        else
        {
            SelectTutorialMode();
            if (tutorialManager != null)
            {
                tutorialManager.ChangeTutorialType(tutorialType);
            }
        }
    }

    #endregion

    /// <summary>
    /// Disable game systems to prevent conflicts
    /// </summary>
    private void DisableGameSystems()
    {
        if (tutorialManager != null)
            tutorialManager.enabled = false;
        if (missionManager != null)
            missionManager.enabled = false;
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
                // Check if tutorial is complete using compatible method
                bool isComplete = false;
                try
                {
                    isComplete = tutorialManager.GetComponent<TutorialManager>().IsTutorialComplete;
                }
                catch
                {
                    // Property doesn't exist - skip recording
                    LogDebug("Cannot check tutorial completion - IsTutorialComplete property not found");
                    return;
                }

                if (isComplete)
                {
                    JourneyType tutorialType = JourneyType.Walk; // Default fallback
                    try
                    {
                        tutorialType = tutorialManager.GetComponent<TutorialManager>().CurrentTutorialType;
                    }
                    catch
                    {
                        LogDebug("Cannot get current tutorial type - using Walk as fallback");
                    }

                    progressTracker.RecordTutorialCompletion(tutorialType);
                    LogDebug($"Recorded tutorial completion: {tutorialType}");
                }
            }
            else if (currentMode == GameMode.Mission && missionManager != null)
            {
                // Use compatible method calls for mission manager
                int missionIndex = 0;
                string missionName = "Unknown Mission";

                try
                {
                    missionIndex = missionManager.GetCurrentMissionIndex();
                    missionName = missionManager.GetCurrentMissionName();
                }
                catch
                {
                    LogDebug("Cannot get mission info - using defaults");
                }

                progressTracker.RecordMissionProgress(missionIndex, missionName);
                LogDebug($"Recorded mission progress: {missionIndex} - {missionName}");
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error recording progress: {ex.Message}");
        }
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

    private void LogError(string message)
    {
        Debug.LogError($"[GameManager] {message}");
    }
}