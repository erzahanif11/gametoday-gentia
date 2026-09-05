using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class MoveArea : MonoBehaviour
{
    private MovePlayer movePlayer;
    public CinemachineCamera virtualCamera;
    public float cameraZoomInSize = 3f;
    public float cameraZoomOutSize = 5f;

    public Tilemap movementTilemap;
    public float snapDuration = 0.2f;

    void Awake(){
        movePlayer = this.GetComponent<MovePlayer>();
        if (movementTilemap == null)
        {
            movementTilemap = GameObject.FindGameObjectWithTag("Platform").GetComponent<Tilemap>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GridArea"))
        {
            Debug.Log("Entered Grid Area");
            if (movePlayer != null)
            {
                SnapToNearestPlatform();
            }
        }
        if (other.CompareTag("TinyArea"))
        {
            Debug.Log("Entered Tiny Area");
            if (virtualCamera != null)
            {
                virtualCamera.Lens.OrthographicSize = cameraZoomInSize;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("GridArea"))
        {
            Debug.Log("Exited Grid Area");
            if (movePlayer != null)
            {
                movePlayer.SetMovementMode(MovementMode.Free);
            }
        }

        if (other.CompareTag("TinyArea"))
        {
            Debug.Log("Exited Tiny Area");
            if (virtualCamera != null)
            {
                virtualCamera.Lens.OrthographicSize = cameraZoomOutSize;
            }
        }
    }

    private void SnapToNearestPlatform(){
        if (movementTilemap == null)
        {
            Debug.LogWarning("Movement Tilemap is not assigned.");
            return;
        }
        Vector3Int currentCell = movementTilemap.WorldToCell(movePlayer.transform.position);
        Vector3Int nearestCell = FindNearestTile(currentCell);

        if (nearestCell == currentCell)
        {
            Debug.Log("Already on a valid tile. No snapping needed.");
            return;
        }

        Vector3 targetPosition = movementTilemap.GetCellCenterWorld(nearestCell);
        Debug.Log("Snapping to nearest tile at: " + nearestCell + " World Position: " + targetPosition);
        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f, movePlayer.wallLayerMask);
        if (hitCollider != null)
        {
            Debug.Log("Cannot snap to tile at: " + nearestCell + " because it is blocked by a wall.");
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(targetPosition, 0.1f, LayerMask.GetMask("Player"));
        if (playerCollider != null)
        {
            Debug.Log("Cannot snap to tile at: " + nearestCell + " because it is occupied by another player.");
            return;
        }

        movePlayer.SetMovementMode(MovementMode.Free);

        transform.DOMove(targetPosition, snapDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            Debug.Log("Snapped to nearest tile at: " + nearestCell);
            movePlayer.SetMovementMode(MovementMode.Grid);
        });
    }

    private Vector3Int FindNearestTile(Vector3Int currentCell)
    {
        Vector3Int nearestCell = currentCell;
        
        float nearestDistance = Mathf.Infinity;

        int searchRadius = 5; // Adjust this value based on how far you want to search for tiles

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchRadius; y <= searchRadius; y++)
            {
                Vector3Int cell = currentCell + new Vector3Int(x, y, 0);
                if (!movementTilemap.HasTile(cell))
                {
                    continue;
                }
                Vector3 worldPosition = movementTilemap.GetCellCenterWorld(cell);
                float distance = Vector2.Distance(transform.position, worldPosition);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCell = cell;
                }
            }
        }
        return nearestCell;
    }
}
