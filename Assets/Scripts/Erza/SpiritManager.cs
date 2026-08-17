using UnityEngine;

public class SpiritManager : MonoBehaviour
{
    public PlayerManager playerManager;

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
    
}
