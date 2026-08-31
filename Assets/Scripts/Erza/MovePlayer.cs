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
    public bool isSpirit = false;
    public LayerMask wallLayerMask;
    private Rigidbody2D rb;
    float moveSpeed = 15f;
    public Tilemap movementTilemap;

    public SpiritState spiritState;
    public SpiritManager spiritManager;
    public LevelManager levelManager;
    public PlayerManager playerManager;
    public PlayerIndicator playerIndicator;

    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallLayerMask = LayerMask.GetMask("Wall");
        spiritState = GetComponent<SpiritState>();
        animator = GetComponent<Animator>();
        playerIndicator = GetComponent<PlayerIndicator>();
        if (spiritManager == null)
        {
            spiritManager = FindAnyObjectByType<SpiritManager>();
        }
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }
        if (movementTilemap == null)
        {
            movementTilemap = FindAnyObjectByType<Tilemap>();
        }
        if (playerManager == null)
        {
            playerManager = FindAnyObjectByType<PlayerManager>();
        }
    }

    void Start(){
        playerIndicator.toggleIndicator(isControlled);
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        movementMode = playerMoveMode.movementMode;

        if (levelManager != null)
        {
            levelManager.OnLevelCompleted += HandleOnLevelComplete;
        }
    }

    void OnDisable()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelCompleted -= HandleOnLevelComplete;
        }
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
    
    void HandleOnLevelComplete(int completedLevelIndex)
    {
        if(isSpirit)
        {
            playerManager.DisableAllSpirits();
            Destroy(gameObject);
        }
    }

    void MoveFree(){
        float horizontalInput = moveAction.action.ReadValue<Vector2>().x;
        float verticalInput = moveAction.action.ReadValue<Vector2>().y;
        rb.linearVelocity = new Vector2(horizontalInput, verticalInput) * moveSpeed;
        animator.SetFloat("HorizontalInput", horizontalInput);
        animator.SetFloat("VerticalInput", verticalInput);
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

        // Block movement if there is no tile (empty gap)
        if (!movementTilemap.HasTile(targetCell))
        {
            Debug.Log("Movement blocked by empty space at: " + targetCell);
            return;
        }

        // Block movement if there is a hidden pressure platform
        if (PressurePlatformManager.Instance != null)
        {
            PressurePlatform platform = PressurePlatformManager.Instance.GetByPosition(targetPosition);
            if (platform != null && platform.CurrentState == PressurePlatform.State.Hidden)
            {
                Debug.Log("Movement blocked by hidden platform at: " + targetCell);
                return;
            }
        }

        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f, wallLayerMask);
        Collider2D playerCollider = Physics2D.OverlapCircle(targetPosition, 0.1f, LayerMask.GetMask("Player"));
        if (hitCollider != null){
            Debug.Log("Movement blocked by wall at: " + targetCell);
            return;
        }
        if (playerCollider != null){
            Debug.Log("Movement blocked by player at: " + targetCell);
            return;
        }
        transform.position = targetPosition;
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
        if(playerIndicator != null)
        {
            playerIndicator.toggleIndicator(controlled);
        }
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

        // if (other.CompareTag("LoadLevel"))
        // {
        //     levelManager.LoadLevel(levelManager.GetCurrentLevelId());
        // }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("Exited the finish area!");
            spiritState.SetSpiritState(SpiritStateEnum.Free);
        }
    }
}
