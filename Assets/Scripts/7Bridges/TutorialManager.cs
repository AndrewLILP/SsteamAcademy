// TutorialManager.cs
// Main orchestration class for tutorial system using Facade pattern

using UnityEngine;
using System;

/// <summary>
/// Central coordinator for tutorial system
/// Implements Facade pattern to provide simple interface to complex tutorial subsystems
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

    void Start()
    {
        InitializeSystems();
        CreateAndStartTutorial();
    }

    void Update()
    {
        HandleInput();
        currentTutorial?.CheckProgress();
    }

    /// <summary>
    /// Initialize required system dependencies and validate setup
    /// </summary>
    private void InitializeSystems()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        uiManager = GetComponent<TutorialUIManager>();

        if (journeyTracker == null)
        {
            Debug.LogError("TutorialManager requires a JourneyTracker in the scene!");
            enabled = false;
            return;
        }

        if (uiManager == null)
        {
            Debug.LogError("TutorialManager requires a TutorialUIManager component!");
            enabled = false;
            return;
        }

        LogDebug($"Tutorial systems initialized for {tutorialType} tutorial");
    }

    /// <summary>
    /// Create and initialize the appropriate tutorial using factory pattern
    /// </summary>
    private void CreateAndStartTutorial()
    {
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

        // Set initial UI
        var config = TutorialConfig.GetConfig(tutorialType);
        uiManager.SetInstructionMessage(config.instructionMessage);
        uiManager.UpdateProgressMessage(config.progressMessages[0]);
        uiManager.UpdateStatusMessage($"Creating {tutorialType}: 0/{config.requiredSteps} steps");

        LogDebug($"{config.tutorialName} tutorial created and initialized");
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
    /// </summary>
    public void ResetTutorial()
    {
        LogDebug("Resetting tutorial");
        currentTutorial?.Reset();

        // Reset UI
        var config = TutorialConfig.GetConfig(tutorialType);
        uiManager.ResetUI();
        uiManager.UpdateProgressMessage(config.progressMessages[0]);
        uiManager.UpdateStatusMessage($"Creating {tutorialType}: 0/{config.requiredSteps} steps");
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
            RestartTutorial();
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

    #endregion
}