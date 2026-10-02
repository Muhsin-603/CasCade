using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PuzzleObjectConfigurator : MonoBehaviour
{
    public enum ConfigurationType
    {
        Position,
        Rotation
    }

    [Header("Configuration")]
    [SerializeField] private ConfigurationType configurationType;

    [Header("Default State")]
    [SerializeField] private Transform defaultConfiguration;

    [Header("Three Player Configurations")]
    [SerializeField] private Transform[] configurations = new Transform[3];

    [Header("Release Behavior")]
    [SerializeField] private bool snapOnRelease = true;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private bool isQuitting;

    public int CurrentConfiguration { get; private set; } = -1;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (isQuitting)
            return;

        if (!snapOnRelease)
            return;

        SnapToNearestConfiguration();
    }

    public void SetDefault()
    {
        if (defaultConfiguration == null)
        {
            Debug.LogError($"{name}: Default Configuration is not assigned.");
            return;
        }

        StopPhysics();

        if (configurationType == ConfigurationType.Position)
        {
            transform.position = new Vector3(
                defaultConfiguration.position.x,
                transform.position.y,
                transform.position.z
            );
        }
        else
        {
            transform.rotation = defaultConfiguration.rotation;
        }

        CurrentConfiguration = -1;
    }

    public void SnapToConfiguration(int index)
    {
        if (configurations == null || configurations.Length != 3)
        {
            Debug.LogError($"{name}: Assign exactly 3 configurations.");
            return;
        }

        index = Mathf.Clamp(index, 0, 2);

        Transform target = configurations[index];

        if (target == null)
        {
            Debug.LogError($"{name}: Configuration {index} is not assigned.");
            return;
        }

        StopPhysics();

        if (configurationType == ConfigurationType.Position)
        {
            transform.position = new Vector3(
                target.position.x,
                transform.position.y,
                transform.position.z
            );
        }
        else
        {
            transform.rotation = target.rotation;
        }

        CurrentConfiguration = index;
    }

    private void SnapToNearestConfiguration()
    {
        if (configurations == null || configurations.Length != 3)
            return;

        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < configurations.Length; i++)
        {
            if (configurations[i] == null)
                continue;

            float distance;

            if (configurationType == ConfigurationType.Position)
            {
                distance = Mathf.Abs(
                    transform.position.x - configurations[i].position.x
                );
            }
            else
            {
                distance = Quaternion.Angle(
                    transform.rotation,
                    configurations[i].rotation
                );
            }

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        SnapToConfiguration(nearestIndex);
    }

    private void StopPhysics()
    {
        if (rb == null)
            return;

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.isKinematic = true;
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
}