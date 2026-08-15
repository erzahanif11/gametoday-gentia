using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerManager : MonoBehaviour
{
    public MovePlayer[] movePlayers;
    int currentPlayerIndex = 0;
    int lastSpiritIndex = 1;
    public InputActionReference ChangePlayerForwardAction;
    public InputActionReference ChangePlayerBackwardAction;
    public InputActionReference ChangePlayerModeAction; // between angel and spirits
    public CinemachineCamera virtualCamera;

    void Awake(){
        if (movePlayers.Length > 0){
            //player 0 must be angel
            EnablePlayer(0);
            for(int i = 1; i < movePlayers.Length; i++){
                DisablePlayer(i);
            }
        }
    }

    void OnEnable(){
        ChangePlayerForwardAction.action.Enable();
        ChangePlayerBackwardAction.action.Enable();
        ChangePlayerModeAction.action.Enable();

        ChangePlayerForwardAction.action.performed += ctx => OnForward(ctx);
        ChangePlayerBackwardAction.action.performed += ctx => OnBackward(ctx);
        ChangePlayerModeAction.action.performed += ctx => OnChangeMode(ctx);
    }

    void OnDisable(){
        ChangePlayerForwardAction.action.Disable();
        ChangePlayerBackwardAction.action.Disable();
        ChangePlayerModeAction.action.Disable();

        ChangePlayerForwardAction.action.performed -= ctx => OnForward(ctx);
        ChangePlayerBackwardAction.action.performed -= ctx => OnBackward(ctx);
        ChangePlayerModeAction.action.performed -= ctx => OnChangeMode(ctx);
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
        if(movePlayers.Length <= 1 || currentPlayerIndex == 0){
            return;
        }
        DisablePlayer(currentPlayerIndex);
        currentPlayerIndex = (currentPlayerIndex + 1) % movePlayers.Length;
        if(currentPlayerIndex == 0){
            //player 0 must be angel
            currentPlayerIndex = 1;
        }
        lastSpiritIndex = currentPlayerIndex;
        EnablePlayer(currentPlayerIndex);
    }

    void ChangePlayerBackward(){ //spirit only
        if(movePlayers.Length <= 1 || currentPlayerIndex == 0){
            return;
        }
        DisablePlayer(currentPlayerIndex);
        currentPlayerIndex = (currentPlayerIndex - 1 + movePlayers.Length) % movePlayers.Length;
        if(currentPlayerIndex == 0){
            //player 0 must be angel
            currentPlayerIndex = movePlayers.Length - 1;
        }
        lastSpiritIndex = currentPlayerIndex;
        EnablePlayer(currentPlayerIndex);
    }

    void ChangePlayerMode(){
        if(movePlayers.Length <= 1){
            return;
        }
        if(currentPlayerIndex == 0){
            EnablePlayer(lastSpiritIndex);
            DisablePlayer(0);
        }else{
            DisablePlayer(currentPlayerIndex);
            EnablePlayer(0);
        }
    }

    void EnablePlayer(int index){
        if (movePlayers[index] == null){
            return;
        }
        movePlayers[index].isControlled = true;
        virtualCamera.Follow = movePlayers[index].transform;
        currentPlayerIndex = index;
    }

    void DisablePlayer(int index){
        movePlayers[index].isControlled = false;
    }



}
