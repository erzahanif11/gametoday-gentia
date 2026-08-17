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
    public bool isControlled = false;
    public LayerMask wallLayerMask;
    private Rigidbody2D rb;
    public SpiritState spiritState;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        wallLayerMask = LayerMask.GetMask("Wall");
        spiritState = GetComponent<SpiritState>();
    }

    void OnEnable()
    {
        MoveAction.action.Enable();
        movementMode = playerMoveMode.movementMode;
    }

    void Update()
    {
        if(!isControlled){
            return;
        }

        float moveSpeed = 5f;
        
        if (movementMode == MovementMode.Free)
        {
            float horizontalInput = MoveAction.action.ReadValue<Vector2>().x;
            float verticalInput = MoveAction.action.ReadValue<Vector2>().y;
            rb.linearVelocity = new Vector2(horizontalInput, verticalInput) * moveSpeed;
        }else if (movementMode == MovementMode.Grid)
        {
            float gridSize = 1f;
            float horizontalInput = MoveAction.action.ReadValue<Vector2>().x;
            float verticalInput = MoveAction.action.ReadValue<Vector2>().y;
            rb.linearVelocity = Vector2.zero;

            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * gridSize;
            MoveOneStep(movement);
        }
        
    }

    void MoveOneStep(Vector3 movement)
    {
        if(!MoveAction.action.WasPressedThisFrame()) return;
        
        Vector2 newPosition = transform.position + movement;

        Collider2D hitCollider = Physics2D.OverlapCircle(newPosition, 0.1f, wallLayerMask);
        if (hitCollider != null)
        {
            Debug.Log("Movement blocked by wall.");
            return;
        }

        transform.position = newPosition;
    }

    public void SetMovementMode(MovementMode mode)
    {
        movementMode = mode;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("Reached the finish area!");
            if (spiritState != null)
            {
                spiritState.SetSpiritState(SpiritStateEnum.Captured);
            }
        }
    }
}
