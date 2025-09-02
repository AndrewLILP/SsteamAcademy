using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// ==============================================
// CORE DATA STRUCTURES
// ==============================================

[System.Serializable]
public class JourneyStep
{
    public string vertexId;
    public string edgeId; // bridge that was crossed to reach this vertex
    public float timestamp;

    public JourneyStep(string vertex, string edge = null)
    {
        vertexId = vertex;
        edgeId = edge;
        timestamp = Time.time;
    }
}

public enum JourneyType
{
    Walk,        // Basic sequence of vertices and edges
    Trail,       // No repeated edges
    Path,        // No repeated vertices  
    Circuit,     // Trail that returns to start
    Cycle,       // Path that returns to start
    Invalid      // Doesn't meet any criteria
}

// ==============================================
// CONFIGURATION SYSTEM
// ==============================================

// Configuration data for each journey type
[System.Serializable]
public class JourneyTypeConfig
{
    public JourneyType journeyType;
    public int minimumStepsForClassification;
    public int minimumStepsForCompletion;
    public string missionInstruction;
    public string progressEncouragement;
    public string successMessage;
    public string correctionHint;

    public JourneyTypeConfig(JourneyType type, int classificationSteps, int completionSteps,
                           string instruction, string encouragement, string success, string hint)
    {
        journeyType = type;
        minimumStepsForClassification = classificationSteps;
        minimumStepsForCompletion = completionSteps;
        missionInstruction = instruction;
        progressEncouragement = encouragement;
        successMessage = success;
        correctionHint = hint;
    }
}

