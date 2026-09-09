using UnityEngine;
using DG.Tweening;

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
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Color onTint = new Color(0.5f, 1f, 0.5f, 1f);
    [SerializeField] private Color offTint = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float tweenDuration = 0.3f;

    // ───────── Runtime ─────────
    private SpriteRenderer spriteRenderer;
    private Collider2D[] allColliders; // Array untuk menyimpan semua collider (solid & trigger)
    private bool initialized = false;

    [HideInInspector] public PressurePlatform parentPlatform;

    // ───────── Lifecycle ─────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 1. Ambil collider bawaan dari Prefab (misalnya BoxCollider2D)
        Collider2D[] initialColliders = GetComponents<Collider2D>();

        // 2. Jadikan semua collider bawaan tersebut menjadi SOLID agar tidak bisa dilangkahi Player
        foreach (Collider2D c in initialColliders)
        {
            c.isTrigger = false;
        }

        // 3. Tambahkan Trigger Collider dinamis (Lingkaran) khusus untuk mendeteksi interaksi Player
        CircleCollider2D interactTrigger = gameObject.AddComponent<CircleCollider2D>();
        interactTrigger.isTrigger = true;
        interactTrigger.radius = 0.6f; // Sesuaikan radius jangkauan deteksi interaksi

        // 4. Simpan semua collider (bawaan + buatan dinamis) untuk dikontrol saat hidden/reveal
        allColliders = GetComponents<Collider2D>();
    }

    private void Update()
    {
        if (parentPlatform != null && spriteRenderer != null)
        {
            bool platformVisible = (parentPlatform.CurrentState != PressurePlatform.State.Hidden);
            if (spriteRenderer.enabled != platformVisible)
            {
                spriteRenderer.enabled = platformVisible;

                // Matikan atau nyalakan seluruh collider (solid + trigger) sesuai platform
                foreach (Collider2D c in allColliders)
                {
                    if (c != null) c.enabled = platformVisible;
                }
            }
        }
    }

    // ───────── Triggers (Player Detection) ─────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    // ───────── Initialization ─────────

    public void Initialize(int id)
    {
        leverId = id;
        initialized = true;
        ApplyVisualState(animate: false);
    }

    // ───────── IInteractable ─────────

    public void Interact()
    {
        // Validasi: pastikan collider sedang aktif
        if (allColliders != null && allColliders.Length > 0 && !allColliders[0].enabled) return;

        // Tolak interaksi jika player tidak berada di dalam jangkauan
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
        if (manager == null) return;

        for (int i = 0; i < targets.Length; i++)
        {
            PressurePlatform target = manager.GetById(targets[i]);
            if (target != null) target.Reveal(animate: true);
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
            // overridePersistent: true — a lever's own on/off targets are a
            // deliberate switch and must always respond, even if that target
            // happens to be marked persistent (persistence only protects a
            // platform from a chain cascade started by another platform).
            if (target != null) target.Hide(animate: true, force: true, overridePersistent: true);
        }
    }

    // ───────── Visuals ─────────

    private void ApplyVisualState(bool animate)
    {
        if (spriteRenderer == null) return;

        Sprite targetSprite = isOn ? onSprite : offSprite;
        if (targetSprite != null) spriteRenderer.sprite = targetSprite;

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
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}