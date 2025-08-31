using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using TMPro;
using StarterAssets;

/// <summary>
/// Central game manager that orchestrates all systems for 7Bridges MVP
/// Integrates journey tracking, missions, tutorials, cutscenes, and post-processing
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Core System References")]
    [SerializeField] private JourneyTracker journeyTracker;
    [SerializeField] private LearningMissionsManager missionsManager;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("World Areas")]
    [SerializeField] private Transform tutorialZone;
    [SerializeField] private Transform konigbergDistrict;
    [SerializeField] private Transform modernNetworksArea;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Cutscene System")]
    [SerializeField] private Canvas cutsceneCanvas;
    [SerializeField] private TextMeshProUGUI cutsceneText;
    [SerializeField] private CanvasGroup cutsceneCanvasGroup;
    [SerializeField] private AudioSource narratorAudioSource;

    [Header("Post-Processing")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private VolumeProfile tutorialProfile;
    [SerializeField] private VolumeProfile konigbergProfile;
    [SerializeField] private VolumeProfile completionProfile;

    [Header("Game Flow Settings")]
    [SerializeField] private GamePhase startingPhase = GamePhase.Introduction;
    [SerializeField] private bool skipIntroForTesting = false;

    // Game state tracking
    private GamePhase currentPhase;
    private bool systemsInitialized = false;
    private Player playerController;

    public enum GamePhase
    {
        Introduction,
        Tutorial,
        FreeExploration,
        KonigbergMission,
        Completion
    }

    #region Unity Lifecycle

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (!systemsInitialized) return;

        HandleGameFlow();
        HandleDebugInput();
    }

    #endregion

    #region Game Initialization

    private void InitializeGame()
    {
        Debug.Log("[GameManager] Initializing 7Bridges MVP...");

        StartCoroutine(InitializeSequence());
    }

    private IEnumerator InitializeSequence()
    {
        // 1. Validate core systems
        if (!ValidateSystems())
        {
            Debug.LogError("[GameManager] System validation failed - cannot start game");
            yield break;
        }

        // 2. Initialize player
        InitializePlayer();
        yield return new WaitForSeconds(0.1f);

        // 3. Setup post-processing
        SetupPostProcessing();
        yield return new WaitForSeconds(0.1f);

        // 4. Initialize UI systems
        InitializeUI();
        yield return new WaitForSeconds(0.1f);

        // 5. Start game flow
        systemsInitialized = true;

        if (skipIntroForTesting)
        {
            StartPhase(GamePhase.Tutorial);
        }
        else
        {
            StartPhase(startingPhase);
        }

        Debug.Log("[GameManager] Game initialization complete");
    }

    private bool ValidateSystems()
    {
        bool valid = true;

        if (journeyTracker == null)
        {
            Debug.LogError("[GameManager] JourneyTracker not assigned");
            valid = false;
        }

        if (missionsManager == null)
        {
            Debug.LogError("[GameManager] LearningMissionsManager not assigned");
            valid = false;
        }

        // Auto-find systems if not assigned
        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();

        if (missionsManager == null)
            missionsManager = FindFirstObjectByType<LearningMissionsManager>();

        return valid;
    }

    private void InitializePlayer()
    {
        playerController = FindFirstObjectByType<Player>();
        if (playerController != null && playerSpawnPoint != null)
        {
            playerController.transform.position = playerSpawnPoint.position;
            playerController.transform.rotation = playerSpawnPoint.rotation;
        }
    }

    private void SetupPostProcessing()
    {
        if (globalVolume != null && tutorialProfile != null)
        {
            globalVolume.profile = tutorialProfile;
        }
    }

    private void InitializeUI()
    {
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Game Flow Management

    private void StartPhase(GamePhase newPhase)
    {
        Debug.Log($"[GameManager] Starting phase: {newPhase}");

        currentPhase = newPhase;

        switch (newPhase)
        {
            case GamePhase.Introduction:
                StartCoroutine(PlayIntroductionSequence());
                break;

            case GamePhase.Tutorial:
                StartTutorialPhase();
                break;

            case GamePhase.FreeExploration:
                StartFreeExplorationPhase();
                break;

            case GamePhase.KonigbergMission:
                StartKonigbergMission();
                break;

            case GamePhase.Completion:
                StartCoroutine(PlayCompletionSequence());
                break;
        }
    }

    private void HandleGameFlow()
    {
        // Check for phase transitions based on current state
        switch (currentPhase)
        {
            case GamePhase.Tutorial:
                if (tutorialManager != null && tutorialManager.IsTutorialComplete)
                {
                    StartPhase(GamePhase.FreeExploration);
                }
                break;

            case GamePhase.FreeExploration:
                // Check if player has explored enough to unlock Königsberg
                if (journeyTracker.GetCurrentJourneyLength() >= 10)
                {
                    StartPhase(GamePhase.KonigbergMission);
                }
                break;

            case GamePhase.KonigbergMission:
                if (missionsManager != null && missionsManager.GetCurrentMissionIndex() >= 4)
                {
                    StartPhase(GamePhase.Completion);
                }
                break;
        }
    }

    #endregion

    #region Cutscene System

    private IEnumerator PlayIntroductionSequence()
    {
        yield return StartCoroutine(PlayCutscene(
            "Welcome to the world of networks and connections...",
            "You are about to embark on a mathematical journey through the bridges of Königsberg.",
            "Long ago, a mathematician named Leonhard Euler discovered something extraordinary about these bridges...",
            "But first, you must learn to see the hidden patterns that connect all things."
        ));

        StartPhase(GamePhase.Tutorial);
    }

    private IEnumerator PlayCompletionSequence()
    {
        // Switch to completion post-processing
        if (globalVolume != null && completionProfile != null)
        {
            yield return StartCoroutine(TransitionPostProcessing(completionProfile, 2f));
        }

        yield return StartCoroutine(PlayCutscene(
            "Congratulations! You have mastered the art of network navigation.",
            "You now understand the fundamental concepts that Euler discovered centuries ago.",
            "These patterns exist everywhere - in cities, in friendships, in the very structure of knowledge itself.",
            "You have learned to see the invisible connections that bind our world together."
        ));

        // Show final UI or return to menu
        ShowCompletionOptions();
    }

    private IEnumerator PlayCutscene(params string[] messages)
    {
        if (cutsceneCanvas == null || cutsceneText == null) yield break;

        // Disable player controls during cutscene
        SetPlayerControlsEnabled(false);

        // Fade in cutscene UI
        cutsceneCanvas.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(cutsceneCanvasGroup, 0f, 1f, 1f));

        // Display each message with timing
        foreach (string message in messages)
        {
            cutsceneText.text = message;

            // Play narrator audio if available
            if (narratorAudioSource != null && narratorAudioSource.clip != null)
            {
                narratorAudioSource.Play();
            }

            yield return new WaitForSeconds(3f + (message.Length * 0.05f)); // Dynamic timing based on text length
        }

        // Fade out cutscene UI
        yield return StartCoroutine(FadeCanvasGroup(cutsceneCanvasGroup, 1f, 0f, 1f));
        cutsceneCanvas.gameObject.SetActive(false);

        // Re-enable player controls
        SetPlayerControlsEnabled(true);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    #endregion

    #region Post-Processing Management

    private IEnumerator TransitionPostProcessing(VolumeProfile newProfile, float transitionTime)
    {
        if (globalVolume == null || newProfile == null) yield break;

        // Simple profile swap - for more advanced transitions, you'd blend weights
        float elapsed = 0f;
        VolumeProfile oldProfile = globalVolume.profile;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            // You could implement weight blending here for smoother transitions
            yield return null;
        }

        globalVolume.profile = newProfile;
    }

    private void SetPostProcessingForPhase(GamePhase phase)
    {
        if (globalVolume == null) return;

        VolumeProfile targetProfile = phase switch
        {
            GamePhase.Tutorial => tutorialProfile,
            GamePhase.KonigbergMission => konigbergProfile,
            GamePhase.Completion => completionProfile,
            _ => tutorialProfile
        };

        if (targetProfile != null)
        {
            StartCoroutine(TransitionPostProcessing(targetProfile, 1.5f));
        }
    }

    #endregion

    #region Phase-Specific Logic

    private void StartTutorialPhase()
    {
        Debug.Log("[GameManager] Starting tutorial phase");

        // Move player to tutorial zone
        if (playerController != null && tutorialZone != null)
        {
            MovePlayerToArea(tutorialZone);
        }

        // Set post-processing
        SetPostProcessingForPhase(GamePhase.Tutorial);

        // Initialize tutorial system
        if (tutorialManager != null)
        {
            tutorialManager.ChangeTutorialType(JourneyType.Walk);
        }
    }

    private void StartFreeExplorationPhase()
    {
        Debug.Log("[GameManager] Starting free exploration phase");

        // Reset journey for new phase
        journeyTracker?.ResetJourney();

        // Enable all areas for exploration
        EnableAllAreas(true);

        // Start with walk missions
        if (missionsManager != null)
        {
            missionsManager.JumpToMission(0);
        }
    }

    private void StartKonigbergMission()
    {
        Debug.Log("[GameManager] Starting Königsberg mission");

        // Play transition cutscene
        StartCoroutine(PlayKonigbergIntro());

        // Set post-processing for dramatic effect
        SetPostProcessingForPhase(GamePhase.KonigbergMission);

        // Move player to Königsberg area
        if (playerController != null && konigbergDistrict != null)
        {
            MovePlayerToArea(konigbergDistrict);
        }
    }

    private IEnumerator PlayKonigbergIntro()
    {
        yield return StartCoroutine(PlayCutscene(
            "You stand before the famous bridges of Königsberg...",
            "In 1736, Euler proved that crossing all seven bridges exactly once was impossible.",
            "Now you will discover why, and in doing so, understand the birth of graph theory."
        ));
    }

    #endregion

    #region Utility Methods

    private void MovePlayerToArea(Transform targetArea)
    {
        if (playerController != null && targetArea != null)
        {
            var spawnPoint = targetArea.Find("SpawnPoint");
            if (spawnPoint != null)
            {
                playerController.transform.position = spawnPoint.position;
                playerController.transform.rotation = spawnPoint.rotation;
            }
            else
            {
                playerController.transform.position = targetArea.position + Vector3.up * 2f;
            }
        }
    }

    private void EnableAllAreas(bool enabled)
    {
        if (tutorialZone != null) tutorialZone.gameObject.SetActive(enabled);
        if (konigbergDistrict != null) konigbergDistrict.gameObject.SetActive(enabled);
        if (modernNetworksArea != null) modernNetworksArea.gameObject.SetActive(enabled);
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        var thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = enabled;
        }

        var starterInputs = FindFirstObjectByType<StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.enabled = enabled;
        }
    }

    private void ShowCompletionOptions()
    {
        // Add completion UI here - restart, exit, etc.
        Debug.Log("[GameManager] Showing completion options");
    }

    #endregion

    #region Debug and Testing

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.F1)) StartPhase(GamePhase.Introduction);
        if (Input.GetKeyDown(KeyCode.F2)) StartPhase(GamePhase.Tutorial);
        if (Input.GetKeyDown(KeyCode.F3)) StartPhase(GamePhase.FreeExploration);
        if (Input.GetKeyDown(KeyCode.F4)) StartPhase(GamePhase.KonigbergMission);
        if (Input.GetKeyDown(KeyCode.F5)) StartPhase(GamePhase.Completion);
    }

    [ContextMenu("Test Introduction")]
    public void TestIntroduction() => StartPhase(GamePhase.Introduction);

    [ContextMenu("Test Completion")]
    public void TestCompletion() => StartPhase(GamePhase.Completion);

    #endregion

    #region Public Interface

    public GamePhase CurrentPhase => currentPhase;

    public void ForcePhaseTransition(GamePhase newPhase)
    {
        StartPhase(newPhase);
    }

    public void RestartGame()
    {
        // Reset all systems
        journeyTracker?.ResetJourney();

        // Restart from beginning
        StartPhase(GamePhase.Introduction);
    }

    #endregion
}