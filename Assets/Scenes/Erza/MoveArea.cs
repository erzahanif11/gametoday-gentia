using UnityEngine;

public class MoveArea : MonoBehaviour
{
    private MovePlayer movePlayer;

    void Awake(){
        movePlayer = this.GetComponent<MovePlayer>();
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

        if (other.CompareTag("FreeArea"))
        {
            Debug.Log("Entered Free Area");
            if (movePlayer != null)
            {
                movePlayer.SetMovementMode(MovementMode.Free);
            }
        }
    }
}
