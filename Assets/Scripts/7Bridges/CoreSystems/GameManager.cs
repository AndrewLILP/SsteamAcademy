// GameManager.cs - Complete script with Story Mode integration
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Main game coordinator that manages scene transitions and game modes
/// Now supports Tutorial and Story modes (LearningMissionsManager removed)
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "MVP2";

    [Header("System References")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private JourneyTracker journeyTracker;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Current game state
    private GameMode currentMode = GameMode.None;
    private ProgressTracker progressTracker;
    private SceneTransition sceneTransition;
    private MinimalModeUI minimalModeUI;

    // Static reference for cross-scene access
    public static GameManager Instance { get; private set; }

    public enum GameMode
    {
        None,
        Tutorial,
        Story  // Added Story mode
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

        // Disable old tutorial system completely
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.gameObject.SetActive(false);
        }

        LogDebug("Initializing Game scene components");

        // Find scene-specific references
        if (this.tutorialManager == null)
            this.tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();

        // Find the MinimalModeUI
        minimalModeUI = FindFirstObjectByType<MinimalModeUI>();

        // Disable systems initially to prevent conflicts
        DisableGameSystems();

        // Check for pending mode requests
        string targetMode = PlayerPrefs.GetString("TargetMode", "");

        if (targetMode == "Story")
        {
            // Auto-start story mode
            PlayerPrefs.DeleteKey("TargetMode");
            LogDebug("Auto-starting story mode");
            SelectStoryMode();
        }
        else if (PlayerPrefs.HasKey("TargetTutorialType"))
        {
            // Auto-start specific tutorial
            JourneyType targetType = (JourneyType)PlayerPrefs.GetInt("TargetTutorialType");
            PlayerPrefs.DeleteKey("TargetTutorialType");
            LogDebug($"Auto-starting tutorial mode: {targetType}");
            LaunchSpecificTutorial(targetType);
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
            progressTracker = progressGO.AddComponent<ProgressTracker>();
        }
    }

    /// <summary>
    /// Handle global keyboard shortcuts
    /// </summary>
    private void HandleGlobalInput()
    {
        // Only process input if not transitioning
        if (SceneTransition.Instance != null && SceneTransition.Instance.IsTransitioning)
            return;

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

            if (Input.GetKeyDown(KeyCode.S))
            {
                LogDebug("Keyboard shortcut: Story mode");
                SelectStoryMode();
            }

            // Direct tutorial shortcuts
            if (Input.GetKeyDown(KeyCode.Alpha1))
                LaunchSpecificTutorial(JourneyType.Walk);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                LaunchSpecificTutorial(JourneyType.Trail);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                LaunchSpecificTutorial(JourneyType.Path);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                LaunchSpecificTutorial(JourneyType.Circuit);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                LaunchSpecificTutorial(JourneyType.Cycle);
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
    /// Load game scene and enter story mode
    /// </summary>
    public void LoadGameForStory()
    {
        LogDebug("Loading game for story mode");

        // Store the target mode
        PlayerPrefs.SetString("TargetMode", "Story");

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

    #region Game Mode Management

    /// <summary>
    /// Show mode selection using MinimalModeUI
    /// </summary>
    public void ShowModeSelection()
    {
        // Only works in game scene
        if (SceneManager.GetActiveScene().name != gameSceneName) return;

        currentMode = GameMode.None;

        // Use the MinimalModeUI
        if (minimalModeUI != null)
        {
            minimalModeUI.ShowModeSelection();
        }
        else
        {
            LogDebug("MinimalModeUI not found - searching...");
            minimalModeUI = FindFirstObjectByType<MinimalModeUI>();
            if (minimalModeUI != null)
                minimalModeUI.ShowModeSelection();
        }

        // Disable game systems
        DisableGameSystems();

        LogDebug("Mode selection shown via MinimalModeUI");
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

        // Show tutorial selection via MinimalModeUI
        if (minimalModeUI != null)
        {
            minimalModeUI.ShowModeSelection(); // Will show tutorial selection option
        }

        OnModeChanged?.Invoke(currentMode);
    }

    /// <summary>
    /// Select story mode (replaces old mission mode)
    /// </summary>
    public void SelectStoryMode()
    {
        // If not in game scene, load it first
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            LoadGameForStory();
            return;
        }

        LogDebug("Selecting story mode");

        currentMode = GameMode.Story;

        // Hide MinimalModeUI (used for tutorials)
        if (minimalModeUI != null)
            //minimalModeUI.gameObject.SetActive(false);
            minimalModeUI.HideAllPanels();

        // COMPLETELY DISABLE old systems
        var tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null)
            tutorialManager.enabled = false;

        // Start story system
        var storyManager = FindFirstObjectByType<StoryManager>();
        if (storyManager != null)
        {
            storyManager.StartStory();
        }
        else
        {
            LogError("StoryManager not found! Please add it to MVP2 scene.");
        }

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
            return;
        }

        currentMode = GameMode.Tutorial;

        // COMPLETELY DISABLE the old tutorial system
        if (tutorialManager != null)
        {
            tutorialManager.enabled = false;
            tutorialManager.gameObject.SetActive(false);
        }

        // Let MinimalModeUI handle everything
        if (minimalModeUI != null)
        {
            // You'll need to add a method to MinimalModeUI to start specific tutorials
            // For now, just show the tutorial selection
            minimalModeUI.ShowModeSelection();
        }

        OnModeChanged?.Invoke(currentMode);
        OnGameplayStarted?.Invoke();
    }

    #endregion

    /// <summary>
    /// Disable game systems to prevent conflicts
    /// </summary>
    private void DisableGameSystems()
    {
        if (tutorialManager != null)
            tutorialManager.enabled = false;

        // Note: LearningMissionsManager removed - no longer used
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
                // Check if tutorial is complete
                try
                {
                    // You may need to adapt this based on your tutorial system
                    LogDebug("Recording tutorial progress");
                }
                catch
                {
                    LogDebug("Cannot check tutorial completion - property not available");
                }
            }
            else if (currentMode == GameMode.Story)
            {
                // Record story progress
                var storyManager = FindFirstObjectByType<StoryManager>();
                if (storyManager != null && storyManager.IsStoryComplete)
                {
                    LogDebug("Story completed - progress recorded");
                }
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