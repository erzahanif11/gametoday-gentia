using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// Fade-to-black scene transition.
///
/// A fullscreen Image fades from transparent → opaque (fade out),
/// the new scene loads, then it fades opaque → transparent (fade in)
/// to reveal the new scene.
///
/// Setup:
///   1. Attach this script to any GameObject.
///   2. The Canvas + fullscreen Image are created automatically at runtime.
///   3. Call <see cref="TransitionToScene(string)"/> to trigger.
///
/// The GameObject persists across scenes via DontDestroyOnLoad.
/// </summary>
public class FadeTransition : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Duration of the fade-out (screen goes dark).")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Tooltip("Duration of the fade-in (screen is revealed).")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Tooltip("Optional hold time while the screen is fully dark.")]
    [SerializeField] private float holdDuration = 0.1f;

    [Header("Easing")]
    [SerializeField] private Ease fadeOutEase = Ease.InQuad;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;

    [Header("Color")]
    [Tooltip("The color of the fade overlay.")]
    [SerializeField] private Color fadeColor = Color.black;

    // ───────── Singleton ─────────

    public static FadeTransition Instance { get; private set; }

    // ───────── Runtime ─────────

    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isTransitioning = false;

    // ───────── Lifecycle ─────────

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ───────── Public API ─────────

    /// <summary>
    /// Triggers the full transition: fade out → load scene → fade in.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("FadeTransition: Transition already in progress.", this);
            return;
        }

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    /// <summary>
    /// Fades out (screen goes dark). Calls <paramref name="onComplete"/> when done.
    /// Useful for manual control without automatic scene loading.
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        AnimateFadeOut(() =>
        {
            isTransitioning = false;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Fades in (screen is revealed). Calls <paramref name="onComplete"/> when done.
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        AnimateFadeIn(() =>
        {
            isTransitioning = false;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Returns true while a transition is in progress.
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    // ───────── Core Coroutine ─────────

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // ── Fade Out (screen goes dark) ──
        bool fadeOutDone = false;
        AnimateFadeOut(() => fadeOutDone = true);

        while (!fadeOutDone)
            yield return null;

        // ── Hold ──
        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        // ── Load Scene Async ──
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        // Small delay so the new scene can render a first frame behind the overlay
        yield return null;

        // ── Fade In (reveal new scene) ──
        bool fadeInDone = false;
        AnimateFadeIn(() => fadeInDone = true);

        while (!fadeInDone)
            yield return null;

        isTransitioning = false;
    }

    // ───────── Animation ─────────

    private void AnimateFadeOut(Action onComplete)
    {
        fadeImage.raycastTarget = true; // Block input while fading
        Color target = fadeColor;
        target.a = 1f;

        DOTween.Kill(fadeImage);
        fadeImage.DOColor(target, fadeOutDuration)
            .SetEase(fadeOutEase)
            .SetUpdate(true) // Ignore Time.timeScale
            .OnComplete(() => onComplete?.Invoke());
    }

    private void AnimateFadeIn(Action onComplete)
    {
        Color target = fadeColor;
        target.a = 0f;

        DOTween.Kill(fadeImage);
        fadeImage.DOColor(target, fadeInDuration)
            .SetEase(fadeInEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                fadeImage.raycastTarget = false; // Unblock input
                onComplete?.Invoke();
            });
    }

    // ───────── Setup ─────────

    /// <summary>
    /// Creates a Screen Space - Overlay Canvas with a fullscreen Image at runtime.
    /// </summary>
    private void CreateFadeUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // Render above everything

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Fullscreen Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = imageObj.AddComponent<Image>();

        Color startColor = fadeColor;
        startColor.a = 0f;
        fadeImage.color = startColor;
        fadeImage.raycastTarget = false;

        // Stretch to fill
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // ───────── Debug / Testing ─────────

    /// <summary>
    /// Test: Fades out to cover the screen.
    /// </summary>
    [ContextMenu("Test Fade Out")]
    private void TestFadeOut()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Fade Out: Only works in Play mode.");
            return;
        }

        FadeOut(() => Debug.Log("Test Fade Out: Complete."));
    }

    /// <summary>
    /// Test: Fades in to reveal the screen.
    /// </summary>
    [ContextMenu("Test Fade In")]
    private void TestFadeIn()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Fade In: Only works in Play mode.");
            return;
        }

        FadeIn(() => Debug.Log("Test Fade In: Complete."));
    }

    /// <summary>
    /// Test: Full round trip — fade out, hold, then fade in.
    /// </summary>
    [ContextMenu("Test Round Trip")]
    private void TestRoundTrip()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Round Trip: Only works in Play mode.");
            return;
        }

        StartCoroutine(RoundTripCoroutine());
    }

    private IEnumerator RoundTripCoroutine()
    {
        bool done = false;

        FadeOut(() => done = true);
        while (!done) yield return null;

        yield return new WaitForSeconds(0.5f);

        done = false;
        FadeIn(() => done = true);
        while (!done) yield return null;

        Debug.Log("Test Round Trip: Complete.");
    }
}
