// TutorialManager.cs - Resilient Version
// Main orchestration class for tutorial system using Facade pattern

using UnityEngine;
using System;

/// <summary>
/// Central coordinator for tutorial system
/// Implements Facade pattern to provide simple interface to complex tutorial subsystems
/// RESILIENT VERSION - Won't crash if UI components are missing
/// </summary>
public class TutorialManager : MonoBehaviour, ITutorialExitHandler
{
    [Header("Tutorial Selection")]
    [SerializeField] private JourneyType tutorialType = JourneyType.Walk;

    [Header("Input Controls")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;

    [Header("Debug Options")]
    [SerializeField] private bool enableDebugLogging = true;

    private ITutorial currentTutorial;
    private JourneyTracker journeyTracker;
    private TutorialUIManager uiManager;
    private bool isInitialized = false;

    void Start()
    {
        // Only initialize if enabled - GameManager will enable when needed
        if (!enabled)
        {
            LogDebug("TutorialManager disabled - skipping initialization");
            return;
        }

        InitializeSystems();
        if (isInitialized)
        {
            CreateAndStartTutorial();
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        HandleInput();
        currentTutorial?.CheckProgress();
    }

    /// <summary>
    /// Initialize required system dependencies and validate setup
    /// RESILIENT - Won't crash if components are missing
    /// </summary>
    private void InitializeSystems()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        uiManager = GetComponent<TutorialUIManager>();

        if (journeyTracker == null)
        {
            LogError("TutorialManager requires a JourneyTracker in the scene!");
            LogError("Tutorial system will be disabled until JourneyTracker is available");
            enabled = false;
            return;
        }

        if (uiManager == null)
        {
            LogWarning("TutorialUIManager component not found - UI features will be limited");
            LogWarning("Add TutorialUIManager component for full UI functionality");
            // Don't disable - continue with limited functionality
        }

        isInitialized = true;
        LogDebug($"Tutorial systems initialized for {tutorialType} tutorial (UI: {uiManager != null})");
    }

    /// <summary>
    /// Create and initialize the appropriate tutorial using factory pattern
    /// RESILIENT - Handles missing UI gracefully
    /// </summary>
    private void CreateAndStartTutorial()
    {
        if (!isInitialized)
        {
            LogWarning("Cannot create tutorial - systems not initialized");
            return;
        }

        // Remove existing tutorial component if present
        var existingTutorial = GetComponent<ITutorial>();
        if (existingTutorial is MonoBehaviour mb)
        {
            LogDebug("Destroying existing tutorial component");
            Destroy(mb);
        }

        // Add appropriate tutorial component using factory
        BaseTutorial tutorial = TutorialFactory.CreateTutorial(tutorialType, gameObject);

        currentTutorial = tutorial;
        currentTutorial.Initialize(journeyTracker);

        // Set initial UI - but handle missing UI manager gracefully
        var config = TutorialConfig.GetConfig(tutorialType);

        if (uiManager != null)
        {
            uiManager.SetInstructionMessage(config.instructionMessage);
            uiManager.UpdateProgressMessage(config.progressMessages[0]);
            uiManager.UpdateStatusMessage($"Creating {tutorialType}: 0/{config.requiredSteps} steps");
            LogDebug($"{config.tutorialName} tutorial created with full UI support");
        }
        else
        {
            LogDebug($"{config.tutorialName} tutorial created with limited UI (console logging only)");
        }
    }

    /// <summary>
    /// Handle keyboard input for reset and exit actions
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(resetKey))
        {
            ResetTutorial();
        }

        if (Input.GetKeyDown(exitKey))
        {
            HandleExit();
        }
    }

    /// <summary>
    /// Reset current tutorial to initial state
    /// RESILIENT - Handles missing components gracefully
    /// </summary>
    public void ResetTutorial()
    {
        LogDebug("Resetting tutorial");

        // Reset tutorial if it exists
        if (currentTutorial != null)
        {
            currentTutorial.Reset();
        }
        else
        {
            LogDebug("No current tutorial to reset - will be created when tutorial starts");
        }

        // Reset UI with null checks
        if (uiManager != null)
        {
            var config = TutorialConfig.GetConfig(tutorialType);
            uiManager.ResetUI();
            uiManager.UpdateProgressMessage(config.progressMessages[0]);
            uiManager.UpdateStatusMessage($"Creating {tutorialType}: 0/{config.requiredSteps} steps");
        }
        else
        {
            LogDebug("UI Manager not available - UI reset skipped");
        }
    }

    /// <summary>
    /// Handle tutorial exit request
    /// </summary>
    public void HandleExit()
    {
        LogDebug($"Exiting {currentTutorial?.TutorialName} tutorial");

        // Add your scene transition logic here
        // Examples:
        // SceneManager.LoadScene("MainMenu");
        // SceneManager.LoadScene("Scene212Selection");
        // GetComponent<SceneTransition>()?.LoadNextScene();

        // For now, just log the exit attempt
        Debug.Log("Tutorial exit requested - implement scene transition here");
    }

    // UI Button Methods
    public void OnResetButtonClicked()
    {
        LogDebug("Reset button clicked");
        ResetTutorial();
    }

    public void OnExitButtonClicked()
    {
        LogDebug("Exit button clicked");
        HandleExit();
    }

    /// <summary>
    /// Context menu method to change tutorial type at runtime in editor
    /// </summary>
    [ContextMenu("Restart with New Tutorial Type")]
    public void RestartTutorial()
    {
        if (!isInitialized)
        {
            LogWarning("Cannot restart tutorial - systems not initialized");
            return;
        }

        LogDebug($"Restarting tutorial with type: {tutorialType}");
        CreateAndStartTutorial();
    }

    /// <summary>
    /// Programmatically change tutorial type and restart
    /// </summary>
    public void ChangeTutorialType(JourneyType newType)
    {
        if (tutorialType != newType)
        {
            tutorialType = newType;
            LogDebug($"Tutorial type changed to: {newType}");
            if (isInitialized)
            {
                RestartTutorial();
            }
        }
    }

    /// <summary>
    /// Public method for GameManager to ensure clean startup
    /// </summary>
    public void InitializeForGameManager()
    {
        if (!isInitialized)
        {
            InitializeSystems();
        }

        if (isInitialized && currentTutorial == null)
        {
            CreateAndStartTutorial();
        }
    }

    /// <summary>
    /// Debug logging with enable/disable toggle
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[TutorialManager] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogging)
        {
            Debug.LogWarning($"[TutorialManager] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[TutorialManager] {message}");
    }

    #region Public Properties

    /// <summary>
    /// Current tutorial type being run
    /// </summary>
    public JourneyType CurrentTutorialType => tutorialType;

    /// <summary>
    /// Whether the current tutorial is completed
    /// </summary>
    public bool IsTutorialComplete => currentTutorial?.IsComplete ?? false;

    /// <summary>
    /// Name of the current tutorial
    /// </summary>
    public string CurrentTutorialName => currentTutorial?.TutorialName ?? "None";

    /// <summary>
    /// Whether the tutorial system is properly initialized
    /// </summary>
    public bool IsInitialized => isInitialized;

    #endregion
}