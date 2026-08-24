using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public List<MovePlayer> movePlayers;
    int currentPlayerIndex = 0;
    int lastSpiritIndex = 1;
    public InputActionReference ChangePlayerForwardAction;
    public InputActionReference ChangePlayerBackwardAction;
    public InputActionReference ChangePlayerModeAction; // between angel and spirits
    public CinemachineCamera virtualCamera;

    void Awake(){
        if (movePlayers.Count > 0){
            //player 0 must be angel
            EnablePlayer(0);
            for(int i = 1; i < movePlayers.Count; i++){
                DisablePlayer(i);
            }
        }
    }

    void OnEnable(){
        ChangePlayerForwardAction.action.Enable();
        ChangePlayerBackwardAction.action.Enable();
        ChangePlayerModeAction.action.Enable();

        ChangePlayerForwardAction.action.performed += OnForward;
        ChangePlayerBackwardAction.action.performed += OnBackward;
        ChangePlayerModeAction.action.performed += OnChangeMode;
    }

    void OnDisable(){
        ChangePlayerForwardAction.action.Disable();
        ChangePlayerBackwardAction.action.Disable();
        ChangePlayerModeAction.action.Disable();

        ChangePlayerForwardAction.action.performed -= OnForward;
        ChangePlayerBackwardAction.action.performed -= OnBackward;
        ChangePlayerModeAction.action.performed -= OnChangeMode;
    }

    void OnForward(InputAction.CallbackContext context){
        ChangePlayerForward();
    }

    void OnBackward(InputAction.CallbackContext context){
        ChangePlayerBackward();
    }

    void OnChangeMode(InputAction.CallbackContext context){
        ChangePlayerMode();
    }

    void ChangePlayerForward(){ //spirit only
        if(movePlayers.Count <= 1 || currentPlayerIndex == 0){
            return;
        }
        DisablePlayer(currentPlayerIndex);
        currentPlayerIndex = (currentPlayerIndex + 1) % movePlayers.Count;
        if(currentPlayerIndex == 0){
            //player 0 must be angel
            currentPlayerIndex = 1;
        }
        lastSpiritIndex = currentPlayerIndex;
        EnablePlayer(currentPlayerIndex);
    }

    void ChangePlayerBackward(){ //spirit only
        if(movePlayers.Count <= 1 || currentPlayerIndex == 0){
            return;
        }
        DisablePlayer(currentPlayerIndex);
        currentPlayerIndex = (currentPlayerIndex - 1 + movePlayers.Count) % movePlayers.Count;
        if(currentPlayerIndex == 0){
            //player 0 must be angel
            currentPlayerIndex = movePlayers.Count - 1;
        }
        lastSpiritIndex = currentPlayerIndex;
        EnablePlayer(currentPlayerIndex);
    }

    void ChangePlayerMode(){
        if(movePlayers.Count <= 1){
            return;
        }
        if(currentPlayerIndex == 0){
            if(lastSpiritIndex >= movePlayers.Count){
                lastSpiritIndex = movePlayers.Count - 1;
            }
            EnablePlayer(lastSpiritIndex);
            DisablePlayer(0);
        }else{
            DisablePlayer(currentPlayerIndex);
            EnablePlayer(0);
        }
    }

    void EnablePlayer(int index){
        if(index < 0 || index >= movePlayers.Count){
            return;
        }
        if (movePlayers[index] == null){
            return;
        }
        movePlayers[index].isControlled = true;
        if(virtualCamera != null){
            virtualCamera.Follow = movePlayers[index].transform;
        }
        currentPlayerIndex = index;
    }

    void DisablePlayer(int index){
        if(index < 0 || index >= movePlayers.Count){
            return;
        }
        if (movePlayers[index] == null){
            return;
        }
        movePlayers[index].isControlled = false;
    }

    public MovePlayer GetCurrentPlayer(){
        if(currentPlayerIndex < 0 || currentPlayerIndex >= movePlayers.Count){
            return null;
        }
        return movePlayers[currentPlayerIndex];
    }

    public void AddSpirit(MovePlayer spirit){
        if(spirit == null){
            return;
        }
        movePlayers.Add(spirit);
        spirit.isControlled = false;
        Debug.Log("Added new spirit. Total spirits: " + (movePlayers.Count - 1));
    }

    public void RemoveSpirit(MovePlayer spirit){
        if(spirit == null){
            return;
        }
        int index = movePlayers.IndexOf(spirit);
        if(index == -1){
            return;
        }
        if(currentPlayerIndex == index){
            DisablePlayer(currentPlayerIndex);
            currentPlayerIndex = 0; // switch back to angel
            EnablePlayer(currentPlayerIndex);
        }
        movePlayers.RemoveAt(index);
        Debug.Log("Removed a spirit. Total spirits: " + (movePlayers.Count - 1));
    }

}
