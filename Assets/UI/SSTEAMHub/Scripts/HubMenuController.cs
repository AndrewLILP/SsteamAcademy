using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// SSTEAM Academy hub menu - builds UI entirely in code
/// No UXML required - pure C# approach
/// </summary>
public class HubMenuController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private string sevenBridgesScene = "MainMenu";
    [SerializeField] private string openWorldScene = "OpenWorld";
    [SerializeField] private string golfScene = "Golf";

    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private UIDocument uiDocument;
    private VisualElement root;

    // UI Elements
    private Button btn7Bridges;
    private Button btnOpenWorld;
    private Button btnGolf;
    private Label progressText;

    void OnEnable()
    {
        // Get UI Document component
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            LogError("UIDocument component not found!");
            return;
        }

        // Get root visual element
        root = uiDocument.rootVisualElement;

        // Clear any existing content
        root.Clear();

        // Build the entire UI in code
        BuildUI();

        // Load and apply stylesheet
        ApplyStylesheet();

        LogDebug("Hub Menu UI built programmatically");
    }

    void OnDisable()
    {
        // Unregister callbacks to prevent memory leaks
        if (btn7Bridges != null)
            btn7Bridges.clicked -= OnSevenBridgesClicked;

        if (btnOpenWorld != null)
            btnOpenWorld.clicked -= OnOpenWorldClicked;

        if (btnGolf != null)
            btnGolf.clicked -= OnGolfClicked;
    }

    private void BuildUI()
    {
        // Create root container
        var rootContainer = new VisualElement();
        rootContainer.name = "root";
        rootContainer.AddToClassList("root-container");
        root.Add(rootContainer);

        // Build Header
        BuildHeader(rootContainer);

        // Build Content Area
        BuildContent(rootContainer);

        // Build Footer
        BuildFooter(rootContainer);

        LogDebug("UI structure built successfully");
    }

    private void BuildHeader(VisualElement parent)
    {
        var header = new VisualElement();
        header.name = "header";
        header.AddToClassList("header");

        var title = new Label("SSTEAM Academy");
        title.AddToClassList("title");

        var subtitle = new Label("Choose Your Adventure");
        subtitle.AddToClassList("subtitle");

        header.Add(title);
        header.Add(subtitle);
        parent.Add(header);
    }

    private void BuildContent(VisualElement parent)
    {
        var content = new VisualElement();
        content.name = "content";
        content.AddToClassList("content-area");

        var modulesContainer = new VisualElement();
        modulesContainer.name = "modules-container";
        modulesContainer.AddToClassList("modules-grid");

        // Create module cards
        btn7Bridges = CreateModuleCard(
            "btn-7bridges",
            "card-purple",
            "🌉",
            "7 Bridges",
            "Graph Theory Adventure",
            OnSevenBridgesClicked
        );

        btnOpenWorld = CreateModuleCard(
            "btn-openworld",
            "card-skyblue",
            "🌍",
            "Open World",
            "Explore & Discover",
            OnOpenWorldClicked
        );

        btnGolf = CreateModuleCard(
            "btn-golf",
            "card-gold",
            "⛳",
            "Golf",
            "Physics & Precision",
            OnGolfClicked
        );

        modulesContainer.Add(btn7Bridges);
        modulesContainer.Add(btnOpenWorld);
        modulesContainer.Add(btnGolf);

        content.Add(modulesContainer);
        parent.Add(content);
    }

    private Button CreateModuleCard(string name, string colorClass, string icon, string title, string description, System.Action clickCallback)
    {
        var button = new Button();
        button.name = name;
        button.AddToClassList("module-card");
        button.AddToClassList(colorClass);

        // Card content container
        var cardContent = new VisualElement();
        cardContent.AddToClassList("card-content");

        // Icon
        var iconLabel = new Label(icon);
        iconLabel.AddToClassList("card-icon");

        // Title
        var titleLabel = new Label(title);
        titleLabel.AddToClassList("card-title");

        // Description
        var descLabel = new Label(description);
        descLabel.AddToClassList("card-description");

        cardContent.Add(iconLabel);
        cardContent.Add(titleLabel);
        cardContent.Add(descLabel);

        button.Add(cardContent);

        // Register click callback
        button.clicked += clickCallback;

        return button;
    }

    private void BuildFooter(VisualElement parent)
    {
        var footer = new VisualElement();
        footer.name = "footer";
        footer.AddToClassList("footer");

        progressText = new Label("Progress: Coming Soon");
        progressText.name = "progress-text";
        progressText.AddToClassList("progress-label");

        footer.Add(progressText);
        parent.Add(footer);
    }

    private void ApplyStylesheet()
    {
        // Load the stylesheet from Resources or create inline styles
        var styleSheet = Resources.Load<StyleSheet>("SSTEAMHub/SSTEAMHubStyles");

        if (styleSheet != null)
        {
            root.styleSheets.Add(styleSheet);
            LogDebug("Stylesheet loaded from Resources");
        }
        else
        {
            // Fallback: Apply inline styles if stylesheet not found
            LogDebug("Stylesheet not found in Resources - using inline styles");
            ApplyInlineStyles();
        }
    }

    private void ApplyInlineStyles()
    {
        // Apply inline styles as fallback
        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);
        root.style.backgroundColor = new Color(0.08f, 0.08f, 0.12f); // Dark background
    }

    // Button Click Handlers
    private void OnSevenBridgesClicked()
    {
        LogDebug("7 Bridges clicked - loading MainMenu");
        LoadScene(sevenBridgesScene);
    }

    private void OnOpenWorldClicked()
    {
        LogDebug("Open World clicked");
        LoadScene(openWorldScene);
    }

    private void OnGolfClicked()
    {
        LogDebug("Golf clicked");
        LoadScene(golfScene);
    }

    // Scene Loading
    private void LoadScene(string sceneName)
    {
        // Use SceneTransition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(sceneName);
        }
        else
        {
            // Fallback: Direct scene load
            LogDebug($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }

    // Keyboard shortcuts
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnSevenBridgesClicked();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            OnOpenWorldClicked();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnGolfClicked();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    // Debug logging
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
            Debug.Log($"[HubMenuController] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[HubMenuController] {message}");
    }
}