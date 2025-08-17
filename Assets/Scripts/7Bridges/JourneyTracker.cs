using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// ==============================================
// CORE DATA STRUCTURES
// ==============================================

/// <summary>
/// Notes: we get errors with colors: gold, orange 
/// </summary>

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

public class JourneyTracker : MonoBehaviour, IPuzzleSystem
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI journeyStatusText;
    [SerializeField] private TextMeshProUGUI journeyHistoryText;
    [SerializeField] private TextMeshProUGUI missionObjectiveText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Mission Configuration")]
    [SerializeField] private JourneyType targetJourneyType = JourneyType.Walk;

    private List<JourneyStep> currentJourney = new List<JourneyStep>();
    private string startingVertex = null;
    private Dictionary<JourneyType, JourneyTypeConfig> journeyConfigs;

    public bool IsActive => true;

    void Awake()
    {
        InitializeJourneyConfigs();
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

        UpdateUI();
    }

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

    public void OnVertexVisited(IVertex vertex, ICrosser crosser)
    {
        var step = new JourneyStep(vertex.VertexId);

        if (currentJourney.Count == 0)
        {
            startingVertex = vertex.VertexId;
        }

        currentJourney.Add(step);
        UpdateUI();
    }

    public void OnBridgeCrossed(IBridge bridge, ICrosser crosser)
    {
        if (currentJourney.Count > 0)
        {
            var lastStep = currentJourney[currentJourney.Count - 1];
            lastStep.edgeId = bridge.BridgeId;
        }

        UpdateUI();
    }

    private JourneyType AnalyzeCurrentJourney()
    {
        if (currentJourney.Count < 2)
        {
            return JourneyType.Walk;
        }

        var vertices = currentJourney.Select(step => step.vertexId).ToList();
        var edges = currentJourney.Where(step => !string.IsNullOrEmpty(step.edgeId))
                                 .Select(step => step.edgeId).ToList();

        bool returnsToStart = vertices.Count > 2 && vertices.First() == vertices.Last();

        if (returnsToStart)
        {
            var pathVertices = vertices.Take(vertices.Count - 1).ToList();
            bool hasRepeatedVertices = pathVertices.Count != pathVertices.Distinct().Count();
            bool hasRepeatedEdges = edges.Count != edges.Distinct().Count();

            if (!hasRepeatedVertices && !hasRepeatedEdges)
            {
                return JourneyType.Cycle;
            }
            else if (!hasRepeatedEdges)
            {
                return JourneyType.Circuit;
            }
        }

        bool noRepeatedVertices = vertices.Count == vertices.Distinct().Count();
        if (noRepeatedVertices)
        {
            return JourneyType.Path;
        }

        bool noRepeatedEdges = edges.Count == edges.Distinct().Count();
        if (noRepeatedEdges)
        {
            return JourneyType.Trail;
        }

        return JourneyType.Walk;
    }

    private void UpdateUI()
    {
        var config = GetCurrentConfig();

        // Always show path history
        UpdatePathHistory();

        // Update mission objective
        UpdateMissionObjective(config);

        // Provide appropriate feedback based on progress
        if (currentJourney.Count < config.minimumStepsForClassification)
        {
            ShowProgressFeedback(config);
        }
        else
        {
            ShowClassificationFeedback(config);
        }
    }

    private JourneyTypeConfig GetCurrentConfig()
    {
        return journeyConfigs.TryGetValue(targetJourneyType, out var config)
            ? config
            : journeyConfigs[JourneyType.Invalid];
    }

    private void UpdatePathHistory()
    {
        if (journeyHistoryText != null)
        {
            var history = string.Join(" → ", currentJourney.Select(step => step.vertexId));
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
                missionObjectiveText.text = $"✓ COMPLETE: {config.successMessage}";
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
        // Check if the actual journey satisfies the target requirements
        return target switch
        {
            JourneyType.Walk => true,  // Any journey is a valid walk
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

        // Provide specific guidance based on what went wrong
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

    // Public interface methods
    public void ResetJourney()
    {
        currentJourney.Clear();
        startingVertex = null;
        UpdateUI();
    }

    public void SetMissionType(JourneyType newTarget)
    {
        targetJourneyType = newTarget;
        UpdateUI();
    }

    public string GetJourneyExplanation(JourneyType journeyType)
    {
        var config = journeyConfigs.TryGetValue(journeyType, out var value)
            ? value
            : journeyConfigs[JourneyType.Invalid];

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

    // Helper method to add new journey types easily
    public void RegisterJourneyType(JourneyType type, JourneyTypeConfig config)
    {
        journeyConfigs[type] = config;
    }

    // Public method for mission manager integration
    public bool IsMissionComplete()
    {
        var config = GetCurrentConfig();
        if (currentJourney.Count < config.minimumStepsForCompletion)
            return false;

        var actualType = AnalyzeCurrentJourney();
        return IsJourneyTypeValid(actualType, targetJourneyType);
    }

    // Additional helper methods for external systems
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