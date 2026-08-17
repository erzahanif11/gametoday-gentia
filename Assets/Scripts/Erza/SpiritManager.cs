using UnityEngine;

public class SpiritManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public LevelSpiritInfo levelSpiritInfo;
    public GameObject spiritPrefab;
    private Vector3 spawnPosition;

    void Awake(){
        if(playerManager == null){
            playerManager = GetComponent<PlayerManager>();
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
        if(newState == SpiritStateEnum.Captured){
            playerManager.RemoveSpirit(spirit);
            Debug.Log("A spirit has been captured!");
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
    
}
