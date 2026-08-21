using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// A pressure platform that reveals chained platforms when stepped on.
/// 
/// States:
///   Hidden    → Invisible, collider disabled. Waiting to be revealed.
///   Revealed  → Visible, collider enabled. Waiting to be stepped on.
///   Activated → Stepped on. Chain targets have been triggered.
/// 
/// IMPORTANT: Do NOT rely on Awake/Start for initialization.
/// Call Initialize() from TilemapSpawner after setting the platformId.
/// </summary>
public class PressurePlatform : MonoBehaviour
{
    public enum State { Hidden, Revealed, Activated }

    // ───────── Inspector ─────────

    [Header("Identity")]
    [Tooltip("Unique ID for this platform in the level.")]
    public int platformId;

    [Header("Chain Targets")]
    [Tooltip("IDs of platforms to reveal when this one is activated.")]
    public int[] targetIds;

    [Tooltip("Delay (seconds) before each target is revealed.")]
    public float triggerDelay = 0.15f;

    [Header("Initial State")]
    [Tooltip("If true, platform starts visible and ready to be stepped on.")]
    public bool startsRevealed = false;

    [Header("Animation")]
    [SerializeField] private float dropDistance = 0.3f;
    [SerializeField] private float revealDuration = 0.4f;
    [SerializeField] private Color activatedTint = new Color(0.6f, 1f, 0.6f, 1f);

    // ───────── Runtime ─────────

    public State CurrentState { get; private set; } = State.Hidden;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Color originalColor;
    private bool initialized = false;

    // ───────── Lifecycle ─────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Do NOT register here — ID is not set yet.
        // TilemapSpawner will call Initialize() after assigning the correct ID.
    }

    private void OnDestroy()
    {
        if (PressurePlatformManager.Instance != null)
            PressurePlatformManager.Instance.Unregister(this);
    }

    // ───────── Initialization (called by TilemapSpawner) ─────────

    /// <summary>
    /// Sets the platform ID and registers with the manager.
    /// Must be called by TilemapSpawner BEFORE ApplyChainRules.
    /// </summary>
    public void Initialize(int id)
    {
        platformId = id;
        startsRevealed = false; // Reset — ApplyChainRules will set true for entry points

        if (PressurePlatformManager.Instance != null)
            PressurePlatformManager.Instance.Register(this);

        initialized = true;
    }

    /// <summary>
    /// Applies the initial visibility state (hidden or revealed).
    /// Must be called by TilemapSpawner AFTER ApplyChainRules has set startsRevealed.
    /// </summary>
    public void InitializeState()
    {
        if (startsRevealed)
        {
            SetRevealed(animate: false);
        }
        else
        {
            SetHidden();
        }
    }

    // ───────── State Transitions ─────────

    /// <summary>Makes the platform invisible and non-interactive.</summary>
    private void SetHidden()
    {
        CurrentState = State.Hidden;

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        if (col != null)
            col.enabled = false;
    }

    /// <summary>
    /// Reveals the platform (makes it visible and steppable).
    /// Called externally by the chain system.
    /// </summary>
    public void Reveal(bool animate = true)
    {
        if (CurrentState != State.Hidden) return; // already revealed or activated

        SetRevealed(animate);
    }

    private void SetRevealed(bool animate)
    {
        CurrentState = State.Revealed;

        if (col != null)
            col.enabled = true;

        if (animate && spriteRenderer != null)
        {
            PlayRevealAnimation();
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Activates the platform and triggers chain targets.
    /// Called when the player steps on it.
    /// </summary>
    public void Activate()
    {
        if (CurrentState != State.Revealed) return; // can only activate from revealed state

        CurrentState = State.Activated;

        // Visual feedback — tint to show it's been used
        if (spriteRenderer != null)
            spriteRenderer.DOColor(activatedTint, 0.2f).SetEase(Ease.OutQuad);

        // Trigger chain reaction
        StartCoroutine(TriggerChainReaction());
    }

    // ───────── Chain Reaction ─────────

    private IEnumerator TriggerChainReaction()
    {
        if (targetIds == null || targetIds.Length == 0) yield break;

        var manager = PressurePlatformManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("PressurePlatform: No PressurePlatformManager found.", this);
            yield break;
        }

        for (int i = 0; i < targetIds.Length; i++)
        {
            if (triggerDelay > 0f)
                yield return new WaitForSeconds(triggerDelay);

            PressurePlatform target = manager.GetById(targetIds[i]);
            if (target != null)
            {
                target.Reveal(animate: true);
            }
            else
            {
                Debug.LogWarning(
                    $"PressurePlatform {platformId}: Target ID {targetIds[i]} not found.", this);
            }
        }
    }

    // ───────── Animation ─────────

    private void PlayRevealAnimation()
    {
        Vector3 finalPos = transform.localPosition;
        Vector3 startPos = finalPos + Vector3.up * dropDistance;

        // Start state
        transform.localPosition = startPos;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Animate in
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOLocalMove(finalPos, revealDuration).SetEase(Ease.OutCubic));
        seq.Join(spriteRenderer.DOFade(originalColor.a, revealDuration).SetEase(Ease.OutCubic));
    }

    // ───────── Collision Detection ─────────
    // Uses trigger collider — the platform prefab needs a BoxCollider2D set to IsTrigger.
    // Player must have a Rigidbody2D and a Collider2D.

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CurrentState != State.Revealed) return;

        // Only react to the player
        if (other.CompareTag("Player"))
        {
            Activate();
        }
    }
}
