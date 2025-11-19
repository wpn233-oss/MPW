using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private float checkDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundedCooldown = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    public bool IsGrounded { get; private set; }
    private float _lastUngroundedTime;

    void FixedUpdate()
    {
        CheckGround();
    }

    private void CheckGround()
    {
        bool hitGround = Physics.SphereCast(
            transform.position + Vector3.up * checkRadius,
            checkRadius,
            Vector3.down,
            out RaycastHit hit,
            checkDistance + checkRadius,
            groundLayer
        );

        if (hitGround)
        {
            IsGrounded = true;
        }
        else
        {
            if (IsGrounded)
            {
                _lastUngroundedTime = Time.time;
                IsGrounded = false;
            }
            else
            {
                IsGrounded = (Time.time - _lastUngroundedTime) < groundedCooldown;
            }
        }
    }

    // 显示地面检测调试视图
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * checkRadius;
        Gizmos.DrawWireSphere(origin, checkRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * (checkDistance + checkRadius));
        Gizmos.DrawWireSphere(origin + Vector3.down * (checkDistance + checkRadius), checkRadius);
    }
}