using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// A pressure platform that reveals chained platforms when a "spirit" steps on it,
/// and hides them again when the spirit steps off.
/// 
/// States:
///   Hidden    → Invisible, collider disabled. Waiting to be revealed.
///   Revealed  → Visible, collider enabled. Waiting to be stepped on.
///   Activated → A spirit is standing on it. Chain targets are revealed.
/// 
/// Behaviour:
///   - Active (Revealed/Activated) platforms can be walked on.
///   - Hidden platforms cannot be walked on (collider disabled).
///   - Stepping ON  → Activate → reveal chain targets (Platform A reveals B, C…).
///   - Stepping OFF → Deactivate → hide chain targets again (reversible).
///   - Multiple spirits on the same platform are ref-counted so the platform
///     only deactivates when ALL spirits have left.
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

    [Tooltip("Delay (seconds) before each target is revealed/hidden.")]
    public float triggerDelay = 0.15f;

    [Header("Initial State")]
    [Tooltip("If true, platform starts visible and ready to be stepped on.")]
    public bool startsRevealed = false;

    [Header("Animation")]
    [SerializeField] private float dropDistance = 0.3f;
    [SerializeField] private float revealDuration = 0.4f;
    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private Color activatedTint = new Color(0.6f, 1f, 0.6f, 1f);

    [Header("Detection")]
    [Tooltip("Size of the overlap box used to detect spirits. Should match the platform's visual size.")]
    [SerializeField] private Vector2 detectionSize = new Vector2(0.9f, 0.9f);

    // ───────── Runtime ─────────

    public State CurrentState { get; private set; } = State.Hidden;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Color originalColor;
    private Vector3 restPosition; // cached final local position (set on reveal)
    private bool initialized = false;

    /// <summary>
    /// How many "spirit" objects are currently overlapping this platform's detection box.
    /// The platform deactivates only when this reaches 0.
    /// </summary>
    private int activatorCount = 0;

    /// <summary>
    /// Running chain coroutine — cached so we can stop it if the spirit steps
    /// off before the chain finishes propagating.
    /// </summary>
    private Coroutine chainCoroutine;

    // Reusable buffer for overlap queries (avoids GC allocations)
    private static readonly Collider2D[] overlapBuffer = new Collider2D[16];

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
        DOTween.Kill(transform);
        if (spriteRenderer != null) DOTween.Kill(spriteRenderer);

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
        restPosition = transform.localPosition;

        if (startsRevealed)
        {
            SetRevealed(animate: false);
        }
        else
        {
            SetHidden(animate: false);
        }
    }

    // ───────── State Transitions ─────────

    /// <summary>Makes the platform invisible and non-interactive (immediate, no animation).</summary>
    private void SetHidden(bool animate)
    {
        CurrentState = State.Hidden;
        activatorCount = 0;

        if (animate && spriteRenderer != null)
        {
            PlayHideAnimation();
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            if (col != null)
                col.enabled = false;
        }
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
    /// Called when a spirit steps on it.
    /// </summary>
    public void Activate()
    {
        if (CurrentState != State.Revealed) return; // can only activate from revealed state

        CurrentState = State.Activated;

        // Visual feedback — tint to show it's active
        if (spriteRenderer != null)
            spriteRenderer.DOColor(activatedTint, 0.2f).SetEase(Ease.OutQuad);

        // Trigger chain reaction (reveal targets)
        if (chainCoroutine != null) StopCoroutine(chainCoroutine);
        chainCoroutine = StartCoroutine(TriggerChainReaction());
    }

    /// <summary>
    /// Deactivates the platform: reverts tint to normal revealed color and hides
    /// all chain targets (recursively). The platform itself stays visible (Revealed).
    /// </summary>
    public void Deactivate()
    {
        if (CurrentState != State.Activated) return;

        // Stop any in-progress chain reveal
        if (chainCoroutine != null)
        {
            StopCoroutine(chainCoroutine);
            chainCoroutine = null;
        }

        CurrentState = State.Revealed;

        // Revert visual tint back to normal
        if (spriteRenderer != null)
            spriteRenderer.DOColor(originalColor, 0.2f).SetEase(Ease.OutQuad);

        // Hide chain targets
        StartCoroutine(HideChainTargets());
    }

    // ───────── Chain Reaction — Reveal ─────────

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

        chainCoroutine = null;
    }

    // ───────── Chain Reaction — Hide ─────────

    /// <summary>
    /// Hides all chain targets immediately (no delays). Each target is first
    /// deactivated (if activated) so it will recursively hide its own chain
    /// targets before being hidden itself. Uses force=true so platforms collapse
    /// even if a spirit is standing on them (chain dependency broken).
    /// </summary>
    private IEnumerator HideChainTargets()
    {
        if (targetIds == null || targetIds.Length == 0) yield break;

        var manager = PressurePlatformManager.Instance;
        if (manager == null) yield break;

        for (int i = 0; i < targetIds.Length; i++)
        {
            PressurePlatform target = manager.GetById(targetIds[i]);
            if (target == null) continue;

            // Force-hide: chain dependency is broken, collapse everything
            target.Hide(animate: true, force: true);
        }
    }

    /// <summary>
    /// Public entry point to hide this platform (transition to Hidden).
    /// Called by a parent platform's deactivation chain.
    /// 
    /// When <paramref name="force"/> is true, the platform hides even if a spirit
    /// is standing on it (chain dependency was broken upstream).
    /// </summary>
    public void Hide(bool animate = true, bool force = false)
    {
        // Only hide from Revealed or Activated states
        if (CurrentState == State.Hidden) return;

        // Unless forced (chain collapse), don't hide if a spirit is standing on us
        if (!force && activatorCount > 0) return;

        // If we're activated, deactivate first (hides our own targets recursively)
        if (CurrentState == State.Activated)
        {
            Deactivate();
        }

        SetHidden(animate);

    }

    // ───────── Animation ─────────

    private void PlayRevealAnimation()
    {
        // Kill any existing tweens on this object
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);

        Vector3 startPos = restPosition + Vector3.up * dropDistance;

        // Start state
        transform.localPosition = startPos;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Animate in
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOLocalMove(restPosition, revealDuration).SetEase(Ease.OutCubic));
        seq.Join(spriteRenderer.DOFade(originalColor.a, revealDuration).SetEase(Ease.OutCubic));
    }

    private void PlayHideAnimation()
    {
        // Kill any existing tweens on this object
        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);

        Vector3 targetPos = restPosition + Vector3.up * dropDistance;

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOLocalMove(targetPos, hideDuration).SetEase(Ease.InCubic));
        seq.Join(spriteRenderer.DOFade(0f, hideDuration).SetEase(Ease.InCubic));
        seq.OnComplete(() =>
        {
            // Ensure final state is fully hidden
            if (col != null) col.enabled = false;
            transform.localPosition = restPosition; // reset position for next reveal
        });
    }

    // ───────── Spirit Detection (FixedUpdate overlap check) ─────────
    // Uses Physics2D.OverlapBox instead of OnTriggerEnter/Exit because
    // the spirit uses transform.position (teleportation) for grid movement,
    // which does not reliably trigger OnTriggerEnter2D/OnTriggerExit2D.

    private void FixedUpdate()
    {
        // Only check when the platform is interactive
        if (CurrentState != State.Revealed && CurrentState != State.Activated) return;

        // Count how many "spirit" tagged objects overlap our detection box
        int count = Physics2D.OverlapBoxNonAlloc(
            transform.position, detectionSize, 0f, overlapBuffer);

        int spiritCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (overlapBuffer[i] != null && overlapBuffer[i].CompareTag("spirit"))
                spiritCount++;
        }

        int previousCount = activatorCount;
        activatorCount = spiritCount;

        // Spirit just stepped on → activate
        if (previousCount == 0 && activatorCount > 0 && CurrentState == State.Revealed)
        {
            Activate();
        }
        // All spirits stepped off → deactivate
        else if (previousCount > 0 && activatorCount == 0 && CurrentState == State.Activated)
        {
            Deactivate();
        }
    }

    // ───────── Debug Gizmo ─────────

    private void OnDrawGizmosSelected()
    {
        // Visualize the detection box in the Scene view
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireCube(transform.position, detectionSize);
    }
}
