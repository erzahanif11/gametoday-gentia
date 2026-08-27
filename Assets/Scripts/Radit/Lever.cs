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
    [Tooltip("IDs of pressure platforms to reveal/hide when toggled.")]
    public int[] targetIds;

    [Header("State")]
    [Tooltip("Current toggle state. True = ON (targets revealed).")]
    public bool isOn = false;

    [Header("Visuals")]
    [SerializeField] private Color onTint = new Color(0.5f, 1f, 0.5f, 1f);
    [SerializeField] private Color offTint = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float flipDuration = 0.3f;

    // ───────── Runtime ─────────

    private SpriteRenderer spriteRenderer;
    private bool initialized = false;

    // ───────── Lifecycle ─────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            RevealTargets();
        }
        else
        {
            HideTargets();
        }
    }

    public string GetInteractText()
    {
        return isOn ? "(F) Reset Lever" : "(F) Pull Lever";
    }

    // ───────── Target Management ─────────

    private void RevealTargets()
    {
        if (targetIds == null || targetIds.Length == 0) return;

        var manager = PressurePlatformManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("Lever: No PressurePlatformManager found.", this);
            return;
        }

        for (int i = 0; i < targetIds.Length; i++)
        {
            PressurePlatform target = manager.GetById(targetIds[i]);
            if (target != null)
            {
                target.Reveal(animate: true);
            }
            else
            {
                Debug.LogWarning(
                    $"Lever {leverId}: Target platform ID {targetIds[i]} not found.", this);
            }
        }
    }

    private void HideTargets()
    {
        if (targetIds == null || targetIds.Length == 0) return;

        var manager = PressurePlatformManager.Instance;
        if (manager == null) return;

        for (int i = 0; i < targetIds.Length; i++)
        {
            PressurePlatform target = manager.GetById(targetIds[i]);
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
