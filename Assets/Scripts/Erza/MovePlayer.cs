using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementMode{
    Free,
    Grid
}

public class MovePlayer : MonoBehaviour
{
    public InputActionReference MoveAction;
    public PlayerMoveMode playerMoveMode;
    public MovementMode movementMode;

    void OnEnable()
    {
        MoveAction.action.Enable();
        movementMode = playerMoveMode.movementMode;
    }

    void Update()
    {
        float moveSpeed = 5f;
        
        if (movementMode == MovementMode.Free)
        {
            float horizontalInput = MoveAction.action.ReadValue<Vector2>().x;
            float verticalInput = MoveAction.action.ReadValue<Vector2>().y;
            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }else if (movementMode == MovementMode.Grid)
        {
            float gridSize = 1f;
            float horizontalInput = MoveAction.action.ReadValue<Vector2>().x;
            float verticalInput = MoveAction.action.ReadValue<Vector2>().y;

            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * gridSize;
            MoveOneStep(movement);
        }
        
    }

    void MoveOneStep(Vector3 movement)
    {
        if(MoveAction.action.WasPressedThisFrame())
        {
            transform.position += movement;
        }
        
    }

    public void SetMovementMode(MovementMode mode)
    {
        movementMode = mode;
    }
}
