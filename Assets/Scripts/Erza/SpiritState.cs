using UnityEngine;
using System;

public enum SpiritStateEnum{
    Free,
    Captured
}

public class SpiritState : MonoBehaviour
{
    public SpiritStateEnum spiritState;
    public event Action<SpiritStateEnum> OnSpiritStateChanged;

    void Awake(){
        spiritState = SpiritStateEnum.Free;
    }

    public void SetSpiritState(SpiritStateEnum newState){
        if(spiritState != newState){
            spiritState = newState;
            OnSpiritStateChanged?.Invoke(newState);
        }
    }
}
