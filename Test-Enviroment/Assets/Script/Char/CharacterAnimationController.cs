using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator animator;

    private void Update()
    {
        if (playerController == null || animator == null) return;

        animator.SetFloat("Speed", playerController.CurrentHorizontalSpeed);
        animator.SetBool("IsSprinting", playerController.IsSprinting);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (animator == null) animator = GetComponent<Animator>();
    }
#endif
}