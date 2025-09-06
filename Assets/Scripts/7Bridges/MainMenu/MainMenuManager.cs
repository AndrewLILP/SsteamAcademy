// MainMenuManager.cs - Simple Main Menu Dashboard
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the main menu dashboard with simple tutorial/mission launch
/// Functional design - add visuals later
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI progressSummaryText;

    [Header("Tutorial Section")]
    [SerializeField] private Button tutorialModeButton;
    [SerializeField] private TextMeshProUGUI tutorialProgressText;
    [SerializeField] private Button[] tutorialDirectButtons; // 5 buttons for direct tutorial access

    [Header("Mission Section")]
    [SerializeField] private Button missionModeButton;
    [SerializeField] private TextMeshProUGUI missionProgressText;
    [SerializeField] private Button[] missionDirectButtons; // 5 buttons for direct mission access

    [Header("Settings")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool enableKeyboardShortcuts = true;

    private GameManager gameManager;
    private ProgressTracker progressTracker;

    void Start()
    {
        // Find references
        gameManager = GameManager.Instance;
        progressTracker = ProgressTracker.Instance;

        if (gameManager == null)
        {
            LogError("GameManager instance not found! Make sure GameManager exists in scene.");
            return;
        }

        // Setup UI
        SetupButtons();
        SetupText();
        UpdateProgressDisplay();

        // Listen for progress updates
        if (progressTracker != null)
        {
            progressTracker.OnProgressUpdated += UpdateProgressDisplay;
        }

        LogDebug("MainMenu initialized");
    }

    void Update()
    {
        if (enableKeyboardShortcuts)
        {
            HandleKeyboardShortcuts();
        }
    }

    void OnDestroy()
    {
        // Cleanup listeners
        if (progressTracker != null)
        {
            progressTracker.OnProgressUpdated -= UpdateProgressDisplay;
        }
    }

    /// <summary>
    /// Setup all button click listeners
    /// </summary>
    private void SetupButtons()
    {
        // Main mode buttons
        if (tutorialModeButton != null)
        {
            tutorialModeButton.onClick.RemoveAllListeners();
            tutorialModeButton.onClick.AddListener(() => {
                LogDebug("Tutorial Mode button clicked");
                gameManager?.LoadGameForTutorial();
            });
        }

        if (missionModeButton != null)
        {
            missionModeButton.onClick.RemoveAllListeners();
            missionModeButton.onClick.AddListener(() => {
                LogDebug("Mission Mode button clicked");
                gameManager?.LoadGameForMission();
            });
        }

        // Direct tutorial buttons (Walk, Trail, Path, Circuit, Cycle)
        SetupTutorialDirectButtons();

        // Direct mission buttons (Mission 1-5)
        SetupMissionDirectButtons();

        // Settings and exit
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(ShowSettings);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    /// <summary>
    /// Setup direct tutorial launch buttons
    /// </summary>
    private void SetupTutorialDirectButtons()
    {
        if (tutorialDirectButtons == null) return;

        JourneyType[] tutorialTypes = { JourneyType.Walk, JourneyType.Trail, JourneyType.Path, JourneyType.Circuit, JourneyType.Cycle };

        for (int i = 0; i < tutorialDirectButtons.Length && i < tutorialTypes.Length; i++)
        {
            if (tutorialDirectButtons[i] != null)
            {
                int buttonIndex = i; // Capture for closure
                JourneyType tutorialType = tutorialTypes[i];

                tutorialDirectButtons[i].onClick.RemoveAllListeners();
                tutorialDirectButtons[i].onClick.AddListener(() => {
                    LogDebug($"Direct tutorial button clicked: {tutorialType}");
                    gameManager?.LoadGameForTutorial(tutorialType);
                });

                // Update button text
                var buttonText = tutorialDirectButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"{tutorialType}";

                    // Color coding for completion status
                    if (progressTracker != null && progressTracker.IsTutorialCompleted(tutorialType))
                    {
                        buttonText.color = Color.green;
                        buttonText.text += " ✓";
                    }
                    else
                    {
                        buttonText.color = Color.white;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Setup direct mission launch buttons
    /// </summary>
    private void SetupMissionDirectButtons()
    {
        if (missionDirectButtons == null) return;

        string[] missionNames = { "Free Exploration", "Bridge Management", "Vertex Efficiency", "Closed Trails", "Perfect Loops" };

        for (int i = 0; i < missionDirectButtons.Length && i < missionNames.Length; i++)
        {
            if (missionDirectButtons[i] != null)
            {
                int missionIndex = i; // Capture for closure

                missionDirectButtons[i].onClick.RemoveAllListeners();
                missionDirectButtons[i].onClick.AddListener(() => {
                    LogDebug($"Direct mission button clicked: Mission {missionIndex + 1}");
                    gameManager?.LoadGameForMission(missionIndex);
                });

                // Update button text
                var buttonText = missionDirectButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"M{i + 1}: {missionNames[i]}";

                    // Color coding for completion status
                    if (progressTracker != null && progressTracker.IsMissionCompleted(i))
                    {
                        buttonText.color = Color.green;
                        buttonText.text += " ✓";
                    }
                    else
                    {
                        buttonText.color = Color.white;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Setup static text elements
    /// </summary>
    private void SetupText()
    {
        if (titleText != null)
        {
            titleText.text = "7 Bridges\nGraph Theory Explorer";
        }
    }

    /// <summary>
    /// Update progress display
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressTracker == null) return;

        // Overall progress
        if (progressSummaryText != null)
        {
            progressSummaryText.text = progressTracker.GetProgressSummary();
        }

        // Tutorial progress
        if (tutorialProgressText != null)
        {
            var completedTutorials = progressTracker.GetCompletedTutorials();
            var totalTutorials = progressTracker.GetTotalTutorialCount();

            tutorialProgressText.text = $"Tutorials: {completedTutorials.Count}/{totalTutorials}\n";

            if (completedTutorials.Count > 0)
            {
                tutorialProgressText.text += "Completed: " + string.Join(", ", completedTutorials);
            }
            else
            {
                tutorialProgressText.text += "None completed yet";
            }
        }

        // Mission progress
        if (missionProgressText != null)
        {
            var completedMissions = progressTracker.GetCompletedMissions();
            var missionCount = progressTracker.GetMissionCompletionCount();

            missionProgressText.text = $"Missions: {missionCount}/5\n";

            if (completedMissions.Count > 0)
            {
                missionProgressText.text += "Highest: Mission " + (progressTracker.GetHighestCompletedMission() + 1);
            }
            else
            {
                missionProgressText.text += "None completed yet";
            }
        }

        // Update button colors
        SetupTutorialDirectButtons();
        SetupMissionDirectButtons();
    }

    /// <summary>
    /// Handle keyboard shortcuts
    /// </summary>
    private void HandleKeyboardShortcuts()
    {
        // Tutorial shortcuts
        if (Input.GetKeyDown(KeyCode.T))
        {
            LogDebug("Keyboard: Tutorial mode");
            gameManager?.LoadGameForTutorial();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            LogDebug("Keyboard: Mission mode");
            gameManager?.LoadGameForMission();
        }

        // Direct tutorial shortcuts (1-5)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameManager?.LoadGameForTutorial(JourneyType.Walk);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameManager?.LoadGameForTutorial(JourneyType.Trail);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gameManager?.LoadGameForTutorial(JourneyType.Path);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gameManager?.LoadGameForTutorial(JourneyType.Circuit);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            gameManager?.LoadGameForTutorial(JourneyType.Cycle);
        }

        // Settings
        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowSettings();
        }

        // Exit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }

    /// <summary>
    /// Show settings panel (placeholder)
    /// </summary>
    private void ShowSettings()
    {
        LogDebug("Settings requested - implement settings panel");
        // TODO: Implement settings panel
        Debug.Log("Settings panel not yet implemented - add audio/graphics settings here");
    }

    /// <summary>
    /// Exit game
    /// </summary>
    private void ExitGame()
    {
        LogDebug("Exit game requested");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// Force refresh of all displays (for testing)
    /// </summary>
    [ContextMenu("Refresh Display")]
    public void RefreshDisplay()
    {
        UpdateProgressDisplay();
    }

    /// <summary>
    /// Test progress simulation
    /// </summary>
    [ContextMenu("Simulate Some Progress")]
    public void SimulateProgress()
    {
        if (progressTracker != null)
        {
            progressTracker.RecordTutorialCompletion(JourneyType.Walk);
            progressTracker.RecordMissionProgress(0, "Free Exploration");
        }
    }

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[MainMenuManager] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[MainMenuManager] {message}");
    }
}