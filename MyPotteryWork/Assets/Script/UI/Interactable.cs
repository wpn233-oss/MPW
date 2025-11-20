using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("交互设置")]
    public string interactionName = "交互";
    public float interactionRadius = 2f;

    public bool CanInteract { get; protected set; } = true;
    public bool HasFocus { get; private set; }

    private InteractionUI interactionUI;

    void Start()
    {
        interactionUI = GetComponentInChildren<InteractionUI>();
        if (interactionUI != null)
        {
            interactionUI.SetInteractionName(interactionName);
            interactionUI.Hide();
        }
    }

    public virtual void Interact()
    {
        Debug.Log($"与 {gameObject.name} 交互");
    }

    public void SetFocus(bool focused)
    {
        HasFocus = focused;

        if (interactionUI != null)
        {
            if (focused) interactionUI.Show();
            else interactionUI.Hide();
        }
    }

    public void SetInteractable(bool canInteract)
    {
        CanInteract = canInteract;
        if (!canInteract && HasFocus) SetFocus(false);
    }
}