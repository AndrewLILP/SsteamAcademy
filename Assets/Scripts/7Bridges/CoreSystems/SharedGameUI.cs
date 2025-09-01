// SharedGameUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles shared UI elements that both Tutorial and Mission modes use
/// Includes: Back button, journey display, notifications, common controls
/// </summary>
public class SharedGameUI : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button resetJourneyButton;

    [Header("Journey Display")]
    [SerializeField] private TextMeshProUGUI journeyHistoryText;
    [SerializeField] private TextMeshProUGUI currentPositionText;

    [Header("Notifications")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI bridgeNotificationText;

    [Header("Completion")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private Button continueButton;

    private GameManager gameManager;
    private JourneyTracker journeyTracker;

    void Start()
    {
        // Find required references
        gameManager = FindFirstObjectByType<GameManager>();
        journeyTracker = FindFirstObjectByType<JourneyTracker>();

        // Setup buttons
        SetupButtons();

        // Setup journey tracking
        SetupJourneyTracking();

        Debug.Log("[SharedGameUI] Initialized");
    }

    void OnDestroy()
    {
        // Cleanup listeners
        CleanupJourneyTracking();
    }

    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtons()
    {
        // Back to menu button
        if (backToMenuButton != null && gameManager != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(() => {
                Debug.Log("[SharedGameUI] Back to menu clicked");
                gameManager.ReturnToModeSelection();
            });
        }

        // Reset journey button
        if (resetJourneyButton != null && journeyTracker != null)
        {
            resetJourneyButton.onClick.RemoveAllListeners();
            resetJourneyButton.onClick.AddListener(() => {
                Debug.Log("[SharedGameUI] Reset journey clicked");
                journeyTracker.ResetJourney();
                ShowNotification("Journey reset!", 2f);
            });
        }

        // Continue button (for completion panel)
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => {
                Debug.Log("[SharedGameUI] Continue clicked");
                HideCompletion();
            });
        }
    }

    /// <summary>
    /// Setup journey tracking listeners
    /// </summary>
    private void SetupJourneyTracking()
    {
        if (journeyTracker == null) return;

        // Listen to journey updates (we'll create a simple event system)
        // For now, we'll update in Update() method
        // Later we can add events to JourneyTracker if needed
    }

    /// <summary>
    /// Cleanup journey tracking listeners
    /// </summary>
    private void CleanupJourneyTracking()
    {
        // Remove any event listeners here when we add them
    }

    void Update()
    {
        // Update journey display
        UpdateJourneyDisplay();
    }

    /// <summary>
    /// Update the journey history and current position display
    /// </summary>
    private void UpdateJourneyDisplay()
    {
        if (journeyTracker == null) return;

        // Update journey history
        if (journeyHistoryText != null)
        {
            var journeyLength = journeyTracker.GetCurrentJourneyLength();
            var journeyType = journeyTracker.GetCurrentJourneyType();

            if (journeyLength > 0)
            {
                journeyHistoryText.text = $"Journey: {journeyType} ({journeyLength} steps)";
            }
            else
            {
                journeyHistoryText.text = "Journey: Not started";
            }
        }

        // Update current position (simplified)
        if (currentPositionText != null)
        {
            var journeyLength = journeyTracker.GetCurrentJourneyLength();
            if (journeyLength > 0)
            {
                currentPositionText.text = $"Current Step: {journeyLength}";
            }
            else
            {
                currentPositionText.text = "Ready to start";
            }
        }
    }

    /// <summary>
    /// Show a temporary notification message
    /// </summary>
    public void ShowNotification(string message, float duration = 3f)
    {
        if (notificationText == null) return;

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Hide after duration
        Invoke(nameof(HideNotification), duration);

        Debug.Log($"[SharedGameUI] Notification: {message}");
    }

    /// <summary>
    /// Hide notification text
    /// </summary>
    private void HideNotification()
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show bridge crossing notification
    /// </summary>
    public void ShowBridgeNotification(string bridgeId)
    {
        if (bridgeNotificationText == null) return;

        bridgeNotificationText.text = $"Crossed Bridge: {bridgeId}";
        bridgeNotificationText.gameObject.SetActive(true);

        // Hide after 2 seconds
        Invoke(nameof(HideBridgeNotification), 2f);
    }

    /// <summary>
    /// Hide bridge notification
    /// </summary>
    private void HideBridgeNotification()
    {
        if (bridgeNotificationText != null)
        {
            bridgeNotificationText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show completion panel with message
    /// </summary>
    public void ShowCompletion(string message)
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);

        if (completionText != null)
            completionText.text = message;

        Debug.Log($"[SharedGameUI] Showing completion: {message}");
    }

    /// <summary>
    /// Hide completion panel
    /// </summary>
    public void HideCompletion()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    /// <summary>
    /// Enable/disable back button
    /// </summary>
    public void SetBackButtonEnabled(bool enabled)
    {
        if (backToMenuButton != null)
            backToMenuButton.interactable = enabled;
    }

    /// <summary>
    /// Enable/disable reset button
    /// </summary>
    public void SetResetButtonEnabled(bool enabled)
    {
        if (resetJourneyButton != null)
            resetJourneyButton.interactable = enabled;
    }

    /// <summary>
    /// Update journey display text manually (called by other systems)
    /// </summary>
    public void UpdateJourneyText(string historyText, string positionText = "")
    {
        if (journeyHistoryText != null)
            journeyHistoryText.text = historyText;

        if (currentPositionText != null && !string.IsNullOrEmpty(positionText))
            currentPositionText.text = positionText;
    }

    // Public methods for bridge system integration

    /// <summary>
    /// Called when bridge is crossed (connect to bridge events)
    /// </summary>
    public void OnBridgeCrossed(string bridgeId)
    {
        ShowBridgeNotification(bridgeId);
    }

    /// <summary>
    /// Called when vertex is visited
    /// </summary>
    public void OnVertexVisited(string vertexId)
    {
        // Could show vertex notifications if desired
        // ShowNotification($"Visited: {vertexId}", 1f);
    }
}