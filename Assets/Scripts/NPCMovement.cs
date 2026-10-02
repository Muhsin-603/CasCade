using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform kickPoint;  
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;

    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private Transform kickTarget;
    [SerializeField] private float kickForce = 5f;

    private Animator animator;

    private bool kickApplied;

private bool hasKicked;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        MoveToKickPoint();
    }

    private void MoveToKickPoint()
    {
        Vector3 direction = kickPoint.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            animator.SetBool("IsWalking", false);

            if (!hasKicked)
            {
                hasKicked = true;
                animator.Play("kick");
            }

            return;
        }

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        animator.SetBool("IsWalking", true);
    }

public void KickBall()
    {
        if (kickApplied)
            return;

        Debug.Log("Ball was Kicked");

        if (ballRigidbody == null || kickTarget == null)
            return;

        Vector3 kickDirection = Vector3.ProjectOnPlane(
            kickTarget.position - ballRigidbody.position,
            Vector3.up
        ).normalized;

        if (kickDirection.sqrMagnitude <= 0.001f)
            return;

        kickApplied = true;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.WakeUp();
        ballRigidbody.AddForce(kickDirection * kickForce, ForceMode.Impulse);
    }
}
