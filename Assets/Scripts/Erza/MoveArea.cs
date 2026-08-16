using UnityEngine;
using Unity.Cinemachine;

public class MoveArea : MonoBehaviour
{
    private MovePlayer movePlayer;
    public CinemachineCamera virtualCamera;
    public float cameraZoomInSize = 3f;
    public float cameraZoomOutSize = 5f;

    void Awake(){
        movePlayer = this.GetComponent<MovePlayer>();
        virtualCamera = FindAnyObjectByType<CinemachineCamera>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GridArea"))
        {
            Debug.Log("Entered Grid Area");
            if (movePlayer != null)
            {
                movePlayer.SetMovementMode(MovementMode.Grid);
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
}
