using UnityEngine;

public class TableImpactTrigger : MonoBehaviour
{
    [SerializeField] private MugController mugController;
    
    private bool hasTriggered = false;

private void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered)
            return;

        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody == null || !otherRigidbody.CompareTag("Chair"))
            return;

        hasTriggered = true;

        if (mugController != null)
        {
            mugController.ReleaseMug();
            Debug.Log("MUG RELEASED");
        }
    }
}
