using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupCanvas;
    public RectTransform popupPanel;
    public TextMeshProUGUI popupText;

    [Header("Input")]
    public UnityEngine.InputSystem.InputActionReference enterAction;

    [Header("Tutorial Link (Level 1)")]
    public TutorialManager tutorialManager;

    [Header("Level 1 Specific Settings")]
    [Tooltip("Jika di-ceklis, popup akan otomatis muncul di awal game, membuka tutorial, lalu memunculkan teks kedua setelah tutorial ditutup.")]
    public bool isLevel1 = false;
    [TextArea]
    public string[] preTutorialMessages;
    [TextArea]
    public string[] postTutorialMessages;

    [Header("Animation & Typing Settings")]
    public float animationDuration = 0.4f;
    public float typingSpeed = 0.05f;

    // Static property untuk mencegah Pause Menu terbuka
    public static bool IsPopupOpen { get; private set; }

    private string[] currentMessages;
    private int currentMessageIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private enum PopupMode { Tutorial, LevelComplete, Level1PostTutorial }
    private PopupMode currentMode;
    private UnityEngine.Events.UnityAction onCompleteAction;

    private void Awake()
    {
        if (popupCanvas != null)
        {
            popupCanvas.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (enterAction != null)
        {
            enterAction.action.Enable();
            enterAction.action.performed += OnActionPerformed;
        }
    }

    private void OnDisable()
    {
        if (enterAction != null)
        {
            enterAction.action.performed -= OnActionPerformed;
        }
    }

    private void Start()
    {
        if (enterAction != null)
        {
            enterAction.action.Enable();
        }

        // Jika ini adalah level 1, jalankan sequence dari awal (PreTutorial -> Tutorial -> PostTutorial)
        if (isLevel1 && preTutorialMessages != null && preTutorialMessages.Length > 0)
        {
            ShowTutorialPopup(preTutorialMessages);
        }
    }

    /// <summary>
    /// Menampilkan popup untuk Level 1 dengan urutan teks.
    /// Setelah teks terakhir selesai, akan otomatis membuka tutorial.
    /// </summary>
    public void ShowTutorialPopup(string[] messages)
    {
        currentMode = PopupMode.Tutorial;
        StartPopup(messages);
    }

    /// <summary>
    /// Menampilkan popup standar dengan urutan teks (misalnya saat level selesai).
    /// </summary>
    public void ShowLevelCompletePopup(string[] messages, UnityEngine.Events.UnityAction onNextAction)
    {
        currentMode = PopupMode.LevelComplete;
        onCompleteAction = onNextAction;
        StartPopup(messages);
    }

    private void StartPopup(string[] messages)
    {
        if (messages == null || messages.Length == 0) return;

        currentMessages = messages;
        currentMessageIndex = 0;
        IsPopupOpen = true;
        Time.timeScale = 0f; // Freeze gameplay di belakang

        gameObject.SetActive(true); // Pastikan PopupManager sendiri menyala
        if (popupCanvas != null) popupCanvas.SetActive(true);

        // Pastikan action kembali aktif jika sempat dimatikan oleh skrip lain (misal PlayerInteract saat di-destroy)
        if (enterAction != null)
        {
            enterAction.action.Enable();
        }

        AnimatePopupIn();
        ShowCurrentLine();
    }

    private void OnActionPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsPopupOpen)
        {
            OnInputTriggered();
        }
    }

    private void OnInputTriggered()
    {
        if (isTyping)
        {
            // Skip typing dan langsung tampilkan semua teks
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            popupText.text = currentMessages[currentMessageIndex];
            isTyping = false;
        }
        else
        {
            // Lanjut ke teks berikutnya
            currentMessageIndex++;
            if (currentMessageIndex < currentMessages.Length)
            {
                ShowCurrentLine();
            }
            else
            {
                // Selesai membaca semua teks
                FinishPopup();
            }
        }
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(currentMessages[currentMessageIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        popupText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            popupText.text += letter;
            // Penting: Menggunakan WaitForSecondsRealtime karena Time.timeScale = 0
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    private void FinishPopup()
    {
        ClosePopup();

        if (currentMode == PopupMode.Tutorial)
        {
            if (tutorialManager != null)
            {
                // Paksa pemain membaca sampai halaman terakhir
                tutorialManager.forceReadTutorial = true;
                tutorialManager.onTutorialClosed += OnLevel1TutorialClosed;
                tutorialManager.OpenTutorial();
            }
            else
            {
                Debug.LogWarning("PopupManager: Tutorial Manager belum dimasukkan di Inspector!");
            }
        }
        else if (currentMode == PopupMode.LevelComplete)
        {
            onCompleteAction?.Invoke();
        }
        else if (currentMode == PopupMode.Level1PostTutorial)
        {
            // Selesai sepenuhnya, popup sudah ditutup via ClosePopup()
        }
    }

    private void OnLevel1TutorialClosed()
    {
        // Tutorial selesai dibaca dan ditutup, munculkan popup lagi untuk teks penutup level 1
        if (postTutorialMessages != null && postTutorialMessages.Length > 0)
        {
            currentMode = PopupMode.Level1PostTutorial;
            StartPopup(postTutorialMessages);
        }
    }

    private void AnimatePopupIn()
    {
        if (popupPanel != null)
        {
            // Reset scale ke 0 lalu membesar dengan efek memantul (OutBack)
            popupPanel.localScale = Vector3.zero;

            // SetUpdate(true) sangat penting agar DOTween mengabaikan Time.timeScale = 0
            popupPanel.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    private void ClosePopup()
    {
        IsPopupOpen = false;
        Time.timeScale = 1f;

        if (popupPanel != null)
        {
            // Mengecil lalu hilang (InBack)
            popupPanel.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                if (popupCanvas != null) popupCanvas.SetActive(false);
            });
        }
        else
        {
            if (popupCanvas != null) popupCanvas.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        IsPopupOpen = false;
    }
}
