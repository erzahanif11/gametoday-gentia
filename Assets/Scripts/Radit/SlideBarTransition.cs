using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// Sliding diagonal bar scene transition.
/// 
/// The bars (child Image objects) start off-screen and slide in to cover the
/// entire screen. Once fully covered, a scene is loaded asynchronously.
/// After the scene is ready the bars slide back out to reveal the new scene.
///
/// Setup:
///   1. Create a Canvas (Screen Space - Overlay, sort order high).
///   2. Add diagonal bar Images as children (rotated, wide enough to cover screen).
///   3. Attach this script to the Canvas or a parent GameObject.
///   4. Bars are auto-collected from child Images on Awake.
///   5. Call <see cref="TransitionToScene(string)"/> to trigger.
///
/// The GameObject persists across scenes via DontDestroyOnLoad.
/// </summary>
public class SlideBarTransition : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("How far off-screen (in local X) the bars start/end.")]
    [SerializeField] private float slideOffset = 3000f;

    [Tooltip("Duration of the slide-in animation.")]
    [SerializeField] private float slideInDuration = 0.6f;

    [Tooltip("Duration of the slide-out animation.")]
    [SerializeField] private float slideOutDuration = 0.5f;

    [Tooltip("Stagger delay between each bar's animation start.")]
    [SerializeField] private float staggerDelay = 0.03f;

    [Header("Easing")]
    [SerializeField] private Ease slideInEase = Ease.OutQuart;
    [SerializeField] private Ease slideOutEase = Ease.InQuart;

    [Header("Direction")]
    [Tooltip("If true, bars slide in from the right. If false, from the left.")]
    [SerializeField] private bool slideFromRight = true;

    // ───────── Singleton ─────────

    public static SlideBarTransition Instance { get; private set; }

    // ───────── Runtime ─────────

    private RectTransform[] bars;
    private Vector2[] barRestPositions;   // on-screen positions (where they cover the screen)
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

        CollectBars();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ───────── Public API ─────────

    /// <summary>
    /// Triggers the full transition: slide bars in → load scene → slide bars out.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("SlideBarTransition: Transition already in progress.", this);
            return;
        }

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    /// <summary>
    /// Slides bars in to cover the screen. Calls <paramref name="onComplete"/> when done.
    /// Useful for manual control without automatic scene loading.
    /// </summary>
    public void SlideIn(Action onComplete = null)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        AnimateSlideIn(() =>
        {
            isTransitioning = false;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Slides bars out to reveal the screen. Calls <paramref name="onComplete"/> when done.
    /// </summary>
    public void SlideOut(Action onComplete = null)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        AnimateSlideOut(() =>
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

        // ── Slide In ──
        bool slideInDone = false;
        AnimateSlideIn(() => slideInDone = true);

        while (!slideInDone)
            yield return null;

        // ── Load Scene Async ──
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        // Small delay so the new scene can render a first frame behind the bars
        yield return null;

        // ── Slide Out ──
        bool slideOutDone = false;
        AnimateSlideOut(() => slideOutDone = true);

        while (!slideOutDone)
            yield return null;

        isTransitioning = false;
    }

    // ───────── Animation ─────────

    private void AnimateSlideIn(Action onComplete)
    {
        float direction = slideFromRight ? 1f : -1f;

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < bars.Length; i++)
        {
            RectTransform bar = bars[i];
            Vector2 restPos = barRestPositions[i];

            // Start position: off-screen
            Vector2 startPos = restPos + Vector2.right * (slideOffset * direction);
            bar.anchoredPosition = startPos;

            // Animate to rest position (covering the screen)
            seq.Insert(
                i * staggerDelay,
                bar.DOAnchorPos(restPos, slideInDuration).SetEase(slideInEase)
            );
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }

    private void AnimateSlideOut(Action onComplete)
    {
        float direction = slideFromRight ? -1f : 1f; // slide out to opposite side

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < bars.Length; i++)
        {
            RectTransform bar = bars[i];

            // Animate from current (rest) position to off-screen on the other side
            Vector2 endPos = barRestPositions[i] + Vector2.right * (slideOffset * direction);

            seq.Insert(
                i * staggerDelay,
                bar.DOAnchorPos(endPos, slideOutDuration).SetEase(slideOutEase)
            );
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }

    // ───────── Setup ─────────

    /// <summary>
    /// Collects all child Image RectTransforms as bars.
    /// </summary>
    private void CollectBars()
    {
        Image[] images = GetComponentsInChildren<Image>(true);

        // Exclude the Canvas's own Image if it has one
        var barList = new System.Collections.Generic.List<RectTransform>();
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].transform == transform) continue;
            barList.Add(images[i].rectTransform);
        }

        bars = barList.ToArray();
        barRestPositions = new Vector2[bars.Length];

        for (int i = 0; i < bars.Length; i++)
        {
            barRestPositions[i] = bars[i].anchoredPosition;
        }

        Debug.Log($"SlideBarTransition: Collected {bars.Length} bars.", this);
    }

    // ───────── Debug / Testing ─────────

    /// <summary>
    /// Test: Slides bars in to cover the screen.
    /// </summary>
    [ContextMenu("Test Slide In")]
    private void TestSlideIn()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Slide In: Only works in Play mode.");
            return;
        }

        SlideIn(() => Debug.Log("Test Slide In: Complete."));
    }

    /// <summary>
    /// Test: Slides bars out to reveal the screen.
    /// </summary>
    [ContextMenu("Test Slide Out")]
    private void TestSlideOut()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Slide Out: Only works in Play mode.");
            return;
        }

        SlideOut(() => Debug.Log("Test Slide Out: Complete."));
    }

    /// <summary>
    /// Test: Full round trip — slide in, wait, then slide out.
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

        SlideIn(() => done = true);
        while (!done) yield return null;

        // Hold for a moment with screen covered
        yield return new WaitForSeconds(0.5f);

        done = false;
        SlideOut(() => done = true);
        while (!done) yield return null;

        Debug.Log("Test Round Trip: Complete.");
    }
}
