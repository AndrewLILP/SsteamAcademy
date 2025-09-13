// EulerNPC.cs
// Simple Euler NPC for story mode
using UnityEngine;

/// <summary>
/// Simple Euler NPC that can appear and disappear
/// Follows Single Responsibility Principle - just manages character visibility
/// </summary>
public class EulerNPC : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private string npcName = "Leonhard Euler";
    [SerializeField] private GameObject characterModel;

    [Header("Appearance Settings")]
    [SerializeField] private float appearDuration = 1f;
    [SerializeField] private bool startVisible = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // State
    private bool isVisible = false;

    void Start()
    {
        InitializeNPC();
    }

    /// <summary>
    /// Initialize NPC
    /// </summary>
    private void InitializeNPC()
    {
        // Find character model if not assigned
        if (characterModel == null && transform.childCount > 0)
        {
            characterModel = transform.GetChild(0).gameObject;
        }

        // Set initial visibility
        SetVisibility(startVisible);

        LogDebug($"{npcName} NPC initialized");
    }

    /// <summary>
    /// Make Euler appear
    /// </summary>
    public void Appear()
    {
        if (isVisible) return;

        SetVisibility(true);
        LogDebug($"{npcName} appeared");
    }

    /// <summary>
    /// Make Euler disappear
    /// </summary>
    public void Disappear()
    {
        if (!isVisible) return;

        SetVisibility(false);
        LogDebug($"{npcName} disappeared");
    }

    /// <summary>
    /// Set NPC visibility
    /// </summary>
    private void SetVisibility(bool visible)
    {
        isVisible = visible;

        if (characterModel != null)
        {
            characterModel.SetActive(visible);
        }
        else
        {
            // If no model, just enable/disable this GameObject
            gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Move NPC to position (simple teleport)
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        transform.position = position;
        LogDebug($"{npcName} moved to {position}");
    }

    // Public properties
    public bool IsVisible => isVisible;
    public string NPCName => npcName;
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[EulerNPC] {message}");
    }

    // Manual testing methods for inspector
    [ContextMenu("Test Appear")]
    public void TestAppear()
    {
        Appear();
    }

    [ContextMenu("Test Disappear")]
    public void TestDisappear()
    {
        Disappear();
    }

    // Gizmo for positioning in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isVisible ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.5f);
    }
}