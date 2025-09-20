// StoryManager.cs - Simplified
// Manages the Königsberg story progression
using System.Collections;
using UnityEngine;

/// <summary>
/// Simple story manager for Königsberg narrative
/// Follows Single Responsibility Principle - just coordinates story flow
/// </summary>
public class StoryManager : MonoBehaviour
{
    [Header("Story Configuration")]
    [SerializeField] private int stepsRequiredForEuler = 3;

    [Header("System References")]
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private JourneyTracker journeyTracker;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Story state
    private bool storyActive = false;
    private bool introductionShown = false;
    private bool eulerAppeared = false;
    private bool storyCompleted = false;

    // Story dialogue
    private readonly string[] introDialogue = {
        "Welcome to Königsberg, 1736!",
        "You are a curious citizen who has heard about an interesting puzzle...",
        "The townspeople speak of seven bridges connecting four landmasses.",
        "The challenge: Can someone walk through the city and cross each bridge exactly once?",
        "Explore the city and try this puzzle for yourself!"
    };

    private readonly string[] eulerDialogue = {
        "Ah, another citizen attempting the bridge puzzle!",
        "I am Leonhard Euler, a mathematician visiting your fine city.",
        "I have been observing these attempts with great interest...",
        "This simple problem gave birth to an entire new field: graph theory!",
        "Sometimes the greatest discovery is proving that something cannot be done."
    };

    void Start()
    {
        InitializeStory();
    }

    void Update()
    {
        if (storyActive && !storyCompleted)
        {
            CheckStoryProgress();
        }

        // ESC to return to main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }

    [ContextMenu("Test 5Bridges Transition")]
    public void TestTransitionTo5Bridges()
    {
        LogDebug("Testing direct transition to 5Bridges...");

        if (GameManager.Instance != null)
        {
            LogDebug("GameManager found, calling LoadFiveBridgesScene()");
            GameManager.Instance.LoadFiveBridgesScene();
        }
        else
        {
            LogDebug("ERROR: GameManager.Instance is NULL!");
        }
    }

    /// <summary>
    /// Initialize story systems
    /// </summary>
    private void InitializeStory()
    {
        // Find components if not assigned
        if (dialogueSystem == null)
            dialogueSystem = FindFirstObjectByType<DialogueSystem>();

        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();

        LogDebug("StoryManager initialized");
    }

    /// <summary>
    /// Start the Königsberg story
    /// </summary>
    public void StartStory()
    {
        if (storyActive) return;

        storyActive = true;

        // Setup journey tracker for story mode
        if (journeyTracker != null)
        {
            journeyTracker.ResetJourney();
            journeyTracker.SetMissionType(JourneyType.Walk);
        }


        // Show introduction
        //StartCoroutine(ShowIntroduction());
        StartCoroutine(DelayedStart());

        LogDebug("Königsberg story started");
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f); // Wait 2 seconds
        StartCoroutine(ShowIntroduction());
    }

    /// <summary>
    /// Check story progression based on player actions
    /// </summary>
    private void CheckStoryProgress()
    {
        if (!introductionShown || storyCompleted) return;

        if (journeyTracker != null)
        {
            int currentSteps = journeyTracker.GetCurrentJourneyLength();

            // Euler appears when player has explored enough
            if (!eulerAppeared && currentSteps >= stepsRequiredForEuler)
            {
                StartCoroutine(ShowEulerSequence());
            }
        }
    }

    /// <summary>
    /// Show introduction dialogue sequence
    /// </summary>
    private IEnumerator ShowIntroduction()
    {
        if (dialogueSystem == null) yield break;

        for (int i = 0; i < introDialogue.Length; i++)
        {
            dialogueSystem.ShowDialogue(introDialogue[i], "Narrator");

            // Wait for dialogue to complete
            while (dialogueSystem.IsDialogueActive)
            {
                yield return null;
            }

            // Pause between dialogues
            yield return new WaitForSeconds(0.5f);
        }

        introductionShown = true;
        LogDebug("Introduction sequence completed");
    }

    /// <summary>
    /// Show Euler appearance and dialogue
    /// </summary>
    private IEnumerator ShowEulerSequence()
    {
        if (eulerAppeared || dialogueSystem == null) yield break;

        eulerAppeared = true;

        LogDebug("Euler sequence starting");

        // Small delay before Euler appears
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < eulerDialogue.Length; i++)
        {
            dialogueSystem.ShowDialogue(eulerDialogue[i], "Leonhard Euler");

            // Wait for dialogue to complete
            while (dialogueSystem.IsDialogueActive)
            {
                yield return null;
            }

            // Pause between dialogues
            yield return new WaitForSeconds(0.5f);
        }

        // Complete the story
        CompleteStory();
    }

    /// <summary>
    /// Complete the story sequence
    /// </summary>
    // updated for 5 bridges cutscene and story the final dialogue:
    private void CompleteStory()
    {
        storyCompleted = true;

        // Replace "Press ESC" with transition dialogue
        dialogueSystem.ShowDialogue("Now let me show you something interesting about those bridges...", "Leonhard Euler");
        dialogueSystem.OnDialogueComplete += TransitionTo5Bridges;
    }

    private void TransitionTo5Bridges()
    {
        dialogueSystem.OnDialogueComplete -= TransitionTo5Bridges;
        GameManager.Instance?.LoadFiveBridgesScene();
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        LogDebug("Returning to main menu");

        // Find and use GameManager to return to menu
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.ReturnToMainMenu();
        }
        else
        {
            // Fallback - load main menu scene directly
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Get current story progress for UI display
    /// </summary>
    public string GetStoryProgress()
    {
        if (!storyActive)
            return "Story not started";

        if (!introductionShown)
            return "Introduction...";

        if (!eulerAppeared)
            return "Exploring the bridges...";

        if (!storyCompleted)
            return "Meeting with Euler...";

        return "Story complete!";
    }

    /// <summary>
    /// Check if story is currently active
    /// </summary>
    public bool IsStoryActive => storyActive;

    /// <summary>
    /// Check if story is completed
    /// </summary>
    public bool IsStoryComplete => storyCompleted;

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[StoryManager] {message}");
    }

    // Manual testing methods
    [ContextMenu("Start Story")]
    public void TestStartStory()
    {
        StartStory();
    }

    [ContextMenu("Force Euler Sequence")]
    public void TestEulerSequence()
    {
        if (storyActive && !eulerAppeared)
        {
            StartCoroutine(ShowEulerSequence());
        }
    }

    [ContextMenu("Return to Menu")]
    public void TestReturnToMenu()
    {
        ReturnToMainMenu();
    }
}