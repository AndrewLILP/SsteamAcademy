using UnityEngine;
using System;

// ==============================================
// VERTEX DETECTION SYSTEM
// ==============================================

public interface IVertex
{
    string VertexId { get; }
    Vector3 Position { get; }
    event Action<IVertex, ICrosser> OnVertexVisited;
}

public class Vertex : MonoBehaviour, IVertex
{
    [SerializeField] private string vertexId;

    public string VertexId => vertexId;
    public Vector3 Position => transform.position;

    public event Action<IVertex, ICrosser> OnVertexVisited;

    private void Awake()
    {
        if (string.IsNullOrEmpty(vertexId))
        {
            vertexId = $"Vertex_{GetInstanceID()}";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var crosser = other.GetComponent<ICrosser>();
        if (crosser != null)
        {
            OnVertexVisited?.Invoke(this, crosser);
            Debug.Log($"Player visited vertex: {VertexId}");
        }
    }

    // Visual feedback
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

// Factory for creating vertices
public class VertexFactory
{
    public static GameObject CreateVertex(string vertexId, Vector3 position, float detectionRadius = 1f)
    {
        var vertexObj = new GameObject($"Vertex_{vertexId}");
        vertexObj.transform.position = position;

        var vertex = vertexObj.AddComponent<Vertex>();

        // Set the vertex ID via reflection or make it public
        var idField = typeof(Vertex).GetField("vertexId",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField?.SetValue(vertex, vertexId);

        var collider = vertexObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = detectionRadius;

        return vertexObj;
    }
}