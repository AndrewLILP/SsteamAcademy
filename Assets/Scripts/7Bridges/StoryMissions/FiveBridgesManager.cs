using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

public class FiveBridgesManager : MonoBehaviour
{
    [Header("Timeline Components")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera playerCamera;

    [Header("Dialogue Components")]
    [SerializeField] private DialogueSystem dialogueSystem;

    [Header("Tutorial Components")]
    [SerializeField] private JourneyTracker journeyTracker;
    [SerializeField] private GameObject tutorialUI;

    [Header("Cutscene Settings")]
    [SerializeField] private bool skipCutscene = false; // For testing

    private bool cutsceneComplete = false;
    private bool tutorialStarted = false;

    // Dialogue content
    private readonly string[] cutsceneDialogue = {
        "During the war, two of these bridges were destroyed...",
        "Now observe - with only 5 bridges, this puzzle becomes solvable!",
        "It is possible to cross each bridge exactly once.",
        "This creates what we call an Eulerian Path!",
        "Try it yourself - cross each bridge exactly once."
    };

    void Start()
    {
        if (skipCutscene)
        {
            SkipToCutsceneEnd();
        }
        else
        {
            StartCutscene();
        }
    }

    void Update()
    {
        // Check for tutorial completion
        if (tutorialStarted && journeyTracker != null)
        {
            if (journeyTracker.IsMissionComplete())
            {
                StartCoroutine(HandleTutorialCompletion());
                tutorialStarted = false; // Prevent multiple triggers
            }
        }

        // Debug controls
        if (Input.GetKeyDown(KeyCode.Space) && !cutsceneComplete)
        {
            SkipToCutsceneEnd();
        }
    }

    private void StartCutscene()
    {
        Debug.Log("[FiveBridgesManager] Starting cutscene");

        // Setup cameras
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

        // Hide tutorial UI
        if (tutorialUI != null)
            tutorialUI.SetActive(false);

        // Start timeline if available
        if (timelineDirector != null && timelineDirector.playableAsset != null)
        {
            timelineDirector.Play();
        }
        else
        {
            Debug.LogWarning("[FiveBridgesManager] No timeline assigned, using dialogue-only cutscene");
            StartCoroutine(PlayDialogueOnlyCutscene());
        }
    }

    private void SkipToCutsceneEnd()
    {
        Debug.Log("[FiveBridgesManager] Skipping to cutscene end");

        if (timelineDirector != null && timelineDirector.state == PlayState.Playing)
        {
            timelineDirector.Stop();
        }

        StartInteractiveTutorial();
    }

    // Timeline Signal Methods - called by Timeline signals
    public void TriggerDialogue1()
    {
        ShowDialogue(0);
    }

    public void TriggerDialogue2()
    {
        ShowDialogue(1);
    }

    public void TriggerDialogue3()
    {
        ShowDialogue(2);
    }

    public void TriggerDialogue4()
    {
        ShowDialogue(3);
    }

    public void TriggerDialogue5()
    {
        ShowDialogue(4);
    }

    // Called by Timeline at the end
    public void OnCutsceneComplete()
    {
        Debug.Log("[FiveBridgesManager] Timeline cutscene completed");
        StartInteractiveTutorial();
    }

    private void ShowDialogue(int index)
    {
        if (index >= 0 && index < cutsceneDialogue.Length && dialogueSystem != null)
        {
            dialogueSystem.ShowDialogue(cutsceneDialogue[index], "Leonhard Euler");
        }
    }

    private IEnumerator PlayDialogueOnlyCutscene()
    {
        // Fallback if no timeline - just play dialogue sequence
        for (int i = 0; i < cutsceneDialogue.Length; i++)
        {
            ShowDialogue(i);

            // Wait for dialogue completion
            while (dialogueSystem != null && dialogueSystem.IsDialogueActive)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);
        }

        StartInteractiveTutorial();
    }

    private void StartInteractiveTutorial()
    {
        Debug.Log("[FiveBridgesManager] Starting interactive tutorial");

        cutsceneComplete = true;
        tutorialStarted = true;

        // Switch cameras back
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        // Show tutorial UI
        if (tutorialUI != null)
            tutorialUI.SetActive(true);

        // Setup journey tracker for circuit
        if (journeyTracker != null)
        {
            journeyTracker.SetMissionType(JourneyType.Circuit);
            journeyTracker.ResetJourney();
        }

        // Show instruction
        if (dialogueSystem != null)
        {
            dialogueSystem.ShowDialogue("Now try to complete the Eulerian Circuit yourself!", "System");
        }
    }

    private IEnumerator HandleTutorialCompletion()
    {
        Debug.Log("[FiveBridgesManager] Tutorial completed");

        // Show completion dialogue
        if (dialogueSystem != null)
        {
            dialogueSystem.ShowDialogue("Excellent! You've mastered the Eulerian Circuit. Now you understand why the 7-bridge problem was impossible!", "Leonhard Euler");

            // Wait for dialogue completion
            while (dialogueSystem.IsDialogueActive)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(2f);

        // Return to main menu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogWarning("[FiveBridgesManager] GameManager.Instance is null");
        }
    }

    // Debug methods
    [ContextMenu("Test Skip Cutscene")]
    public void TestSkipCutscene()
    {
        SkipToCutsceneEnd();
    }

    [ContextMenu("Test Tutorial Complete")]
    public void TestTutorialComplete()
    {
        StartCoroutine(HandleTutorialCompletion());
    }

    void OnValidate()
    {
        // Auto-find components if not assigned
        if (timelineDirector == null)
            timelineDirector = GetComponent<PlayableDirector>();

        if (dialogueSystem == null)
            dialogueSystem = FindFirstObjectByType<DialogueSystem>();

        if (journeyTracker == null)
            journeyTracker = FindFirstObjectByType<JourneyTracker>();
    }
}