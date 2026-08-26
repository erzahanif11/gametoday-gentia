using UnityEngine;

public class SpiritManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public LevelSpiritInfo levelSpiritInfo;
    public GameObject spiritPrefab;
    private Vector3 spawnPosition;
    public TMPro.TMP_Text spiritCountText;
    public LevelManager levelManager;
    int capturedSpiritCount = 0;
    bool levelCompleted = false;

    void Awake(){
        if(playerManager == null){
            playerManager = GetComponent<PlayerManager>();
        }
        if(levelManager == null){
            levelManager = FindAnyObjectByType<LevelManager>();
        }
    }

    void Start(){
        foreach(MovePlayer spirit in playerManager.movePlayers){
            if(spirit.spiritState != null){
                spirit.spiritState.OnSpiritStateChanged += HandleSpiritStateChanged;
            }
        }
    }

    void OnDisable(){
        foreach(MovePlayer spirit in playerManager.movePlayers){
            if(spirit.spiritState != null){
                spirit.spiritState.OnSpiritStateChanged -= HandleSpiritStateChanged;
            }
        }
    }

    void HandleSpiritStateChanged(SpiritStateEnum newState, MovePlayer spirit){
        if(levelCompleted){
            return;
        }

        if(newState == SpiritStateEnum.Captured){
            capturedSpiritCount++;
            // playerManager.RemoveSpirit(spirit);
            spiritCountText.text = "Spirits: " + (capturedSpiritCount) + "/" + levelSpiritInfo.spiritCount + "\nCaptured a spirit!";
            if(capturedSpiritCount >= levelSpiritInfo.spiritCount){
                levelCompleted = true;
                Debug.Log("All spirits have been captured! Level Complete!");
                spiritCountText.text = "All spirits captured!";
                levelManager.CompleteLevel();
            }
            Debug.Log("A spirit has been captured!");
        }else if(newState == SpiritStateEnum.Free){
            capturedSpiritCount = Mathf.Max(0, capturedSpiritCount - 1);
            spiritCountText.text = "Spirits: " + (capturedSpiritCount) + "/" + levelSpiritInfo.spiritCount + "\nFreed a spirit!";
            Debug.Log("A spirit has been freed!");
        }
    }

    void GenerateRandomSpawnPosition(){
        float x = Random.Range(levelSpiritInfo.minSpawnPosition.x, levelSpiritInfo.maxSpawnPosition.x);
        float y = Random.Range(levelSpiritInfo.minSpawnPosition.y, levelSpiritInfo.maxSpawnPosition.y);
        spawnPosition = new Vector3(x, y, 0);
    }

    void SpawnNewSpirit(Vector3 position){
        GameObject newSpiritObj = Instantiate(spiritPrefab, position, Quaternion.identity);
        MovePlayer newSpirit = newSpiritObj.GetComponent<MovePlayer>();
        if(newSpirit != null){
            playerManager.AddSpirit(newSpirit);
            if(newSpirit.spiritState != null){
                newSpirit.spiritState.OnSpiritStateChanged += HandleSpiritStateChanged;
            }
            spiritCountText.text = "Spirits: " + (levelSpiritInfo.spiritCount - (playerManager.movePlayers.Count-1)) + "/" + levelSpiritInfo.spiritCount;
            Debug.Log("Spawned a new spirit at position: " + position);
        }else{
            Debug.LogError("The spawned spirit prefab does not have a MovePlayer component.");
        }
    }

    public void SpawnSpiritAtRandomPosition(){
        for(int i = 0; i < levelSpiritInfo.spiritCount; i++){
            GenerateRandomSpawnPosition();
            SpawnNewSpirit(spawnPosition);
        }
    }

    public void SetupLevel(LevelSpiritInfo newLevelSpiritInfo){
        capturedSpiritCount = 0;
        levelCompleted = false;

        // Clear spirits lama
        int spiritsToRemove = playerManager.movePlayers.Count - 1; // -1 untuk angel
        for(int i = 0; i < spiritsToRemove; i++){
            if(playerManager.movePlayers.Count > 1){
                MovePlayer spirit = playerManager.movePlayers[1]; // selalu remove index 1 (angel di 0)
                Destroy(spirit.gameObject);
                playerManager.RemoveSpirit(spirit);
            }
        }
        
        // Setup level baru
        levelSpiritInfo = newLevelSpiritInfo;
        Debug.Log("Level setup: " + levelSpiritInfo.spiritCount + " spirits to spawn");
    }
    
}
