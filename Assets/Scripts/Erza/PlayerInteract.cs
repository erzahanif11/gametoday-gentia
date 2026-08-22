using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public Transform interactPoint;
    public float interactRadius = 0.5f;
    public LayerMask interactLayer;
    public InputActionReference interactAction;

    private IInteractable currentInteractable;

    private void OnEnable(){
        interactAction.action.Enable();
    }

    private void OnDisable(){
        interactAction.action.Disable();
    }

    void Update(){
        CheckInteract();

        if(interactAction.action.WasPressedThisFrame()){
            TryInteract();
        }
    }

    void CheckInteract(){
        Collider2D hit = Physics2D.OverlapCircle(
            interactPoint.position, interactRadius, interactLayer
        );

        if (hit != null){
            currentInteractable = hit.GetComponent<IInteractable>();
            Debug.Log(currentInteractable.GetInteractText());
        }else{
            currentInteractable = null;
        }
    }

    void TryInteract(){
        currentInteractable?.Interact();
    }
}