// Enhanced JourneyTracker with comprehensive debug logging for Sprint 3 Task 1
public class JourneyTracker : MonoBehaviour, IPuzzleSystem
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI journeyStatusText;
    [SerializeField] private TextMeshProUGUI journeyHistoryText;
    [SerializeField] private TextMeshProUGUI missionObjectiveText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Mission Configuration")]
    [SerializeField] private JourneyType targetJourneyType = JourneyType.Walk;

    [Header("Debug Settings - Sprint 3")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool enableVerboseStateLogging = false;

    private List<JourneyStep> currentJourney = new List<JourneyStep>();
    private string startingVertex = null;
    private Dictionary<JourneyType, JourneyTypeConfig> journeyConfigs;
    
    // Debug tracking variables
    private int resetCount = 0;
    private float lastResetTime = 0f;
    private JourneyType lastTargetType = JourneyType.Invalid;

    public bool IsActive => true;

    void Awake()
    {
        InitializeJourneyConfigs();
        LogDebug($"JourneyTracker Awake - Journey initialized with {currentJourney.Count} steps");
    }

    void Start()
    {
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

        LogDebug($"JourneyTracker Start - Registered with {vertices.Length} vertices and {bridges.Length} bridges");
        UpdateUI();
    }

    // SPRINT 3 TASK 1: Enhanced Reset Logic with Debug Logging
    public void ResetJourney()
    {
        resetCount++;
        lastResetTime = Time.time;
        
        LogDebug("=== JOURNEY RESET INITIATED ===");
        LogDebug($"Reset #{resetCount} at time {lastResetTime:F2}");
        
        // Log state before reset
        LogDebug($"BEFORE RESET - Journey steps: {currentJourney.Count}");
        LogDebug($"BEFORE RESET - Starting vertex: '{startingVertex}'");
        LogDebug($"BEFORE RESET - Target type: {targetJourneyType}");
        
        if (currentJourney.Count > 0)
        {
            LogDebug($"BEFORE RESET - Journey path: {string.Join(" → ", currentJourney.Select(s => s.vertexId))}");
            if (enableVerboseStateLogging)
            {
                for (int i = 0; i < currentJourney.Count; i++)
                {
                    var step = currentJourney[i];
                    LogDebug($"  Step {i}: Vertex={step.vertexId}, Edge={step.edgeId}, Time={step.timestamp:F2}");
                }
            }
        }

        // Perform the reset operations with verification
        var oldJourneyCount = currentJourney.Count;
        var oldStartingVertex = startingVertex;
        
        // Clear the journey list
        currentJourney.Clear();
        
        // Verify the clear operation worked
        if (currentJourney.Count != 0)
        {
            LogError($"CRITICAL: currentJourney.Clear() failed! Still contains {currentJourney.Count} items");
            // Force clear by creating new list
            currentJourney = new List<JourneyStep>();
            LogDebug($"Created new journey list. Count now: {currentJourney.Count}");
        }
        else
        {
            LogDebug("✓ currentJourney.Clear() successful");
        }
        
        // Reset starting vertex
        startingVertex = null;
        
        // Verify starting vertex reset
        if (startingVertex != null)
        {
            LogError($"CRITICAL: startingVertex = null assignment failed! Still contains: '{startingVertex}'");
            // Force reset
            startingVertex = string.Empty;
            startingVertex = null;
        }
        else
        {
            LogDebug("✓ startingVertex = null successful");
        }
        
        // Log state after reset
        LogDebug($"AFTER RESET - Journey steps: {currentJourney.Count}");
        LogDebug($"AFTER RESET - Starting vertex: '{startingVertex}'");
        LogDebug($"AFTER RESET - Target type: {targetJourneyType}");
        
        // Verification checks
        bool resetSuccessful = (currentJourney.Count == 0 && startingVertex == null);
        LogDebug($"Reset verification: {(resetSuccessful ? "✓ SUCCESS" : "✗ FAILED")}");
        
        if (!resetSuccessful)
        {
            LogError("JOURNEY RESET FAILED - State not properly cleared!");
            // Additional diagnostic information
            LogError($"Journey count expected: 0, actual: {currentJourney.Count}");
            LogError($"Starting vertex expected: null, actual: '{startingVertex}'");
        }
        
        // Update UI after reset
        UpdateUI();
        
        LogDebug($"=== JOURNEY RESET COMPLETE ({resetCount}) ===\n");
        
        // Log reset summary for debugging
        if (enableVerboseStateLogging)
        {
            LogDebug($"Reset Summary - Changed from {oldJourneyCount} steps to {currentJourney.Count} steps");
            LogDebug($"Reset Summary - Changed starting vertex from '{oldStartingVertex}' to '{startingVertex}'");
        }
    }

    // SPRINT 3 TASK 1: Enhanced Mission Type Setter with State Validation
    public void SetMissionType(JourneyType newTarget)
    {
        var oldTarget = targetJourneyType;
        targetJourneyType = newTarget;
        
        LogDebug($"Mission type changed: {oldTarget} → {newTarget}");
        
        // Validate that journey is actually reset when mission changes
        if (oldTarget != newTarget && (currentJourney.Count > 0 || startingVertex != null))
        {
            LogWarning($"Mission type changed but journey state not clean! Steps: {currentJourney.Count}, StartVertex: '{startingVertex}'");
            LogWarning("This may cause instant mission completion - consider calling ResetJourney() first");
        }
        
        lastTargetType = newTarget;
        UpdateUI();
    }

    // SPRINT 3 TASK 1: Enhanced Mission Complete Check with Debug Info
    public bool IsMissionComplete()
    {
        var config = GetCurrentConfig();
        
        bool hasMinSteps = currentJourney.Count >= config.minimumStepsForCompletion;
        var actualType = AnalyzeCurrentJourney();
        bool correctType = IsJourneyTypeValid(actualType, targetJourneyType);
        bool isComplete = hasMinSteps && correctType;
        
        if (enableVerboseStateLogging)
        {
            LogDebug($"Mission Completion Check:");
            LogDebug($"  Steps: {currentJourney.Count}/{config.minimumStepsForCompletion} = {(hasMinSteps ? "✓" : "✗")}");
            LogDebug($"  Type: {actualType} vs {targetJourneyType} = {(correctType ? "✓" : "✗")}");
            LogDebug($"  Complete: {(isComplete ? "✓ YES" : "✗ NO")}");
        }
        
        // Log concerning cases
        if (isComplete && currentJourney.Count == 0)
        {
            LogError("CRITICAL: Mission marked complete with 0 journey steps! This indicates a state management bug.");
        }
        
        if (isComplete && Time.time - lastResetTime < 0.1f)
        {
            LogWarning($"Mission completed immediately after reset ({Time.time - lastResetTime:F3}s) - possible state persistence issue");
        }
        
        return isComplete;
    }

    public void OnVertexVisited(IVertex vertex, ICrosser crosser)
    {
        LogDebug($"Vertex visited: {vertex.VertexId} by {crosser.CrosserId}");
        
        var step = new JourneyStep(vertex.VertexId);

        if (currentJourney.Count == 0)
        {
            startingVertex = vertex.VertexId;
            LogDebug($"Starting vertex set to: {startingVertex}");
        }

        currentJourney.Add(step);
        LogDebug($"Journey now has {currentJourney.Count} steps");
        
        UpdateUI();
    }

    public void OnBridgeCrossed(IBridge bridge, ICrosser crosser)
    {
        LogDebug($"Bridge crossed: {bridge.BridgeId} by {crosser.CrosserId}");
        
        if (currentJourney.Count > 0)
        {
            var lastStep = currentJourney[currentJourney.Count - 1];
            lastStep.edgeId = bridge.BridgeId;
            LogDebug($"Added edge {bridge.BridgeId} to step {currentJourney.Count - 1}");
        }
        else
        {
            LogWarning($"Bridge {bridge.BridgeId} crossed but no journey steps recorded - missing vertex detection?");
        }

        UpdateUI();
    }

    // SPRINT 3 TASK 1: Enhanced Journey Analysis with Debug Logging
    private JourneyType AnalyzeCurrentJourney()
    {
        if (currentJourney.Count < 2)
        {
            LogDebug($"Journey analysis: Too few steps ({currentJourney.Count}) - defaulting to Walk");
            return JourneyType.Walk;
        }

        var vertices = currentJourney.Select(step => step.vertexId).ToList();
        var edges = currentJourney.Where(step => !string.IsNullOrEmpty(step.edgeId))
                                 .Select(step => step.edgeId).ToList();

        bool returnsToStart = vertices.Count > 2 && vertices.First() == vertices.Last();
        bool hasRepeatedVertices = vertices.Count != vertices.Distinct().Count();
        bool hasRepeatedEdges = edges.Count != edges.Distinct().Count();

        LogDebug($"Journey analysis: Vertices={vertices.Count}, Edges={edges.Count}");
        LogDebug($"  Returns to start: {returnsToStart}");
        LogDebug($"  Repeated vertices: {hasRepeatedVertices}");
        LogDebug($"  Repeated edges: {hasRepeatedEdges}");

        JourneyType result;

        if (returnsToStart)
        {
            var pathVertices = vertices.Take(vertices.Count - 1).ToList();
            bool pathHasRepeatedVertices = pathVertices.Count != pathVertices.Distinct().Count();

            if (!pathHasRepeatedVertices && !hasRepeatedEdges)
            {
                result = JourneyType.Cycle;
            }
            else if (!hasRepeatedEdges)
            {
                result = JourneyType.Circuit;
            }
            else
            {
                result = JourneyType.Walk;
            }
        }
        else if (!hasRepeatedVertices)
        {
            result = JourneyType.Path;
        }
        else if (!hasRepeatedEdges)
        {
            result = JourneyType.Trail;
        }
        else
        {
            result = JourneyType.Walk;
        }

        LogDebug($"Journey classified as: {result}");
        return result;
    }

    // Debug logging methods
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[JourneyTracker] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (enableDebugLogging)
        {
            Debug.LogWarning($"[JourneyTracker] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[JourneyTracker] {message}");
    }

    // Public debug methods for external testing
    public void EnableDebugMode(bool verbose = false)
    {
        enableDebugLogging = true;
        enableVerboseStateLogging = verbose;
        LogDebug($"Debug mode enabled (verbose: {verbose})");
    }

    public void DisableDebugMode()
    {
        enableDebugLogging = false;
        enableVerboseStateLogging = false;
    }

    public string GetDebugInfo()
    {
        return $"Journey Steps: {currentJourney.Count}, Starting Vertex: '{startingVertex}', " +
               $"Target Type: {targetJourneyType}, Reset Count: {resetCount}, " +
               $"Last Reset: {lastResetTime:F2}s ago";
    }

    // Existing methods remain the same...
    private void InitializeJourneyConfigs()
    {
        journeyConfigs = new Dictionary<JourneyType, JourneyTypeConfig>
        {
            [JourneyType.Walk] = new JourneyTypeConfig(
                JourneyType.Walk,
                5, 3,
                "Move between vertices in any pattern - you can repeat vertices and edges freely",
                "Keep exploring! A walk allows any movement pattern",
                "Perfect walk! You moved freely between vertices",
                "For a walk, you can revisit any vertex or cross any bridge multiple times"
            ),

            [JourneyType.Trail] = new JourneyTypeConfig(
                JourneyType.Trail,
                4, 4,
                "Visit vertices without using the same bridge twice - vertices can be revisited",
                "Good progress! Remember: no bridge should be crossed twice",
                "Excellent trail! You avoided repeating any bridges",
                "For a trail, you can revisit vertices, but don't cross the same bridge twice"
            ),

            [JourneyType.Path] = new JourneyTypeConfig(
                JourneyType.Path,
                4, 4,
                "Visit each vertex only once - the most restrictive journey type",
                "Great! Remember: each vertex should only be visited once",
                "Outstanding path! You visited each vertex exactly once",
                "For a path, no vertex should be visited more than once"
            ),

            [JourneyType.Circuit] = new JourneyTypeConfig(
                JourneyType.Circuit,
                6, 5,
                "Create a trail that returns to your starting vertex",
                "Building your circuit! Remember: no repeated bridges, and return to start",
                "Perfect circuit! You created a closed trail",
                "For a circuit, avoid repeating bridges and end where you started"
            ),

            [JourneyType.Cycle] = new JourneyTypeConfig(
                JourneyType.Cycle,
                6, 5,
                "Create a path that returns to your starting vertex",
                "Creating your cycle! Remember: no repeated vertices, and return to start",
                "Excellent cycle! You created a closed path",
                "For a cycle, visit each vertex once and end where you started"
            ),

            [JourneyType.Invalid] = new JourneyTypeConfig(
                JourneyType.Invalid,
                2, 2,
                "This journey type is not currently available",
                "Continue exploring",
                "Journey complete",
                "Keep going"
            )
        };
    }
    private JourneyTypeConfig GetCurrentConfig()
    {
        // Ensure journeyConfigs is initialized
        if (journeyConfigs == null)
        {
            LogDebug("Journey configs not initialized - initializing now");
            InitializeJourneyConfigs();
        }

        // Check if targetJourneyType exists in configs
        if (journeyConfigs != null && journeyConfigs.TryGetValue(targetJourneyType, out var config))
        {
            return config;
        }

        // Fallback - return Invalid config or create a safe default
        LogWarning($"No config found for journey type: {targetJourneyType} - using fallback");

        // Return a safe fallback config
        return journeyConfigs?.ContainsKey(JourneyType.Invalid) == true
            ? journeyConfigs[JourneyType.Invalid]
            : new JourneyTypeConfig(
                JourneyType.Walk, 2, 2,
                "Basic movement between vertices",
                "Keep exploring",
                "Journey complete",
                "Keep moving"
            );
    }
    private void UpdateUI()
    {
        try
        {
            var config = GetCurrentConfig();
            if (config != null)
            {
                UpdatePathHistory();
                UpdateMissionObjective(config);

                if (currentJourney.Count < config.minimumStepsForClassification)
                {
                    ShowProgressFeedback(config);
                }
                else
                {
                    ShowClassificationFeedback(config);
                }
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error updating UI: {ex.Message}");
        }
    }

    private void UpdatePathHistory()
    {
        if (journeyHistoryText != null)
        {
            var history = currentJourney.Count > 0 
                ? string.Join(" → ", currentJourney.Select(step => step.vertexId))
                : "(No journey)";
            journeyHistoryText.text = $"Journey: {history}";
        }
    }

    private void UpdateMissionObjective(JourneyTypeConfig config)
    {
        if (missionObjectiveText != null)
        {
            missionObjectiveText.text = $"Mission: {config.missionInstruction}";

            if (IsMissionComplete())
            {
                missionObjectiveText.text = $"COMPLETE: {config.successMessage}";
                missionObjectiveText.color = Color.green;
            }
            else
            {
                missionObjectiveText.color = Color.white;
            }
        }
    }

    private void ShowProgressFeedback(JourneyTypeConfig config)
    {
        if (journeyStatusText != null)
        {
            journeyStatusText.text = $"Creating your {targetJourneyType}...";
            journeyStatusText.color = Color.cyan;
        }

        if (feedbackText != null)
        {
            int stepsRemaining = config.minimumStepsForClassification - currentJourney.Count;
            feedbackText.text = $"{config.progressEncouragement}\nSteps needed: {stepsRemaining}";
            feedbackText.color = Color.yellow;
        }
    }

    private void ShowClassificationFeedback(JourneyTypeConfig config)
    {
        var actualType = AnalyzeCurrentJourney();
        bool isCorrectType = IsJourneyTypeValid(actualType, targetJourneyType);

        if (journeyStatusText != null)
        {
            journeyStatusText.text = $"Current: {actualType} | Target: {targetJourneyType}";
            journeyStatusText.color = isCorrectType ? Color.green : Color.black;
        }

        if (feedbackText != null)
        {
            feedbackText.text = GetTargetedFeedback(actualType, targetJourneyType, config);
            feedbackText.color = isCorrectType ? Color.green : Color.blue;
        }
    }

    private bool IsJourneyTypeValid(JourneyType actual, JourneyType target)
    {
        return target switch
        {
            JourneyType.Walk => true,
            JourneyType.Trail => actual == JourneyType.Trail || actual == JourneyType.Path ||
                               actual == JourneyType.Circuit || actual == JourneyType.Cycle,
            JourneyType.Path => actual == JourneyType.Path || actual == JourneyType.Cycle,
            JourneyType.Circuit => actual == JourneyType.Circuit,
            JourneyType.Cycle => actual == JourneyType.Cycle,
            _ => actual == target
        };
    }

    private string GetTargetedFeedback(JourneyType actual, JourneyType target, JourneyTypeConfig config)
    {
        if (IsJourneyTypeValid(actual, target))
        {
            if (IsMissionComplete())
            {
                return config.successMessage;
            }
            else
            {
                return $"Good {target}! Continue to reach minimum length.";
            }
        }

        return GetCorrectionGuidance(actual, target, config);
    }

    private string GetCorrectionGuidance(JourneyType actual, JourneyType target, JourneyTypeConfig config)
    {
        var vertices = currentJourney.Select(step => step.vertexId).ToList();
        var edges = currentJourney.Where(step => !string.IsNullOrEmpty(step.edgeId))
                                 .Select(step => step.edgeId).ToList();

        bool hasRepeatedVertices = vertices.Count != vertices.Distinct().Count();
        bool hasRepeatedEdges = edges.Count != edges.Distinct().Count();
        bool returnsToStart = vertices.Count > 2 && vertices.First() == vertices.Last();

        return target switch
        {
            JourneyType.Trail when hasRepeatedEdges =>
                "You've repeated a bridge! For a trail, each bridge can only be crossed once.",

            JourneyType.Path when hasRepeatedVertices =>
                "You've revisited a vertex! For a path, each vertex can only be visited once.",

            JourneyType.Circuit when hasRepeatedEdges =>
                "You've repeated a bridge! For a circuit, avoid repeating bridges and return to start.",

            JourneyType.Circuit when !returnsToStart =>
                "Good trail! Now return to your starting vertex to complete the circuit.",

            JourneyType.Cycle when hasRepeatedVertices =>
                "You've revisited a vertex! For a cycle, visit each vertex once and return to start.",

            JourneyType.Cycle when !returnsToStart =>
                "Good path! Now return to your starting vertex to complete the cycle.",

            _ => config.correctionHint
        };
    }

    public string GetJourneyExplanation(JourneyType journeyType)
    {
        return journeyType switch
        {
            JourneyType.Walk => "A walk is any sequence of connected vertices and edges - complete freedom of movement.",
            JourneyType.Trail => "A trail is a walk where no edge (bridge) is repeated - vertices can be revisited.",
            JourneyType.Path => "A path is a walk where no vertex is repeated - the most restrictive journey.",
            JourneyType.Circuit => "A circuit is a trail that returns to its starting vertex - closed trail.",
            JourneyType.Cycle => "A cycle is a path that returns to its starting vertex - closed path.",
            _ => "This journey type is not recognized."
        };
    }

    public void RegisterJourneyType(JourneyType type, JourneyTypeConfig config)
    {
        journeyConfigs[type] = config;
    }

    public int GetCurrentJourneyLength()
    {
        return currentJourney.Count;
    }

    public JourneyType GetCurrentJourneyType()
    {
        return AnalyzeCurrentJourney();
    }

    public JourneyType GetTargetJourneyType()
    {
        return targetJourneyType;
    }
}