using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages contextual educational feedback during tutorials
/// Follows Observer pattern, listening to JourneyTracker events
/// Adheres to Single Responsibility Principle
/// </summary>
public class EducationalFeedbackManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI feedbackText; // Will use stepCounterText from MinimalModeUI

    [Header("Feedback Settings")]
    [SerializeField] private float educationalMessageDuration = 10f; // Important educational content
    [SerializeField] private float stepCounterDisplayDuration = 3f;  // Less important step counting
    [SerializeField] private bool enableDebugLogging = true;

    // Dependencies
    private JourneyTracker journeyTracker;
    private MinimalModeUI minimalModeUI;

    // State tracking
    private bool hasVisitedFirstVertex = false;
    private bool hasCrossedFirstBridge = false;
    private bool tutorialActive = false;
    private JourneyType currentTutorialType;
    private Coroutine currentMessageCoroutine;
    private Coroutine endFeedbackCoroutine;

    // Educational messages for Walk tutorial
    private readonly string[] walkMessages = {
        "Move to a vertex (sphere) to begin your walk!",
        "Great start! Move along the edge (bridge) to continue your walk.",
        "Building your walk! A walk is any sequence of connected vertices and edges - there are no restrictions.",
        "Well done, you completed a Walk! Now learn about trails or exit the tutorial."
    };

    // Educational messages for Trail tutorial
    private readonly string[] trailMessages = {
        "Move to a vertex (sphere) to begin your trail!",
        "Good start! Move along the edge (bridge) to continue your trail.",
        "Building your trail! A trail uses each bridge only once - but you can revisit vertices.",
        "Excellent! You completed a Trail! You avoided repeating any bridges."
    };

    // Educational messages for Path tutorial
    private readonly string[] pathMessages = {
        "Move to a vertex (sphere) to begin your path!",
        "Perfect! Move along the edge (bridge) to continue your path.",
        "Building your path! A path visits each vertex only once - the most efficient journey.",
        "Outstanding! You completed a Path! You visited each vertex exactly once."
    };

    // Educational messages for Circuit tutorial
    private readonly string[] circuitMessages = {
        "Move to a vertex (sphere) to begin your circuit!",
        "Nice! Move along the edge (bridge) to continue your circuit.",
        "Building your circuit! A circuit is a trail that returns to where you started.",
        "Brilliant! You completed a Circuit! You created a closed trail that returned home."
    };

    // Educational messages for Cycle tutorial
    private readonly string[] cycleMessages = {
        "Move to a vertex (sphere) to begin your cycle!",
        "Excellent! Move along the edge (bridge) to continue your cycle.",
        "Building your cycle! A cycle is a path that returns to where you started - the most elegant journey.",
        "Magnificent! You completed a Cycle! You created a perfect closed path."
    };

    void Start()
    {
        InitializeComponents();
        SetupEventListeners();
    }

    void OnDestroy()
    {
        CleanupEventListeners();
    }

    /// <summary>
    /// Initialize required components and references
    /// </summary>
    private void InitializeComponents()
    {
        // Find dependencies
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        minimalModeUI = FindFirstObjectByType<MinimalModeUI>();

        if (journeyTracker == null)
        {
            LogError("JourneyTracker not found!");
            enabled = false;
            return;
        }

        if (minimalModeUI == null)
        {
            LogError("MinimalModeUI not found!");
            enabled = false;
            return;
        }

        // Get reference to stepCounterText from MinimalModeUI
        // We'll access this through MinimalModeUI's public method
        LogDebug("EducationalFeedbackManager initialized");
    }

    /// <summary>
    /// Setup event listeners using Observer pattern
    /// </summary>
    private void SetupEventListeners()
    {
        if (journeyTracker == null) return;

        // Listen to journey events
        var vertices = FindObjectsByType<Vertex>(FindObjectsSortMode.None);
        foreach (var vertex in vertices)
        {
            vertex.OnVertexVisited += OnVertexVisited;
        }

        var bridges = FindObjectsByType<Bridge>(FindObjectsSortMode.None);
        foreach (var bridge in bridges)
        {
            bridge.OnBridgeCrossed += OnBridgeCrossed;
        }

        LogDebug($"Setup event listeners for {vertices.Length} vertices and {bridges.Length} bridges");
    }

    /// <summary>
    /// Cleanup event listeners to prevent memory leaks
    /// </summary>
    private void CleanupEventListeners()
    {
        var vertices = FindObjectsByType<Vertex>(FindObjectsSortMode.None);
        foreach (var vertex in vertices)
        {
            vertex.OnVertexVisited -= OnVertexVisited;
        }

        var bridges = FindObjectsByType<Bridge>(FindObjectsSortMode.None);
        foreach (var bridge in bridges)
        {
            bridge.OnBridgeCrossed -= OnBridgeCrossed;
        }
    }

    /// <summary>
    /// Called when tutorial starts - reset state and show initial message
    /// </summary>
    public void StartTutorialFeedback(JourneyType tutorialType)
    {
        // Stop any running coroutines from previous tutorials
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }

        if (endFeedbackCoroutine != null)
        {
            StopCoroutine(endFeedbackCoroutine);
            endFeedbackCoroutine = null;
            LogDebug("Stopped previous tutorial's end feedback coroutine");
        }

        // Reset state for new tutorial
        currentTutorialType = tutorialType;
        tutorialActive = true;
        hasVisitedFirstVertex = false;
        hasCrossedFirstBridge = false;

        LogDebug($"Started educational feedback for {tutorialType} tutorial (state reset)");

        // Show initial message for current tutorial type
        string[] messages = GetMessagesForType(tutorialType);
        if (messages != null && messages.Length > 0)
        {
            ShowEducationalMessage(messages[0]);
        }
    }

    /// <summary>
    /// Called when tutorial ends - cleanup state
    /// </summary>
    public void EndTutorialFeedback()
    {
        tutorialActive = false;

        // Stop any current message
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }

        // Stop any end feedback delay
        if (endFeedbackCoroutine != null)
        {
            StopCoroutine(endFeedbackCoroutine);
            endFeedbackCoroutine = null;
        }

        LogDebug("Ended educational feedback");
    }

    /// <summary>
    /// Handle vertex visit events
    /// </summary>
    private void OnVertexVisited(IVertex vertex, ICrosser crosser)
    {
        if (!tutorialActive) return;

        // Only trigger on player actions
        if (crosser.CrosserId != "Player") return;

        // First vertex visit
        if (!hasVisitedFirstVertex)
        {
            hasVisitedFirstVertex = true;
            string[] messages = GetMessagesForType(currentTutorialType);
            if (messages != null && messages.Length > 1)
            {
                ShowEducationalMessage(messages[1]); // Second message: "Great start! Move along the edge..."
                LogDebug($"First vertex visited - showing bridge guidance for {currentTutorialType}");
            }
        }
    }

    /// <summary>
    /// Handle bridge crossing events  
    /// </summary>
    private void OnBridgeCrossed(IBridge bridge, ICrosser crosser)
    {
        if (!tutorialActive) return;

        // Only trigger on player actions
        if (crosser.CrosserId != "Player") return;

        // First bridge crossing
        if (!hasCrossedFirstBridge && hasVisitedFirstVertex)
        {
            hasCrossedFirstBridge = true;
            string[] messages = GetMessagesForType(currentTutorialType);
            if (messages != null && messages.Length > 2)
            {
                ShowEducationalMessage(messages[2]); // Third message: "Building your [type]! [Definition]..."
                LogDebug($"First bridge crossed - showing {currentTutorialType} definition");
            }
        }
    }

    /// <summary>
    /// Called when tutorial is completed
    /// </summary>
    public void OnTutorialCompleted(JourneyType tutorialType)
    {
        if (!tutorialActive || currentTutorialType != tutorialType) return;

        string[] messages = GetMessagesForType(tutorialType);
        if (messages != null && messages.Length > 3)
        {
            ShowEducationalMessage(messages[3]); // Fourth message: "Well done, you completed a [Type]!"
            LogDebug($"{tutorialType} tutorial completed - showing completion message");
        }

        // End feedback after showing completion message
        endFeedbackCoroutine = StartCoroutine(EndFeedbackAfterDelay(educationalMessageDuration + 1f));
    }

    /// <summary>
    /// Display educational message in the step counter area
    /// </summary>
    private void ShowEducationalMessage(string message)
    {
        // Stop any current message
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        // Start new message display
        currentMessageCoroutine = StartCoroutine(DisplayEducationalMessage(message));
    }

    /// <summary>
    /// Coroutine to display educational message temporarily
    /// </summary>
    private IEnumerator DisplayEducationalMessage(string message)
    {
        // Update the step counter text with educational message
        if (minimalModeUI != null)
        {
            minimalModeUI.ShowEducationalMessage(message);
        }

        LogDebug($"Showing educational message: {message}");

        // Wait for message duration (educational messages get more time)
        yield return new WaitForSeconds(educationalMessageDuration);

        // Revert to normal step counter display
        if (minimalModeUI != null)
        {
            minimalModeUI.RevertToStepCounter();
        }

        currentMessageCoroutine = null;
    }

    /// <summary>
    /// End feedback after delay (for completion messages)
    /// </summary>
    private IEnumerator EndFeedbackAfterDelay(float delay)
    {
        JourneyType completedTutorialType = currentTutorialType;
        yield return new WaitForSeconds(delay);

        // Only end feedback if we're still in the same tutorial
        // (prevents interference with auto-progressed tutorials)
        if (currentTutorialType == completedTutorialType && tutorialActive)
        {
            EndTutorialFeedback();
            LogDebug($"Auto-ended feedback for {completedTutorialType} after delay");
        }
        else
        {
            LogDebug($"Skipped auto-end for {completedTutorialType} - new tutorial {currentTutorialType} is active");
        }

        endFeedbackCoroutine = null;
    }

    /// <summary>
    /// Get the appropriate message array for the specified tutorial type
    /// </summary>
    private string[] GetMessagesForType(JourneyType tutorialType)
    {
        return tutorialType switch
        {
            JourneyType.Walk => walkMessages,
            JourneyType.Trail => trailMessages,
            JourneyType.Path => pathMessages,
            JourneyType.Circuit => circuitMessages,
            JourneyType.Cycle => cycleMessages,
            _ => null
        };
    }

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[EducationalFeedback] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[EducationalFeedback] {message}");
    }

    // Public methods for external integration
    public bool IsTutorialActive => tutorialActive;
    public JourneyType CurrentTutorialType => currentTutorialType;
}