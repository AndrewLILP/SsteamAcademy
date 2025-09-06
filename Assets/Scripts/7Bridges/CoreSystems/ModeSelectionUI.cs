// ModeSelectionUI.cs
// Complete version with comprehensive debugging for button click issues
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles the initial mode selection UI panel
/// Shows Tutorial/Mission mode buttons and basic progress info
/// Includes comprehensive debugging for UI interaction issues
/// </summary>
public class ModeSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button tutorialModeButton;
    [SerializeField] private Button missionModeButton;
    [SerializeField] private TextMeshProUGUI progressSummaryText;
    [SerializeField] private TextMeshProUGUI tutorialProgressText;
    [SerializeField] private TextMeshProUGUI missionProgressText;

    [Header("Optional References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Debug Options")]
    [SerializeField] private bool enableComprehensiveDebug = true;
    [SerializeField] private bool enableButtonClickLogging = true;

    private GameManager gameManager;
    private ProgressTracker progressTracker;

    void Start()
    {
        // Find required references
        gameManager = FindFirstObjectByType<GameManager>();
        progressTracker = FindFirstObjectByType<ProgressTracker>();

        if (gameManager == null)
        {
            Debug.LogError("ModeSelectionUI: GameManager not found!");
            return;
        }

        // Setup button listeners
        SetupButtons();

        // Setup UI text
        SetupInitialText();

        // Update progress display
        UpdateProgressDisplay();

        // Listen for progress updates
        if (progressTracker != null)
        {
            progressTracker.OnProgressUpdated += UpdateProgressDisplay;
        }

        // Run comprehensive debug if enabled
        if (enableComprehensiveDebug)
        {
            StartCoroutine(TestButtonSetup());
        }

        Debug.Log("[ModeSelectionUI] Initialized");
    }

    void Update()
    {
        // Temporary keyboard shortcuts for development
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[KEYBOARD] T pressed - Tutorial Mode");
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.SelectTutorialMode();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("[KEYBOARD] M pressed - Mission Mode");
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.SelectMissionMode();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[KEYBOARD] ESC pressed - Mode Selection");
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.ReturnToModeSelection();
            }
        }
    }

    void OnDestroy()
    {
        // Cleanup listeners
        if (progressTracker != null)
        {
            progressTracker.OnProgressUpdated -= UpdateProgressDisplay;
        }
    }

    private void SetupButtons()
    {
        // Ensure GameManager reference is always found when needed
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("[ModeSelectionUI] GameManager not found in scene!");
            return;
        }

        Debug.Log("[ModeSelectionUI] GameManager found and assigned");

        // Setup Tutorial Button with reliable reference
        if (tutorialModeButton != null)
        {
            tutorialModeButton.onClick.RemoveAllListeners();
            tutorialModeButton.onClick.AddListener(() => {
                if (enableButtonClickLogging)
                    Debug.Log("[ModeSelectionUI] Tutorial mode button clicked");

                // Find GameManager fresh each time to ensure reliability
                var gm = gameManager ?? FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.SelectTutorialMode();
                }
                else
                {
                    Debug.LogError("[ModeSelectionUI] GameManager is null when trying to select tutorial mode!");
                }
            });
            Debug.Log($"[ModeSelectionUI] Tutorial button configured (interactable: {tutorialModeButton.interactable})");
        }
        else
        {
            Debug.LogError("[ModeSelectionUI] Tutorial Mode Button not assigned in inspector!");
        }

        // Setup Mission Button with reliable reference
        if (missionModeButton != null)
        {
            missionModeButton.onClick.RemoveAllListeners();
            missionModeButton.onClick.AddListener(() => {
                if (enableButtonClickLogging)
                    Debug.Log("[ModeSelectionUI] Mission mode button clicked");

                // Find GameManager fresh each time to ensure reliability
                var gm = gameManager ?? FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.SelectMissionMode();
                }
                else
                {
                    Debug.LogError("[ModeSelectionUI] GameManager is null when trying to select mission mode!");
                }
            });
            Debug.Log($"[ModeSelectionUI] Mission button configured (interactable: {missionModeButton.interactable})");
        }
        else
        {
            Debug.LogError("[ModeSelectionUI] Mission Mode Button not assigned in inspector!");
        }
    }

    /// <summary>
    /// Setup initial text content
    /// </summary>
    private void SetupInitialText()
    {
        if (titleText != null)
        {
            titleText.text = "7 Bridges - Graph Theory Explorer";
        }

        if (instructionText != null)
        {
            instructionText.text = "Choose your learning mode:\n\n" +
                                 "• Tutorial Mode: Learn individual concepts (Walk, Trail, Path, etc.)\n" +
                                 "• Mission Mode: Complete guided learning missions";
        }
    }

    /// <summary>
    /// Update progress display texts
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressTracker == null) return;

        // Overall progress summary
        if (progressSummaryText != null)
        {
            progressSummaryText.text = progressTracker.GetProgressSummary();
        }

        // Tutorial progress details
        if (tutorialProgressText != null)
        {
            var completedTutorials = progressTracker.GetCompletedTutorials();
            var totalTutorials = progressTracker.GetTotalTutorialCount();

            string tutorialDetails = $"Tutorials Completed: {completedTutorials.Count}/{totalTutorials}\n";

            if (completedTutorials.Count > 0)
            {
                tutorialDetails += "✓ " + string.Join(" ✓ ", completedTutorials);
            }
            else
            {
                tutorialDetails += "No tutorials completed yet";
            }

            tutorialProgressText.text = tutorialDetails;
        }

        // Mission progress details
        if (missionProgressText != null)
        {
            var completedMissions = progressTracker.GetCompletedMissions();
            var missionCount = progressTracker.GetMissionCompletionCount();

            string missionDetails = $"Missions Completed: {missionCount}/5\n";

            if (completedMissions.Count > 0)
            {
                missionDetails += "Completed:\n";
                foreach (var mission in completedMissions)
                {
                    missionDetails += $"✓ {mission.missionName}\n";
                }
            }
            else
            {
                missionDetails += "No missions completed yet";
            }

            missionProgressText.text = missionDetails.TrimEnd('\n');
        }
    }

    /// <summary>
    /// Called when panel becomes active
    /// </summary>
    void OnEnable()
    {
        // Refresh progress when panel shows
        UpdateProgressDisplay();
    }

    /// <summary>
    /// Comprehensive debug system to identify UI interaction issues
    /// </summary>
    private IEnumerator TestButtonSetup()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("=== COMPREHENSIVE UI DEBUG ===");

        // Test 1: Button References
        Debug.Log($"[DEBUG] tutorialModeButton reference: {(tutorialModeButton != null ? "ASSIGNED" : "NULL")}");
        Debug.Log($"[DEBUG] missionModeButton reference: {(missionModeButton != null ? "ASSIGNED" : "NULL")}");

        if (tutorialModeButton != null)
        {
            Debug.Log($"[DEBUG] Tutorial button name: '{tutorialModeButton.gameObject.name}'");
            Debug.Log($"[DEBUG] Tutorial button path: {GetGameObjectPath(tutorialModeButton.gameObject)}");
        }

        if (missionModeButton != null)
        {
            Debug.Log($"[DEBUG] Mission button name: '{missionModeButton.gameObject.name}'");
            Debug.Log($"[DEBUG] Mission button path: {GetGameObjectPath(missionModeButton.gameObject)}");
        }

        // Test 2: Find ALL buttons in scene
        Debug.Log("=== ALL BUTTONS IN SCENE ===");
        var allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log($"[DEBUG] Total buttons found: {allButtons.Length}");

        for (int i = 0; i < allButtons.Length; i++)
        {
            var btn = allButtons[i];
            Debug.Log($"[DEBUG] Button {i}: '{btn.gameObject.name}' - Active: {btn.gameObject.activeInHierarchy} - Interactable: {btn.interactable}");
            Debug.Log($"[DEBUG] Button {i} path: {GetGameObjectPath(btn.gameObject)}");

            // Check if this button has text that suggests it's a mode button
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                Debug.Log($"[DEBUG] Button {i} text: '{text.text}'");
            }
        }

        // Test 3: Manual Button Test
        Debug.Log("=== MANUAL BUTTON TEST ===");
        foreach (var btn in allButtons)
        {
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null && (text.text.ToLower().Contains("tutorial") || text.text.ToLower().Contains("mission")))
            {
                Debug.Log($"[DEBUG] Found potential mode button: '{btn.gameObject.name}' with text: '{text.text}'");

                // Add a temporary click listener to this button for testing
                btn.onClick.AddListener(() => {
                    Debug.Log($"[MANUAL TEST] BUTTON CLICKED: '{btn.gameObject.name}' with text: '{text.text}'");
                });
            }
        }

        // Test 4: Canvas Groups (these can block interaction)
        Debug.Log("=== CANVAS GROUP TEST ===");
        var canvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        Debug.Log($"[DEBUG] Canvas groups found: {canvasGroups.Length}");

        foreach (var cg in canvasGroups)
        {
            Debug.Log($"[DEBUG] CanvasGroup '{cg.gameObject.name}': interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}, alpha={cg.alpha}");
            Debug.Log($"[DEBUG] CanvasGroup path: {GetGameObjectPath(cg.gameObject)}");
        }

        // Test 5: UI Hierarchy Check
        Debug.Log("=== UI HIERARCHY CHECK ===");
        if (tutorialModeButton != null)
        {
            CheckUIHierarchy(tutorialModeButton.transform, "Tutorial Button");
        }
        if (missionModeButton != null)
        {
            CheckUIHierarchy(missionModeButton.transform, "Mission Button");
        }

        // Test 6: Canvas and EventSystem validation
        Debug.Log("=== SYSTEM VALIDATION ===");
        var canvas = FindFirstObjectByType<Canvas>();
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        var graphicRaycaster = canvas?.GetComponent<GraphicRaycaster>();

        Debug.Log($"[DEBUG] Canvas: {(canvas != null ? "Found" : "MISSING")}");
        Debug.Log($"[DEBUG] GraphicRaycaster: {(graphicRaycaster != null ? "Found" : "MISSING")}");
        Debug.Log($"[DEBUG] EventSystem: {(eventSystem != null ? "Found" : "MISSING")}");

        if (canvas != null)
        {
            Debug.Log($"[DEBUG] Canvas render mode: {canvas.renderMode}");
            Debug.Log($"[DEBUG] Canvas sort order: {canvas.sortingOrder}");
            Debug.Log($"[DEBUG] Canvas enabled: {canvas.enabled}");
        }

        if (graphicRaycaster != null)
        {
            Debug.Log($"[DEBUG] GraphicRaycaster enabled: {graphicRaycaster.enabled}");
        }

        if (eventSystem != null)
        {
            Debug.Log($"[DEBUG] EventSystem enabled: {eventSystem.enabled}");
            Debug.Log($"[DEBUG] EventSystem current selected: {eventSystem.currentSelectedGameObject}");
        }

        Debug.Log("=== END COMPREHENSIVE DEBUG ===");
    }

    /// <summary>
    /// Helper method to get full GameObject path
    /// </summary>
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    /// <summary>
    /// Helper method to check UI hierarchy for blocking elements
    /// </summary>
    private void CheckUIHierarchy(Transform transform, string buttonName)
    {
        Debug.Log($"[HIERARCHY] Checking {buttonName} hierarchy:");
        Transform current = transform;
        while (current != null)
        {
            var go = current.gameObject;
            Debug.Log($"[HIERARCHY] - {go.name}: active={go.activeSelf}, enabled in hierarchy={go.activeInHierarchy}");

            // Check for components that might block interaction
            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"[HIERARCHY]   └ CanvasGroup: interactable={canvasGroup.interactable}, blocksRaycasts={canvasGroup.blocksRaycasts}");
            }

            var canvas = go.GetComponent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"[HIERARCHY]   └ Canvas: enabled={canvas.enabled}, sortingOrder={canvas.sortingOrder}");
            }

            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"[HIERARCHY]   └ RectTransform: position={rectTransform.anchoredPosition}, size={rectTransform.sizeDelta}");
            }

            current = current.parent;
        }
    }

    // Public methods for external control

    /// <summary>
    /// Enable/disable tutorial mode button
    /// </summary>
    public void SetTutorialButtonEnabled(bool enabled)
    {
        if (tutorialModeButton != null)
            tutorialModeButton.interactable = enabled;
    }

    /// <summary>
    /// Enable/disable mission mode button
    /// </summary>
    public void SetMissionButtonEnabled(bool enabled)
    {
        if (missionModeButton != null)
            missionModeButton.interactable = enabled;
    }

    /// <summary>
    /// Update button text (optional customization)
    /// </summary>
    public void SetButtonTexts(string tutorialText, string missionText)
    {
        if (tutorialModeButton != null)
        {
            var buttonText = tutorialModeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = tutorialText;
        }

        if (missionModeButton != null)
        {
            var buttonText = missionModeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = missionText;
        }
    }

    /// <summary>
    /// Manual button test methods for Unity Inspector testing
    /// </summary>
    /// 
    // Also update the manual test methods:
    [ContextMenu("Test Tutorial Button Click")]
    public void TestTutorialButtonClick()
    {
        Debug.Log("[MANUAL TEST] Tutorial button test clicked");

        // Find GameManager fresh each time
        var gm = gameManager ?? FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.SelectTutorialMode();
        }
        else
        {
            Debug.LogError("[MANUAL TEST] GameManager is null!");
        }
    }

    [ContextMenu("Test Mission Button Click")]
    public void TestMissionButtonClick()
    {
        Debug.Log("[MANUAL TEST] Mission button test clicked");

        // Find GameManager fresh each time
        var gm = gameManager ?? FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.SelectMissionMode();
        }
        else
        {
            Debug.LogError("[MANUAL TEST] GameManager is null!");
        }
    }

    [ContextMenu("Force Button Setup")]
    public void ForceButtonSetup()
    {
        Debug.Log("[MANUAL TEST] Forcing button setup...");
        SetupButtons();
    }

    [ContextMenu("Run Debug Test")]
    public void RunDebugTest()
    {
        Debug.Log("[MANUAL TEST] Running debug test...");
        StartCoroutine(TestButtonSetup());
    }
}