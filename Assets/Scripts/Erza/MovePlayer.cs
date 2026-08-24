using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public enum MovementMode
{
    Free,
    Grid
}

public class MovePlayer : MonoBehaviour
{
    public InputActionReference moveAction;
    public PlayerMoveMode playerMoveMode;
    public MovementMode movementMode;
    public bool isControlled = false;
    public LayerMask wallLayerMask;
    private Rigidbody2D rb;
    float moveSpeed = 10f;
    public Tilemap movementTilemap;

    public SpiritState spiritState;
    public SpiritManager spiritManager;
    public LevelManager levelManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallLayerMask = LayerMask.GetMask("Wall");
        spiritState = GetComponent<SpiritState>();
        if (spiritManager == null)
        {
            spiritManager = FindAnyObjectByType<SpiritManager>();
        }
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        movementMode = playerMoveMode.movementMode;
    }

    void Update()
    {
        if (!isControlled)
        {
            return;
        }

        if (movementMode == MovementMode.Free)
        {
            MoveFree();
        }
        else if (movementMode == MovementMode.Grid)
        {
            MoveGrid();
        }

    }

    void MoveFree(){
        float horizontalInput = moveAction.action.ReadValue<Vector2>().x;
        float verticalInput = moveAction.action.ReadValue<Vector2>().y;
        rb.linearVelocity = new Vector2(horizontalInput, verticalInput) * moveSpeed;
    }

    void MoveGrid(){
        if (!moveAction.action.WasPressedThisFrame())
        {
            return;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3Int direction;

        if(Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            direction  = input.x > 0 ? Vector3Int.right : Vector3Int.left;
        }
        else
        {
            direction = input.y > 0 ? Vector3Int.up : Vector3Int.down;
        }
        
        MoveOneStep(direction);
    }

    void MoveOneStep(Vector3Int movement)
    {
        if(movementTilemap == null)
        {
            Debug.LogWarning("Movement Tilemap is not assigned.");
            return;
        }

        Vector3Int currentcell = movementTilemap.WorldToCell(transform.position);
        Vector3Int targetCell = currentcell + movement;
        Vector3 targetPosition = movementTilemap.GetCellCenterWorld(targetCell);
        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f, wallLayerMask);
        if (hitCollider != null){
            Debug.Log("Movement blocked by wall at: " + targetCell);
            return;
        }
        transform.position = targetPosition;
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
                Destroy(gameObject);
            }
        }

        // if (other.CompareTag("LoadLevel"))
        // {
        //     levelManager.LoadLevel(levelManager.GetCurrentLevelId());
        // }
    }
}
