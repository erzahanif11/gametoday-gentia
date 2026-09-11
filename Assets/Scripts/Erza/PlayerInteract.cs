using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Tooltip("List of points to check for interaction.")]
    public Transform[] interactPoints;
    public float interactRadius = 0.5f;
    public LayerMask interactLayer;
    public InputActionReference interactAction;

    private IInteractable currentInteractable;

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
    }

    void Update()
    {
        CheckInteract();

        if (interactAction.action.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private System.Collections.Generic.List<Collider2D> activeTriggers = new System.Collections.Generic.List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<IInteractable>() != null)
        {
            if (!activeTriggers.Contains(other))
                activeTriggers.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (activeTriggers.Contains(other))
            activeTriggers.Remove(other);
    }

    void CheckInteract()
    {
        if (interactPoints != null && interactPoints.Length > 0)
        {
            foreach (var point in interactPoints)
            {
                if (point == null) continue;

                Collider2D hit = Physics2D.OverlapCircle(
                    point.position, interactRadius, interactLayer
                );

                if (hit != null)
                {
                    IInteractable interactable = hit.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        currentInteractable = interactable;
                        return;
                    }
                }
            }
        }

        activeTriggers.RemoveAll(c => c == null || !c.gameObject.activeInHierarchy || !c.enabled);
        if (activeTriggers.Count > 0)
        {
            Collider2D hit = activeTriggers[activeTriggers.Count - 1];
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                return;
            }
        }

        currentInteractable = null;
    }

    void TryInteract()
    {
        if (currentInteractable != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.leverInteractionSFX);
            currentInteractable.Interact();
        }
    }
}