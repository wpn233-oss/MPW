using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Ωªª•…Ë÷√")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRadius = 3f;
    public LayerMask interactableLayer;

    private Interactable currentInteractable;

    void Update()
    {
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            interactableLayer
        );

        Interactable newInteractable = null;
        foreach (var collider in hitColliders)
        {
            Interactable interactable = collider.GetComponent<Interactable>();
            if (interactable != null && interactable.CanInteract)
            {
                newInteractable = interactable;
                break; 
            }
        }

        if (newInteractable != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.SetFocus(false);
            }

            currentInteractable = newInteractable;

            if (currentInteractable != null)
            {
                currentInteractable.SetFocus(true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}