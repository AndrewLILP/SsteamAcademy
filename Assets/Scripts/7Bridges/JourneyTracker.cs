using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using System;

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

    [Header("Celebration Settings")]
    [SerializeField] private float celebrationDelay = 0.5f;
    [SerializeField] private AudioClip completionSound;
    [SerializeField] private ParticleSystem celebrationParticles;

    private List<JourneyStep> currentJourney = new List<JourneyStep>();
    private string startingVertex = null;
    private bool missionComplete = false;

    // Events for celebration triggers
    public event Action<JourneyType> OnJourneyTypeChanged;
    public event Action<JourneyType, int> OnMissionCompleted;
    public event Action<JourneyStep> OnStepAdded;

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

    // ==============================================
    // PUBLIC METHODS FOR UI ACCESS
    // ==============================================

    /// <summary>
    /// Gets the current journey type based on the path taken
    /// </summary>
    public JourneyType GetCurrentJourneyType()
    {
        return AnalyzeCurrentJourney();
    }

    /// <summary>
    /// Gets the current length of the journey (number of steps)
    /// </summary>
    public int GetJourneyLength()
    {
        return currentJourney.Count;
    }

    /// <summary>
    /// Gets the number of unique vertices visited
    /// </summary>
    public int GetUniqueVertexCount()
    {
        return currentJourney.Select(step => step.vertexId).Distinct().Count();
    }

    /// <summary>
    /// Gets the number of unique edges crossed
    /// </summary>
    public int GetUniqueEdgeCount()
    {
        return currentJourney.Where(step => !string.IsNullOrEmpty(step.edgeId))
                           .Select(step => step.edgeId).Distinct().Count();
    }

    /// <summary>
    /// Gets a list of all vertices visited in order
    /// </summary>
    public List<string> GetVertexSequence()
    {
        return currentJourney.Select(step => step.vertexId).ToList();
    }

    /// <summary>
    /// Gets a list of all edges crossed in order
    /// </summary>
    public List<string> GetEdgeSequence()
    {
        return currentJourney.Where(step => !string.IsNullOrEmpty(step.edgeId))
                           .Select(step => step.edgeId).ToList();
    }

    /// <summary>
    /// Checks if the current mission is complete
    /// </summary>
    public bool IsMissionComplete()
    {
        var currentType = GetCurrentJourneyType();
        bool correctType = currentType == targetJourneyType;
        bool enoughSteps = !requireMinimumSteps || GetJourneyLength() >= minimumSteps;

        return correctType && enoughSteps && !missionComplete;
    }

    /// <summary>
    /// Gets the target journey type for the current mission
    /// </summary>
    public JourneyType GetTargetJourneyType()
    {
        return targetJourneyType;
    }

    /// <summary>
    /// Gets progress towards minimum steps requirement
    /// </summary>
    public float GetStepProgress()
    {
        if (!requireMinimumSteps) return 1f;
        return Mathf.Clamp01((float)GetJourneyLength() / minimumSteps);
    }

    // ==============================================
    // JOURNEY TRACKING CORE METHODS
    // ==============================================

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

        // Trigger events
        OnStepAdded?.Invoke(step);

        var previousType = currentJourney.Count > 1 ? AnalyzeJourney(currentJourney.Take(currentJourney.Count - 1).ToList()) : JourneyType.Walk;
        var currentType = AnalyzeCurrentJourney();

        if (currentType != previousType)
        {
            OnJourneyTypeChanged?.Invoke(currentType);
        }

        CheckForMissionCompletion();
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

        CheckForMissionCompletion();
        UpdateUI();
    }

    // ==============================================
    // CELEBRATION TRIGGERS
    // ==============================================

    private void CheckForMissionCompletion()
    {
        if (IsMissionComplete())
        {
            missionComplete = true;
            StartCoroutine(TriggerMissionCompletion());
        }
    }

    private System.Collections.IEnumerator TriggerMissionCompletion()
    {
        yield return new WaitForSeconds(celebrationDelay);

        var completedType = GetCurrentJourneyType();
        var journeyLength = GetJourneyLength();

        // Trigger celebration event
        OnMissionCompleted?.Invoke(completedType, journeyLength);

        // Visual celebration
        TriggerVisualCelebration();

        // Audio celebration
        TriggerAudioCelebration();

        Debug.Log($"🎉 Mission Complete! Successfully created a {completedType} with {journeyLength} steps!");
    }

    private void TriggerVisualCelebration()
    {
        // Particle effects
        if (celebrationParticles != null)
        {
            celebrationParticles.Play();
        }

        // UI celebration (could trigger animations in UI manager)
        if (journeyTypeText != null)
        {
            StartCoroutine(CelebrationTextEffect());
        }
    }

    private void TriggerAudioCelebration()
    {
        if (completionSound != null)
        {
            AudioSource.PlayClipAtPoint(completionSound, Camera.main.transform.position);
        }
    }

    private System.Collections.IEnumerator CelebrationTextEffect()
    {
        var originalColor = journeyTypeText.color;
        var celebrationColor = Color.green;

        // Flash green
        journeyTypeText.color = celebrationColor;
        yield return new WaitForSeconds(0.5f);

        // Fade back to original
        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            journeyTypeText.color = Color.Lerp(celebrationColor, originalColor, t);
            yield return null;
        }

        journeyTypeText.color = originalColor;
    }

    // ==============================================
    // ANALYSIS METHODS
    // ==============================================

    private JourneyType AnalyzeCurrentJourney()
    {
        return AnalyzeJourney(currentJourney);
    }

    private JourneyType AnalyzeJourney(IEnumerable<JourneyStep> journey)
    {
        var journeyList = journey.ToList();

        if (journeyList.Count < 2)
        {
            return JourneyType.Walk; // Single vertex is just a walk
        }

        var vertices = journeyList.Select(step => step.vertexId).ToList();
        var edges = journeyList.Where(step => !string.IsNullOrEmpty(step.edgeId))
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

    // ==============================================
    // UI METHODS
    // ==============================================

    private void UpdateUI()
    {
        var currentType = GetCurrentJourneyType();

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

            // Progress indicator
            if (requireMinimumSteps)
            {
                missionObjectiveText.text += $" ({GetJourneyLength()}/{minimumSteps} steps)";
            }

            // Check if mission is complete
            if (missionComplete)
            {
                missionObjectiveText.text += " ✓ COMPLETE!";
                missionObjectiveText.color = Color.green;
            }
        }
    }

    // ==============================================
    // PUBLIC CONTROL METHODS
    // ==============================================

    /// <summary>
    /// Resets the current journey
    /// </summary>
    public void ResetJourney()
    {
        currentJourney.Clear();
        startingVertex = null;
        missionComplete = false;
        UpdateUI();
        Debug.Log("Journey reset");
    }

    /// <summary>
    /// Sets a new mission type and resets progress
    /// </summary>
    public void SetMissionType(JourneyType newTarget, int minSteps = 3)
    {
        targetJourneyType = newTarget;
        minimumSteps = minSteps;
        missionComplete = false;
        UpdateUI();
        Debug.Log($"Mission set to: {newTarget}");
    }

    /// <summary>
    /// Provides educational explanations for journey types
    /// </summary>
    public string GetJourneyExplanation(JourneyType journeyType)
    {
        return journeyType switch
        {
            JourneyType.Walk => "A walk is any sequence of connected vertices and edges. You can repeat vertices and edges freely.",
            JourneyType.Trail => "A trail is a walk where no edge is repeated. You can revisit vertices, but can't cross the same bridge twice.",
            JourneyType.Path => "A path is a walk where no vertex is repeated. Each location can only be visited once.",
            JourneyType.Circuit => "A circuit is a trail that ends where it started. Like a trail, but you return to your starting point.",
            JourneyType.Cycle => "A cycle is a path that ends where it started. Like a path, but you return home without repeating any stops.",
            _ => "This doesn't form a valid mathematical journey type."
        };
    }

    /// <summary>
    /// Gets detailed journey statistics for debugging or advanced UI
    /// </summary>
    public string GetJourneyStats()
    {
        return $"Journey Stats:\n" +
               $"• Type: {GetCurrentJourneyType()}\n" +
               $"• Length: {GetJourneyLength()} steps\n" +
               $"• Unique Vertices: {GetUniqueVertexCount()}\n" +
               $"• Unique Edges: {GetUniqueEdgeCount()}\n" +
               $"• Mission Complete: {IsMissionComplete()}";
    }
}