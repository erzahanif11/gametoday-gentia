using UnityEngine;

public class PlayerMoveMode : MonoBehaviour
{
    public MovementMode movementMode;
    
    void OnEnable(){
        movementMode = MovementMode.Free;
    }
}
