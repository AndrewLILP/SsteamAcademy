// DialogueSystem.cs
// Simple dialogue system for story mode narrative
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles dialogue display for story mode
/// Follows Single Responsibility Principle - only manages dialogue presentation
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Button continueButton;

    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool enableTypewriter = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // State
    private bool isTyping = false;
    private bool dialogueActive = false;
    private string currentDialogue = "";
    private Coroutine typingCoroutine;

    // Events
    public System.Action OnDialogueComplete;

    void Start()
    {
        InitializeDialogueSystem();
    }

    /// <summary>
    /// Initialize dialogue system and setup UI
    /// </summary>
    private void InitializeDialogueSystem()
    {
        // Find components if not assigned
        if (continueButton == null)
            continueButton = GetComponentInChildren<Button>();

        if (dialogueText == null)
            dialogueText = GetComponentInChildren<TextMeshProUGUI>();

        // Setup button listener
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // Hide dialogue initially
        HideDialogue();

        LogDebug("DialogueSystem initialized");
    }

    /// <summary>
    /// Show dialogue with speaker name
    /// </summary>
    public void ShowDialogue(string dialogue, string speaker = "Narrator")
    {
        if (string.IsNullOrEmpty(dialogue))
        {
            LogDebug("Empty dialogue provided");
            return;
        }

        currentDialogue = dialogue;
        dialogueActive = true;

        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Set speaker name
        if (speakerNameText != null)
        {
            speakerNameText.text = speaker;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }

        // Start typing effect or show immediately
        if (enableTypewriter && dialogueText != null)
        {
            StartTypingEffect(dialogue);
        }
        else if (dialogueText != null)
        {
            dialogueText.text = dialogue;
        }

        // Show continue button
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        LogDebug($"Showing dialogue: {dialogue.Substring(0, Mathf.Min(30, dialogue.Length))}...");
    }

    /// <summary>
    /// Hide dialogue panel
    /// </summary>
    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        dialogueActive = false;

        // Stop typing if active
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    /// <summary>
    /// Handle continue button click
    /// </summary>
    private void OnContinueClicked()
    {
        if (isTyping)
        {
            // Skip to end of typing
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentDialogue;
            isTyping = false;
        }
        else
        {
            // Complete dialogue
            CompleteDialogue();
        }
    }

    /// <summary>
    /// Complete current dialogue
    /// </summary>
    private void CompleteDialogue()
    {
        HideDialogue();
        OnDialogueComplete?.Invoke();
        LogDebug("Dialogue completed");
    }

    /// <summary>
    /// Typewriter effect coroutine
    /// </summary>
    private void StartTypingEffect(string dialogue)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(dialogue));
    }

    /// <summary>
    /// Type text character by character
    /// </summary>
    private IEnumerator TypeText(string dialogue)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < dialogue.Length; i++)
        {
            dialogueText.text += dialogue[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    // Input handling
    void Update()
    {
        if (dialogueActive)
        {
            // Space or Enter to continue
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnContinueClicked();
            }

            // ESC to close dialogue
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CompleteDialogue();
            }
        }
    }

    // Public properties
    public bool IsDialogueActive => dialogueActive;

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[DialogueSystem] {message}");
    }
}