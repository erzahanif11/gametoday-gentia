using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Parent Canvas (Optional)")]
    [Tooltip("Jika TutorialManager ini bukan berada di Canvas Parent, masukkan Canvas Parent di sini agar bisa dihidupkan/matikan.")]
    public GameObject canvasParent;

    [Header("Tutorial Panels")]
    [Tooltip("Masukkan semua panel tutorial secara berurutan ke dalam array ini.")]
    public GameObject[] tutorialPanels;

    [Header("Navigation Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    [Header("Settings")]
    [Tooltip("Jika dicentang, game akan ter-pause (Time.timeScale = 0) saat tutorial terbuka.")]
    public bool freezeTimeWhenOpen = true;

    [Tooltip("Jika true, tombol close tidak akan muncul sampai halaman terakhir (berguna untuk level 1). Akan otomatis menjadi false setelahnya.")]
    public bool forceReadTutorial = false;

    // Callback saat tutorial ditutup
    public System.Action onTutorialClosed;

    // Static property untuk mengecek apakah tutorial sedang terbuka
    public static bool IsTutorialOpen { get; private set; }

    private int currentIndex = 0;

    private void Start()
    {
        // Daftarkan event untuk tombol-tombol
        if (nextButton != null) nextButton.onClick.AddListener(NextPanel);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPanel);
        if (closeButton != null) closeButton.onClick.AddListener(CloseTutorial);

        // Jangan otomatis buka panel di Start. Biarkan script lain memanggil OpenTutorial()
    }

    /// <summary>
    /// Membuka Tutorial Manager dan menampilkan panel pertama.
    /// Panggil fungsi ini dari script lain jika ingin memunculkan tutorial.
    /// </summary>
    public void OpenTutorial()
    {
        IsTutorialOpen = true;
        if (canvasParent != null) canvasParent.SetActive(true);
        gameObject.SetActive(true);

        if (freezeTimeWhenOpen)
        {
            Time.timeScale = 0f;
            Debug.Log("asdd");
        }

        ShowPanel(0);
    }

    /// <summary>
    /// Menutup keseluruhan UI tutorial.
    /// </summary>
    public void CloseTutorial()
    {
        IsTutorialOpen = false;
        if (canvasParent != null) canvasParent.SetActive(false);
        gameObject.SetActive(false);

        if (freezeTimeWhenOpen)
        {
            Time.timeScale = 1f;
        }

        onTutorialClosed?.Invoke();
        onTutorialClosed = null; // Clear the callback after invoking
    }

    public void NextPanel()
    {
        if (currentIndex < tutorialPanels.Length - 1)
        {
            ShowPanel(currentIndex + 1);
        }
    }

    public void PrevPanel()
    {
        if (currentIndex > 0)
        {
            ShowPanel(currentIndex - 1);
        }
    }

    private void ShowPanel(int index)
    {
        currentIndex = index;

        // Loop untuk menyalakan panel yang sesuai dan mematikan sisanya
        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            if (tutorialPanels[i] != null)
            {
                tutorialPanels[i].SetActive(i == currentIndex);
            }
        }

        // Update status tombol Next dan Prev
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        // Matikan tombol Prev (SetActive(false)) jika sedang di panel paling pertama
        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(currentIndex > 0);
        }

        // Matikan tombol Next (SetActive(false)) jika sedang di panel paling terakhir
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentIndex < tutorialPanels.Length - 1);
        }

        // Atur tombol Close untuk fitur Force Read
        if (closeButton != null)
        {
            if (forceReadTutorial)
            {
                // Hanya muncul di halaman terakhir
                bool isLastPage = (currentIndex == tutorialPanels.Length - 1);
                closeButton.gameObject.SetActive(isLastPage);

                // Setelah sampai di halaman terakhir, matikan forceReadTutorial agar tutorial selanjutnya normal
                if (isLastPage)
                {
                    forceReadTutorial = false;
                }
            }
            else
            {
                closeButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnDestroy()
    {
        IsTutorialOpen = false;

        // Bersihkan listener saat object dihancurkan
        if (nextButton != null) nextButton.onClick.RemoveListener(NextPanel);
        if (prevButton != null) prevButton.onClick.RemoveListener(PrevPanel);
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseTutorial);

        // Pastikan time scale kembali normal jika object hancur saat tutorial masih terbuka
        if (freezeTimeWhenOpen)
        {
            Time.timeScale = 1f;
        }
    }
}
