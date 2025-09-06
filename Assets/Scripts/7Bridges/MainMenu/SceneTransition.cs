using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Simple scene transition with fade effect
/// Attach to a persistent GameObject (like GameManager)
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    private bool isTransitioning = false;

    // Public property to check transition state
    public bool IsTransitioning => isTransitioning;

    // Static reference for easy access
    public static SceneTransition Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern for scene transitions
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load scene with fade transition
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }
    }

    /// <summary>
    /// Load scene by index with fade transition
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneWithFade(sceneIndex));
        }
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Load scene
        SceneManager.LoadScene(sceneName);

        // Fade in
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    private IEnumerator LoadSceneWithFade(int sceneIndex)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Load scene
        SceneManager.LoadScene(sceneIndex);

        // Fade in
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        // Create fade overlay if needed
        GameObject fadeOverlay = GameObject.Find("FadeOverlay");
        if (fadeOverlay == null)
        {
            fadeOverlay = CreateFadeOverlay();
        }

        var canvasGroup = fadeOverlay.GetComponent<CanvasGroup>();
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // Keep overlay visible if fading out, hide if fading in
        if (targetAlpha <= 0f)
        {
            fadeOverlay.SetActive(false);
        }
    }

    private GameObject CreateFadeOverlay()
    {
        // Create fade overlay UI
        GameObject overlay = new GameObject("FadeOverlay");
        DontDestroyOnLoad(overlay);

        // Add Canvas
        Canvas canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // High priority

        // Add GraphicRaycaster
        overlay.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Add CanvasGroup for alpha control
        CanvasGroup canvasGroup = overlay.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Create background image
        GameObject background = new GameObject("Background");
        background.transform.SetParent(overlay.transform);

        UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = fadeColor;

        // Stretch to fill screen
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        overlay.SetActive(true);
        return overlay;
    }
}