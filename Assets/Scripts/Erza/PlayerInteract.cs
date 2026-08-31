using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    public Transform interactPoint;
    public float interactRadius = 0.5f;
    public LayerMask interactLayer;
    public InputActionReference interactAction;

    [Header("Floating Prompt")]
    [Tooltip("Vertical offset above the interact point for the floating prompt.")]
    [SerializeField] private float promptOffsetY = 1.2f;
    [SerializeField] private int promptFontSize = 24;
    [SerializeField] private Color promptColor = Color.white;
    [SerializeField] private Color promptBackgroundColor = new Color(0f, 0f, 0f, 0.6f);

    private IInteractable currentInteractable;

    // ───────── Floating Prompt UI ─────────
    private Canvas promptCanvas;
    private Text promptText;
    private Image promptBackground;
    private GameObject promptRoot;

    private void Awake()
    {
        CreatePromptUI();
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
    }

    void Update()
    {
        CheckInteract();

        if (interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void LateUpdate()
    {
        // Keep prompt positioned above interact point
        if (promptRoot != null && promptRoot.activeSelf && interactPoint != null)
        {
            promptRoot.transform.position = interactPoint.position + Vector3.up * promptOffsetY;
        }
    }

    void CheckInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            interactPoint.position, interactRadius, interactLayer
        );

        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                ShowPrompt(interactable.GetInteractText());
                return;
            }
        }

        currentInteractable = null;
        HidePrompt();
    }

    void TryInteract()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();

            // Update prompt text immediately after interaction
            // (e.g. lever text changes from "Pull" to "Reset")
            if (currentInteractable != null)
            {
                ShowPrompt(currentInteractable.GetInteractText());
            }
        }
    }

    // ───────── Prompt Creation & Management ─────────

    /// <summary>
    /// Creates a world-space Canvas with a background panel and text at runtime.
    /// No prefab or TextMeshPro dependency required.
    /// </summary>
    private void CreatePromptUI()
    {
        // Root object
        promptRoot = new GameObject("InteractPrompt");
        promptRoot.transform.SetParent(transform);
        promptRoot.transform.localPosition = Vector3.up * promptOffsetY;

        // World-space Canvas
        promptCanvas = promptRoot.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        promptCanvas.sortingOrder = 100; // render above everything

        // Scale down — Canvas units are huge by default
        promptRoot.transform.localScale = Vector3.one * 0.02f;

        // Canvas size
        RectTransform canvasRect = promptCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(300f, 60f);

        // Background panel
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(promptRoot.transform, false);
        promptBackground = bgObj.AddComponent<Image>();
        promptBackground.color = promptBackgroundColor;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(promptRoot.transform, false);
        promptText = textObj.AddComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = promptFontSize;
        promptText.color = promptColor;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.horizontalOverflow = HorizontalWrapMode.Overflow;
        promptText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Start hidden
        promptRoot.SetActive(false);
    }

    private void ShowPrompt(string text)
    {
        if (promptRoot == null) return;

        if (string.IsNullOrEmpty(text))
        {
            HidePrompt();
            return;
        }

        promptText.text = text;

        if (!promptRoot.activeSelf)
            promptRoot.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptRoot != null && promptRoot.activeSelf)
            promptRoot.SetActive(false);
    }
}
