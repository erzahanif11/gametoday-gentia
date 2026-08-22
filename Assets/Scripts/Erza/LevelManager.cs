using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class LevelData{
    public LevelSpiritInfo levelSpiritInfo;
    public LevelStateEnum levelState;
    public int levelId;
}

public class LevelManager : MonoBehaviour
{
    public LevelDatabase levelDatabase;
    public LevelState levelState;

    private int currentLevelId = 0;
    private int clearedLevelId = -1;
    private LevelData currentLevelData;

    public event System.Action<int> OnLevelCompleted;

    void Awake(){
        if(levelState == null){
            levelState = GetComponent<LevelState>();
        }
    }

    public bool LoadLevel(int levelId){
        LevelData entry = levelDatabase.GetLevel(levelId);
        currentLevelId = levelId;
        if(entry != null && levelId > clearedLevelId){
            currentLevelId = levelId;
            currentLevelData = new LevelData{
                levelSpiritInfo = entry.levelSpiritInfo,
                levelState = LevelStateEnum.NotStarted
            };
            Debug.Log("Loading Level: " + levelId);
            if(levelState != null){
                levelState.SetLevelState(LevelStateEnum.NotStarted);
            }
            
            // Setup spirit manager dengan level data
            SpiritManager spiritManager = FindAnyObjectByType<SpiritManager>();
            if(spiritManager != null){
                spiritManager.SetupLevel(entry.levelSpiritInfo);
                spiritManager.SpawnSpiritAtRandomPosition();
                StartLevel();
            }
            return true;
        } else {
            Debug.LogError("Level with ID " + levelId + " not found in LevelDatabase or it's already cleared.");
            return false;
        }
    }

    public void StartLevel(){
        if(levelState != null){
            levelState.SetLevelState(LevelStateEnum.InProgress);
        }
    }

    public void CompleteLevel(){
        if(levelState != null){
            levelState.SetLevelState(LevelStateEnum.Completed);
            clearedLevelId = currentLevelId;
            Debug.Log("Current State of Level ID " + currentLevelData.levelId + " is " + levelState.levelState);
            OnLevelCompleted?.Invoke(clearedLevelId);
        }
    }

    public void NextLevel(){
        currentLevelId += 1;
    }

    public int GetCurrentLevelId(){
        return currentLevelId;
    }

    public LevelData GetCurrentLevelData(){
        return currentLevelData;
    }

}
