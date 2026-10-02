using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ConstrainedObjectController : MonoBehaviour
{
    public enum ControlMode
    {
        PositionX,
        Rotation
    }

    [Header("Control")]
    [SerializeField] private ControlMode controlMode = ControlMode.PositionX;

    [Header("Movement")]
    [SerializeField] private float sensitivity = 1f;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float maxRotation = 45f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Transform interactor;

    private Vector3 grabStartPosition;
    private Vector3 objectStartPosition;
    private Quaternion grabStartRotation;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject.transform;

        grabStartPosition = interactor.position;
        objectStartPosition = transform.position;
        grabStartRotation = transform.rotation;

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void LateUpdate()
    {
        if (interactor == null)
            return;

        switch (controlMode)
        {
            case ControlMode.PositionX:
                HandleXMovement();
                break;

            case ControlMode.Rotation:
                HandleRotation();
                break;
        }
    }

    private void HandleXMovement()
    {
        float movement =
            (interactor.position.x - grabStartPosition.x) * sensitivity;

        transform.position = new Vector3(
            objectStartPosition.x + movement,
            objectStartPosition.y,
            objectStartPosition.z
        );
    }

    private void HandleRotation()
    {
        float movement =
            (interactor.position.x - grabStartPosition.x) * sensitivity;

        float rotationAmount = Mathf.Clamp(
            movement,
            -maxRotation,
            maxRotation
        );

        transform.rotation =
            grabStartRotation *
            Quaternion.AngleAxis(
                rotationAmount,
                rotationAxis.normalized
            );

        transform.position = objectStartPosition;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        interactor = null;
    }
}