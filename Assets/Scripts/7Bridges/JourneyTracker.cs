using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

// ==============================================
// JOURNEY TRACKING SYSTEM
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

public class JourneyTracker : MonoBehaviour, IPuzzleSystem
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI journeyTypeText;
    [SerializeField] private TextMeshProUGUI journeyHistoryText;
    [SerializeField] private TextMeshProUGUI missionObjectiveText;

    [Header("Mission Configuration")]
    [SerializeField] private JourneyType targetJourneyType = JourneyType.Trail;
    [SerializeField] private bool requireMinimumSteps = true;
    [SerializeField] private int minimumSteps = 3;

    private List<JourneyStep> currentJourney = new List<JourneyStep>();
    private string startingVertex = null;

    public bool IsActive => true;

    void Start()
    {
        // Register with all vertices
        var vertices = FindObjectsByType<Vertex>(FindObjectsSortMode.None);
        foreach (var vertex in vertices)
        {
            vertex.OnVertexVisited += OnVertexVisited;
        }

        // Register with all bridges  
        var bridges = FindObjectsByType<Bridge>(FindObjectsSortMode.None);
        foreach (var bridge in bridges)
        {
            bridge.OnBridgeCrossed += OnBridgeCrossed;
        }

        UpdateUI();
    }

    public void OnVertexVisited(IVertex vertex, ICrosser crosser)
    {
        // Record the vertex visit
        var step = new JourneyStep(vertex.VertexId);

        // Set starting vertex for the first visit
        if (currentJourney.Count == 0)
        {
            startingVertex = vertex.VertexId;
        }

        currentJourney.Add(step);
        AnalyzeCurrentJourney();
        UpdateUI();
    }

    public void OnBridgeCrossed(IBridge bridge, ICrosser crosser)
    {
        // Update the last journey step with the bridge info
        if (currentJourney.Count > 0)
        {
            var lastStep = currentJourney[currentJourney.Count - 1];
            lastStep.edgeId = bridge.BridgeId;
        }

        AnalyzeCurrentJourney();
        UpdateUI();
    }

    private JourneyType AnalyzeCurrentJourney()
    {
        if (currentJourney.Count < 2)
        {
            return JourneyType.Walk; // Single vertex is just a walk
        }

        var vertices = currentJourney.Select(step => step.vertexId).ToList();
        var edges = currentJourney.Where(step => !string.IsNullOrEmpty(step.edgeId))
                                 .Select(step => step.edgeId).ToList();

        // Check for cycles and circuits first (they return to start)
        bool returnsToStart = vertices.Count > 2 && vertices.First() == vertices.Last();

        if (returnsToStart)
        {
            // Remove the duplicate starting vertex for analysis
            var pathVertices = vertices.Take(vertices.Count - 1).ToList();
            bool hasRepeatedVertices = pathVertices.Count != pathVertices.Distinct().Count();
            bool hasRepeatedEdges = edges.Count != edges.Distinct().Count();

            if (!hasRepeatedVertices && !hasRepeatedEdges)
            {
                return JourneyType.Cycle; // Path that returns to start
            }
            else if (!hasRepeatedEdges)
            {
                return JourneyType.Circuit; // Trail that returns to start
            }
        }

        // Check for path (no repeated vertices)
        bool noRepeatedVertices = vertices.Count == vertices.Distinct().Count();
        if (noRepeatedVertices)
        {
            return JourneyType.Path;
        }

        // Check for trail (no repeated edges)
        bool noRepeatedEdges = edges.Count == edges.Distinct().Count();
        if (noRepeatedEdges)
        {
            return JourneyType.Trail;
        }

        // Default to walk
        return JourneyType.Walk;
    }

    private void UpdateUI()
    {
        var currentType = AnalyzeCurrentJourney();

        if (journeyTypeText != null)
        {
            journeyTypeText.text = $"Current Journey: {currentType}";

            // Color coding for feedback
            journeyTypeText.color = currentType == targetJourneyType ? Color.green : Color.white;
        }

        if (journeyHistoryText != null)
        {
            var history = string.Join(" → ", currentJourney.Select(step => step.vertexId));
            journeyHistoryText.text = $"Path: {history}";
        }

        if (missionObjectiveText != null)
        {
            missionObjectiveText.text = $"Mission: Create a {targetJourneyType}";

            // Check if mission is complete
            if (IsMissionComplete())
            {
                missionObjectiveText.text += " ✓ COMPLETE!";
                missionObjectiveText.color = Color.green;
            }
        }
    }

    private bool IsMissionComplete()
    {
        var currentType = AnalyzeCurrentJourney();
        bool correctType = currentType == targetJourneyType;
        bool enoughSteps = !requireMinimumSteps || currentJourney.Count >= minimumSteps;

        return correctType && enoughSteps;
    }

    // Public methods for mission control
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

    // Method to provide educational explanations
    public string GetJourneyExplanation(JourneyType journeyType)
    {
        return journeyType switch
        {
            JourneyType.Walk => "A walk is any sequence of connected vertices and edges.",
            JourneyType.Trail => "A trail is a walk where no edge is repeated.",
            JourneyType.Path => "A path is a walk where no vertex is repeated.",
            JourneyType.Circuit => "A circuit is a trail that ends where it started.",
            JourneyType.Cycle => "A cycle is a path that ends where it started.",
            _ => "This doesn't form a valid mathematical journey type."
        };
    }
}