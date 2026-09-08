using UnityEngine;
using DG.Tweening;

/// <summary>
/// An interactable lever that toggles associated platforms ON/OFF.
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

    [Header("Interaction State")]
    [Tooltip("True jika player sedang berada di dalam area trigger lever.")]
    public bool isPlayerInRange = false;

    [Header("Visuals")]
    [Tooltip("Sprite to display when the lever is ON.")]
    [SerializeField] private Sprite onSprite;
    [Tooltip("Sprite to display when the lever is OFF.")]
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Color onTint = new Color(0.5f, 1f, 0.5f, 1f);
    [SerializeField] private Color offTint = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float tweenDuration = 0.3f;

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

        // Ensure the collider acts as a trigger for interaction detection
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (parentPlatform != null && spriteRenderer != null)
        {
            bool platformVisible = (parentPlatform.CurrentState != PressurePlatform.State.Hidden);
            if (spriteRenderer.enabled != platformVisible)
            {
                spriteRenderer.enabled = platformVisible;
                if (col != null) col.enabled = platformVisible;
            }
        }
    }

    // ───────── Triggers (Player Detection) ─────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pastikan GameObject Player Anda memiliki Tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Opsional: Tambahkan logika untuk menampilkan UI (misal: "Press E to Interact") di sini
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Opsional: Sembunyikan UI di sini
        }
    }

    // ───────── Initialization ─────────

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
        if (col != null && !col.enabled) return;

        // Tolak interaksi jika player tidak berada di dalam area collider
        if (!isPlayerInRange) return;

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
        return "Lever";
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
                Debug.LogWarning($"Lever {leverId}: Target platform ID {targets[i]} not found.", this);
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

            target.Hide(animate: true, force: true);
        }
    }

    // ───────── Visuals ─────────

    private void ApplyVisualState(bool animate)
    {
        if (spriteRenderer == null) return;

        Sprite targetSprite = isOn ? onSprite : offSprite;
        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }

        Color targetColor = isOn ? onTint : offTint;

        if (animate)
        {
            DOTween.Kill(spriteRenderer);
            spriteRenderer.DOColor(targetColor, tweenDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            spriteRenderer.color = targetColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOn ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}