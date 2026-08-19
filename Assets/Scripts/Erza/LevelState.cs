using UnityEngine;

public enum LevelStateEnum{
    NotStarted,
    InProgress,
    Completed
}

public class LevelState : MonoBehaviour
{
    public LevelStateEnum levelState = LevelStateEnum.NotStarted;
    public event System.Action<LevelStateEnum> OnLevelStateChanged;

    public void SetLevelState(LevelStateEnum newState){
        if(levelState != newState){
            levelState = newState;
            OnLevelStateChanged?.Invoke(newState);
        }
    }
}
