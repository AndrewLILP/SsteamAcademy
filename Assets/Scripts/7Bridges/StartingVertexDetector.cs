using UnityEngine;

// vertex not detected in new mission (eg journey/mission 2) - bridge needs to be crossed.

public class StartingVertexDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    private JourneyTracker journeyTracker;
    private bool hasDetectedStartingVertex = false;

    void Start()
    {
        journeyTracker = FindFirstObjectByType<JourneyTracker>();
        DetectStartingVertex();
    }

    void DetectStartingVertex()
    {
        if (hasDetectedStartingVertex) return;

        // Find the closest vertex to player's starting position
        var vertices = FindObjectsByType<Vertex>(FindObjectsSortMode.None);
        Vertex closestVertex = null;
        float closestDistance = float.MaxValue;

        foreach (var vertex in vertices)
        {
            float distance = Vector3.Distance(transform.position, vertex.Position);
            if (distance < closestDistance && distance <= detectionRadius)
            {
                closestDistance = distance;
                closestVertex = vertex;
            }
        }

        if (closestVertex != null)
        {
            // Manually trigger the first vertex visit
            journeyTracker?.OnVertexVisited(closestVertex, GetComponent<ICrosser>());
            hasDetectedStartingVertex = true;
            Debug.Log($"Starting vertex detected: {closestVertex.VertexId}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}