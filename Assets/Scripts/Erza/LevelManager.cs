using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class LevelData
{
    public LevelSpiritInfo levelSpiritInfo;
    public LevelStateEnum levelState;
    // public int levelId;
}

public class LevelManager : MonoBehaviour
{
    public LevelState levelState;

    // private int currentLevelId = 0;
    // private int clearedLevelId = -1;
    private LevelData currentLevelData;
    public GameObject Transitioner;
    public GameObject arrow;
    public GameObject offBoundary;
    public GameObject onBoundary;
    public SpiritManager spiritManager;

    [Header("Level Complete Popup")]
    public PopupManager popupManager;
    [TextArea]
    public string[] levelCompleteMessages = new string[] {
        "Bagus Sekali!",
        "Teka-teki di area ini telah terpecahkan.",
        "Jalan menuju area selanjutnya telah terbuka."
    };

    void Awake()
    {
        if (levelState == null)
        {
            levelState = GetComponent<LevelState>();
        }

        if (spiritManager == null)
        {
            spiritManager = FindAnyObjectByType<SpiritManager>();
        }
    }

    void Start()
    {
        StartLevel();
    }

    public bool LoadLevel(int levelId)
    {
        LevelData entry = currentLevelData;
        if (entry != null)
        {
            Debug.Log("Loading Level: " + levelId);
            if (levelState != null)
            {
                levelState.SetLevelState(LevelStateEnum.InProgress);
            }

            // Setup spirit manager dengan level data
            if (spiritManager != null)
            {
                // spiritManager.SetupLevel(entry.levelSpiritInfo);
                // spiritManager.SpawnSpiritAtRandomPosition();
                StartLevel();
            }
            return true;
        }
        else
        {
            // Debug.LogError("Level with ID " + levelId + " not found in LevelDatabase or it's already cleared.");
            return false;
        }
    }

    public void StartLevel()
    {
        if (levelState != null)
        {
            Time.timeScale = 1f;
            levelState.SetLevelState(LevelStateEnum.InProgress);
        }
    }

    public void CompleteLevel()
    {
        if (levelState != null)
        {
            levelState.SetLevelState(LevelStateEnum.Completed);

            if (spiritManager != null)
            {
                spiritManager.DestroyAllSpirits();
            }
            AudioManager.Instance.PlaySFX(AudioManager.Instance.puzzleCompleteSFX);

            // Tampilkan popup jika ada, jalankan aksi buka jalan setelah popup selesai
            if (popupManager != null && levelCompleteMessages != null && levelCompleteMessages.Length > 0)
            {
                popupManager.ShowLevelCompletePopup(levelCompleteMessages, () =>
                {
                    OpenPath();
                });
            }
            else
            {
                // Jika tidak ada popup, langsung buka jalan
                OpenPath();
            }
        }
    }

    private void OpenPath()
    {
        if (Transitioner != null) Transitioner.SetActive(true);
        if (arrow != null) arrow.SetActive(true);
        if (offBoundary != null) offBoundary.SetActive(false);
        if (onBoundary != null) onBoundary.SetActive(true);
    }

    // public void NextLevel(){
    //     currentLevelId += 1;
    // }

    // public int GetCurrentLevelId(){
    //     return currentLevelId;
    // }

    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }

}
