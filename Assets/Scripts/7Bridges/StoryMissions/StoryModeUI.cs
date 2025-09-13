// StoryModeUI.cs
// Simple UI for story mode showing narrative progress
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple UI component for story mode
/// Shows narrative progress instead of step counters
/// </summary>
public class StoryModeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TextMeshProUGUI storyProgressText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button menuButton;

    [Header("Settings")]
    [SerializeField] private bool enableDebugLogging = true;

    // Dependencies
    private StoryManager storyManager;
    private GameManager gameManager;

    void Start()
    {
        InitializeUI();
    }

    void Update()
    {
        UpdateProgressDisplay();
    }

    /// <summary>
    /// Initialize UI components and find dependencies
    /// </summary>
    private void InitializeUI()
    {
        // Find dependencies
        storyManager = FindFirstObjectByType<StoryManager>();
        gameManager = GameManager.Instance;

        // Setup menu button
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(ReturnToMenu);
        }

        // Show story panel
        if (storyPanel != null)
            storyPanel.SetActive(true);

        // Set initial hint
        if (hintText != null)
            hintText.text = "Explore the city by walking to different areas. Press ESC to return to menu.";

        LogDebug("StoryModeUI initialized");
    }

    /// <summary>
    /// Update story progress display
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (storyManager == null || storyProgressText == null) return;

        string progressText = storyManager.GetStoryProgress();

        // Update progress text
        storyProgressText.text = progressText;

        // Change color based on progress
        if (progressText.Contains("complete"))
        {
            storyProgressText.color = Color.green;
        }
        else if (progressText.Contains("Euler"))
        {
            storyProgressText.color = Color.cyan;
        }
        else
        {
            storyProgressText.color = Color.white;
        }
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        LogDebug("Menu button clicked");

        if (gameManager != null)
        {
            gameManager.ReturnToMainMenu();
        }
        else
        {
            // Fallback
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Hide UI during dialogue sequences
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        if (storyPanel != null)
            storyPanel.SetActive(visible);
    }

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[StoryModeUI] {message}");
    }
}