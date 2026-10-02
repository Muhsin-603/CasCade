using UnityEngine;

public class CoffeeFlowController : MonoBehaviour
{
    [Header("Coffee Flow")]
    [SerializeField] private Renderer coffeeRenderer;
    [SerializeField] private Transform spillPointStart;

    [Header("Flow Settings")]
    [SerializeField] private float flowDuration = 4f;

    [Header("Shader Appearance")]
    [SerializeField] private Vector4 Appearance;

    [Header("Fire Trigger")]
    [SerializeField] private LampFireController lampFireController;
    [SerializeField] private float fireTriggerReveal = 0.85f;
    private bool fireTriggered = false;
    private bool canTriggerFire = false;

    private Material coffeeMaterial;
    private float reveal = 0f;
    private bool flowing = false;

    private void Start()
    {
        coffeeMaterial = coffeeRenderer.material;
        coffeeMaterial.SetFloat("_Reveal", 0f);
    }

    private void Update()
    {
        if (!flowing)
            return;

        reveal = Mathf.Clamp01(reveal + Time.deltaTime / flowDuration);

        coffeeMaterial.SetFloat("_Reveal", reveal);

        if (canTriggerFire && !fireTriggered && reveal >= fireTriggerReveal)
        {
            fireTriggered = true;

            if (lampFireController != null)
                lampFireController.Ignite();
        }

        if (reveal >= 1f)
            flowing = false;
    }

    public void StartFlow(bool canIgnite)
    {
        if (coffeeMaterial == null)
            return;

        reveal = 0f;
        flowing = true;
        fireTriggered = false;

        canTriggerFire = canIgnite;

        coffeeMaterial.SetFloat("_Reveal", reveal);
    }

    public void SetStartPosition(Vector3 position, Vector3 spillDirection)
    {
        if (spillPointStart == null)
        {
            Debug.LogWarning("CoffeeFlowController: SpillPointStart is not assigned.");
            return;
        }

        float yaw = Mathf.Atan2(-spillDirection.z, spillDirection.x) * Mathf.Rad2Deg;

        // Rotate first — anchor correction below depends on the rotated child position.
        transform.rotation = Quaternion.Euler(90f, yaw, 0f);

        // Re-anchor so SpillPointStart (not the puddle's own pivot) lands at `position`.
        Vector3 anchorCorrection = position - spillPointStart.position;
        transform.position += anchorCorrection;

        transform.position += Vector3.up * 0.003f;

        Debug.Log($"CoffeeFlowController: Setting puddle at position {position}, yaw={yaw}");
    }
}
