// TutorialUIManager.cs
// Manages UI display and feedback for tutorial system using Observer pattern

using UnityEngine;
using TMPro;

/// <summary>
/// Handles all tutorial UI updates and visual feedback
/// Implements ITutorialUI interface for decoupled communication
/// </summary>
public class TutorialUIManager : MonoBehaviour, ITutorialUI
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private GameObject exitButtonGroup;

    /// <summary>
    /// Update the progress message with current tutorial feedback
    /// </summary>
    public void UpdateProgressMessage(string message)
    {
        if (progressText != null)
        {
            progressText.text = message;
            progressText.color = Color.yellow;
        }
    }

    /// <summary>
    /// Update the status message with step counts and journey classification
    /// </summary>
    public void UpdateStatusMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = Color.cyan;
        }
    }

    /// <summary>
    /// Display tutorial completion state and message
    /// </summary>
    public void ShowCompletion(string completionMessage)
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);

        if (completionText != null)
        {
            completionText.text = completionMessage;
            completionText.color = Color.green;
        }

        if (progressText != null)
        {
            progressText.text = "Tutorial Complete!";
            progressText.color = Color.green;
        }
    }

    /// <summary>
    /// Show exit options when tutorial is completed
    /// </summary>
    public void ShowExitOption()
    {
        if (exitButtonGroup != null)
            exitButtonGroup.SetActive(true);
    }

    /// <summary>
    /// Set the main instruction message for the current tutorial
    /// </summary>
    public void SetInstructionMessage(string instruction)
    {
        if (instructionText != null)
        {
            instructionText.text = instruction;
            instructionText.color = Color.white;
        }
    }

    /// <summary>
    /// Hide completion UI elements
    /// </summary>
    public void HideCompletion()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (exitButtonGroup != null)
            exitButtonGroup.SetActive(false);
    }

    /// <summary>
    /// Reset UI to initial tutorial state
    /// </summary>
    public void ResetUI()
    {
        HideCompletion();

        if (progressText != null)
        {
            progressText.color = Color.yellow;
        }

        if (statusText != null)
        {
            statusText.color = Color.cyan;
        }
    }
}