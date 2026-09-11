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

    [Header("Layer Masks")]
    public LayerMask wallLayerMask;

    [Header("Tilemaps")]
    public Tilemap movementTilemap;
    [Tooltip("Tilemap layer that triggers Free Move mode when stepped on.")]
    public Tilemap dropOffTilemap; // ADDED: New Tilemap reference for drop-off zones

    [Header("Grid Position")]
    [SerializeField] private Vector3 gridPositionOffset = new Vector3(0f, 0.5f, 0f);

    private Rigidbody2D rb;
    float moveSpeed = 10f;

    public SpiritState spiritState;
    public SpiritManager spiritManager;
    public LevelManager levelManager;
    public PlayerManager playerManager;
    public PlayerIndicator playerIndicator;

    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure Wall and Interactable (Lever) layers are detected as obstacles
        wallLayerMask = LayerMask.GetMask("Wall", "Interactable");

        spiritState = GetComponent<SpiritState>();
        animator = GetComponent<Animator>();
        playerIndicator = GetComponent<PlayerIndicator>();

        if (spiritManager == null) spiritManager = FindAnyObjectByType<SpiritManager>();
        if (levelManager == null) levelManager = FindAnyObjectByType<LevelManager>();
        if (movementTilemap == null) movementTilemap = FindAnyObjectByType<Tilemap>();
        if (playerManager == null) playerManager = FindAnyObjectByType<PlayerManager>();
    }

    void Start()
    {
        playerIndicator.toggleIndicator(isControlled);
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        movementMode = playerMoveMode.movementMode;

        // if (levelManager != null)
        // {
        //     levelManager.OnLevelCompleted += HandleOnLevelComplete;
        // }
    }

    void OnDisable()
    {
        // if (levelManager != null)
        // {
        //     levelManager.OnLevelCompleted -= HandleOnLevelComplete;
        // }
    }

    void Update()
    {
        if (!isControlled) return;

        if (movementMode == MovementMode.Free)
        {
            MoveFree();
        }
        else if (movementMode == MovementMode.Grid)
        {
            MoveGrid();
        }
    }

    // void HandleOnLevelComplete(int completedLevelIndex)
    // {
    //     if (isSpirit)
    //     {
    //         playerManager.DisableAllSpirits();
    //         Destroy(gameObject);
    //     }
    // }

    void MoveFree()
    {
        float horizontalInput = moveAction.action.ReadValue<Vector2>().x;
        float verticalInput = moveAction.action.ReadValue<Vector2>().y;
        rb.linearVelocity = new Vector2(horizontalInput, verticalInput) * moveSpeed;
        animator.SetFloat("HorizontalInput", horizontalInput);
        animator.SetFloat("VerticalInput", verticalInput);
    }

    void MoveGrid()
    {
        if (!moveAction.action.WasPressedThisFrame()) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3Int direction;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            direction = input.x > 0 ? Vector3Int.right : Vector3Int.left;
        }
        else
        {
            direction = input.y > 0 ? Vector3Int.up : Vector3Int.down;
        }

        MoveOneStep(direction);
    }

    void MoveOneStep(Vector3Int movement)
    {
        if (movementTilemap == null)
        {
            Debug.LogWarning("Movement Tilemap is not assigned.");
            return;
        }

        Vector3 currentPlatformPosition = GetPlatformPosition();
        Vector3Int currentcell = movementTilemap.WorldToCell(currentPlatformPosition);
        Vector3Int targetCell = currentcell + movement;
        Vector3 targetPlatformPosition = movementTilemap.GetCellCenterWorld(targetCell);

        bool hasVisiblePlatform = false;
        bool isDropOffZone = false; // ADDED: Check for drop-off zone

        // 1. Check if the target cell is a Drop-Off Tile
        if (dropOffTilemap != null && dropOffTilemap.HasTile(targetCell))
        {
            isDropOffZone = true;
        }

        // 2. Check Pressure Platform Manager
        if (PressurePlatformManager.Instance != null)
        {
            PressurePlatform platform = PressurePlatformManager.Instance.GetByPosition(targetPlatformPosition);

            if (platform != null)
            {
                if (platform.CurrentState == PressurePlatform.State.Hidden)
                {
                    Debug.Log("Movement blocked by hidden platform at: " + targetCell);
                    return; // Fail: Platform exists but is hidden
                }
                else
                {
                    hasVisiblePlatform = true; // Success: Platform exists and is visible
                }
            }
        }

        // 3. Block movement if there is NO visible platform AND it is NOT a drop-off zone
        if (!hasVisiblePlatform && !isDropOffZone)
        {
            Debug.Log("Movement blocked by empty space at: " + targetCell);
            return;
        }

        // 4. Check for obstacles (Walls, Levers, etc.)
        Collider2D hitCollider = Physics2D.OverlapCircle(targetPlatformPosition, 0.1f, wallLayerMask);
        if (hitCollider != null)
        {
            Debug.Log("Movement blocked by wall/interactable at: " + targetCell);
            return;
        }

        // 5. Check if occupied by another player
        Vector3 playerCheckPos = targetPlatformPosition + gridPositionOffset;
        Collider2D[] playerColliders = Physics2D.OverlapCircleAll(playerCheckPos, 0.1f, LayerMask.GetMask("Player"));
        foreach (Collider2D col in playerColliders)
        {
            Debug.Log("Player detected at: " + targetCell);
            MovePlayer otherPlayer = col.GetComponentInParent<MovePlayer>();
            if (otherPlayer != null && otherPlayer != this)
            {
                Debug.Log("Movement blocked by another player at: " + targetCell);
                return;
            }
        }

        // 6. ALL CLEAR: Move the player
        transform.position = targetPlatformPosition + gridPositionOffset;
        animator.SetFloat("HorizontalInput", 0f);
        animator.SetFloat("VerticalInput", 0f);

        // 7. ADDED: Switch to Free mode if stepped on Drop-Off Zone
        if (isDropOffZone)
        {
            Debug.Log("Stepped on Drop-Off layer! Switching to Free Move mode.");
            SetMovementMode(MovementMode.Free);
        }
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
        if (playerIndicator != null)
        {
            playerIndicator.toggleIndicator(controlled);
        }
        if (animator != null)
        {
            animator.SetBool("IsControlled", controlled);
            if (controlled)
            {
                animator.SetTrigger("Rasuk");
            }
        }
    }

    public void SetMovementMode(MovementMode mode)
    {
        if (movementMode == mode)
        {
            return;
        }

        if (movementMode == MovementMode.Grid && mode != MovementMode.Grid)
        {
            transform.position -= gridPositionOffset;
        }

        movementMode = mode;

        if (movementMode == MovementMode.Grid)
        {
            transform.position += gridPositionOffset;
        }
    }

    public Vector3 GetPlatformPosition()
    {
        if (movementMode == MovementMode.Grid)
        {
            return transform.position - gridPositionOffset;
        }

        return transform.position;
    }

    public Vector3 GetGridPosition(Vector3 platformPosition)
    {
        return platformPosition + gridPositionOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isSpirit && other.CompareTag("Finish"))
        {
            Debug.Log("Reached the finish area!");
            if (spiritState != null)
            {
                spiritState.SetSpiritState(SpiritStateEnum.Captured);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isSpirit && other.CompareTag("Finish"))
        {
            Debug.Log("Exited the finish area!");
            spiritState.SetSpiritState(SpiritStateEnum.Free);
        }
    }
}