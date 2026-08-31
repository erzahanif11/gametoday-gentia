using UnityEngine;
using DG.Tweening;

/// <summary>
/// An interactable lever that toggles associated platforms ON/OFF.
/// 
/// When the player interacts:
///   OFF → ON:  Reveals all target platforms (they stay visible).
///   ON  → OFF: Hides all target platforms.
/// 
/// Implements <see cref="IInteractable"/> so the existing
/// <see cref="PlayerInteract"/> system detects and triggers it.
/// 
/// IMPORTANT: Do NOT rely on Awake/Start for ID assignment.
/// Call Initialize() from TilemapSpawner after setting the leverId.
/// </summary>
public class Lever : MonoBehaviour, IInteractable
{
    // ───────── Inspector ─────────

    [Header("Identity")]
    [Tooltip("Unique ID for this lever in the level.")]
    public int leverId;

    [Header("Targets")]
    [Tooltip("IDs of pressure platforms to reveal when ON, hide when OFF.")]
    public int[] targetIds;

    [Tooltip("IDs of pressure platforms to reveal when OFF, hide when ON.")]
    public int[] offTargetIds;

    [Header("State")]
    [Tooltip("Current toggle state. True = ON (targets revealed).")]
    public bool isOn = false;

    [Header("Visuals")]
    [SerializeField] private Color onTint = new Color(0.5f, 1f, 0.5f, 1f);
    [SerializeField] private Color offTint = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float flipDuration = 0.3f;

    // ───────── Runtime ─────────

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool initialized = false;

    [HideInInspector] public PressurePlatform parentPlatform;

    // ───────── Lifecycle ─────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (parentPlatform != null)
        {
            bool platformVisible = (parentPlatform.CurrentState != PressurePlatform.State.Hidden);
            if (spriteRenderer.enabled != platformVisible)
            {
                spriteRenderer.enabled = platformVisible;
                if (col != null) col.enabled = platformVisible;
            }
        }
    }

    // ───────── Initialization (called by TilemapSpawner) ─────────

    /// <summary>
    /// Sets the lever ID. Must be called by TilemapSpawner after spawning.
    /// </summary>
    public void Initialize(int id)
    {
        leverId = id;
        initialized = true;

        // Apply initial visual state
        ApplyVisualState(animate: false);
    }

    // ───────── IInteractable ─────────

    public void Interact()
    {
        isOn = !isOn;

        ApplyVisualState(animate: true);

        if (isOn)
        {
            RevealTargets(targetIds);
            HideTargets(offTargetIds);
        }
        else
        {
            HideTargets(targetIds);
            RevealTargets(offTargetIds);
        }
    }

    public string GetInteractText()
    {
        return "";
    }

    // ───────── Target Management ─────────

    private void RevealTargets(int[] targets)
    {
        if (targets == null || targets.Length == 0) return;

        var manager = PressurePlatformManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("Lever: No PressurePlatformManager found.", this);
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            PressurePlatform target = manager.GetById(targets[i]);
            if (target != null)
            {
                target.Reveal(animate: true);
            }
            else
            {
                Debug.LogWarning(
                    $"Lever {leverId}: Target platform ID {targets[i]} not found.", this);
            }
        }
    }

    private void HideTargets(int[] targets)
    {
        if (targets == null || targets.Length == 0) return;

        var manager = PressurePlatformManager.Instance;
        if (manager == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            PressurePlatform target = manager.GetById(targets[i]);
            if (target == null) continue;

            // Force-hide: lever controls visibility directly
            target.Hide(animate: true, force: true);
        }
    }

    // ───────── Visuals ─────────

    private void ApplyVisualState(bool animate)
    {
        if (spriteRenderer == null) return;

        Color targetColor = isOn ? onTint : offTint;

        // Flip sprite horizontally to indicate lever direction
        Vector3 targetScale = transform.localScale;
        targetScale.x = isOn ? -Mathf.Abs(targetScale.x) : Mathf.Abs(targetScale.x);

        if (animate)
        {
            DOTween.Kill(spriteRenderer);
            DOTween.Kill(transform);

            spriteRenderer.DOColor(targetColor, flipDuration).SetEase(Ease.OutQuad);
            transform.DOScaleX(targetScale.x, flipDuration).SetEase(Ease.OutBack);
        }
        else
        {
            spriteRenderer.color = targetColor;
            transform.localScale = targetScale;
        }
    }

    // ───────── Debug Gizmo ─────────

    private void OnDrawGizmosSelected()
    {
        // Show a small icon to distinguish levers in Scene view
        Gizmos.color = isOn ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
