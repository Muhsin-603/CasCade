using System.Collections;
using UnityEngine;

public class MugController : MonoBehaviour
{
    [System.Serializable]
    public class MugFallPreset
    {
        public string presetName;
        public Vector3 launchDirection = Vector3.forward;
        public float launchForce = 2f;
        public float downwardForce = 0.5f;
        public Vector3 fallRotation;
    }

    [Header("References")]
    [SerializeField] private CoffeeFlowController coffeeFlowController;

    [Header("Fall Presets")]
    [SerializeField] private MugFallPreset[] fallPresets;

    [SerializeField] private int selectedPreset = 0;

    [Header("Water Spilling Animation")]
    [SerializeField] private Transform spillPoint;

    [Header("Floor Detection")]
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private float floorRaycastDistance = 2f;

    [Header("Settle Detection")]
    [SerializeField] private float settleTimeout = 5f;

    private Rigidbody rb;
    private bool hasFallen = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void ReleaseMug()
    {
        // Ignore repeat triggers once the mug is already falling.
        if (hasFallen)
            return;

        if (fallPresets == null || fallPresets.Length == 0)
        {
            Debug.LogWarning("MugController: No fall presets configured.");
            return;
        }

        selectedPreset = Mathf.Clamp(selectedPreset, 0, fallPresets.Length - 1);

        MugFallPreset preset = fallPresets[selectedPreset];

        hasFallen = true;

        transform.SetParent(null, true);

        // fallRotation is the mug's starting world rotation. The mouth points
        // along local +Z, so (0, yaw, 0) lays the mug on its side.
        transform.rotation = Quaternion.Euler(preset.fallRotation);

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Horizontal shove carries the mug clear of the table; gravity does the
        // falling. A zero direction normalizes to zero, so no guard is needed.
        Vector3 direction = preset.launchDirection;
        direction.y = 0f;

        rb.AddForce(
            direction.normalized * preset.launchForce + Vector3.down * preset.downwardForce,
            ForceMode.Impulse
        );

        StartCoroutine(WaitForMugToSettle());
    }

    private IEnumerator WaitForMugToSettle()
    {
        yield return new WaitForFixedUpdate();

        float deadline = Time.time + settleTimeout;

        while (!rb.IsSleeping() && Time.time < deadline)
            yield return null;

        // Spill only once settled, so we capture the final resting mouth
        // direction rather than a mid-tumble orientation.
        TriggerSpill();
    }

    private void TriggerSpill()
    {
        if (coffeeFlowController == null)
            return;

        Vector3 direction = GetSpillDirection();

        Debug.Assert(
            direction.y == 0f && Mathf.Approximately(direction.magnitude, 1f),
            "MugController: spill direction must be flat and unit-length."
        );

        coffeeFlowController.SetStartPosition(GetSpillPositionOnFloor(), direction);

        bool canIgnite = selectedPreset != 2;
        coffeeFlowController.StartFlow(canIgnite);
    }

    private Vector3 GetSpillPositionOnFloor()
    {
        // Start slightly above the mouth so the ray does not begin below the floor.
        Vector3 origin = spillPoint.position + Vector3.up * 0.05f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            floorRaycastDistance + 0.05f,
            floorLayer))
        {
            return hit.point;
        }

        Debug.LogWarning("MugController: Could not find floor below SpillPoint.");

        return spillPoint.position;
    }

    private Vector3 GetSpillDirection()
    {
        // The mug's mouth faces along SpillPoint.forward; once tipped over this
        // points horizontally in the direction the coffee should flow.
        Vector3 direction = spillPoint.forward;
        direction.y = 0f;

        // Mouth near-vertical: fall back to the tip-over axis. forward and up
        // are orthogonal, so they cannot both flatten to zero.
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = -spillPoint.up;
            direction.y = 0f;
        }

        return direction.normalized;
    }
}
