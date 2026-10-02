using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class OneAxisGrabRotation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSensitivity = 5f;

    [Header("Rotation Targets")]

    [SerializeField] private Transform rotation1;
    [SerializeField] private Transform rotation2;
    [SerializeField] private Transform rotation3;

    [Header("Angle Indicator")]
    [SerializeField] private Transform angleClock;
    [SerializeField] private Transform arrowAnchor;

    [SerializeField] private Vector3 clockPositionOffset = new Vector3(0f, 0f, 1.5f);

    [Tooltip("Maximum angle difference required to count as a valid target.")]
    [SerializeField] private float selectionRadius = 15f;

    [Header("Smooth Snap")]
    [SerializeField] private float snapSpeed = 8f;


    private XRGrabInteractable grabInteractable;
    private IXRSelectInteractor currentInteractor;

    private PuzzleObjectConfigurator configurator;

    private float lastHandAngle;

    private bool isSnapping;
    private float targetRotationY;
    private int targetConfigurationIndex;

    private Quaternion clockInitialRotation;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        configurator = GetComponent<PuzzleObjectConfigurator>();

        if (angleClock != null)
        {
            clockInitialRotation = angleClock.rotation;
            angleClock.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
        {
            Debug.LogError(
                $"{name}: OneAxisGrabRotation requires an XRGrabInteractable."
            );

            return;
        }

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
        currentInteractor = args.interactorObject;

        isSnapping = false;

        lastHandAngle = GetHandAngle();

        if (angleClock != null)
            angleClock.gameObject.SetActive(true);

        FaceAngleClockToPlayer();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (args.interactorObject != currentInteractor)
            return;

        currentInteractor = null;

        BeginSnap();

        if (angleClock != null)
            angleClock.gameObject.SetActive(false);
    }

private void Update()
    {
        if (isSnapping)
        {
            SmoothSnap();
            return;
        }

        if (currentInteractor == null)
            return;

        FaceAngleClockToPlayer();

        float currentHandAngle = GetHandAngle();

        float angleDifference =
            Mathf.DeltaAngle(lastHandAngle, currentHandAngle);

        float rotationAmount =
            angleDifference * rotationSensitivity;

        transform.Rotate(
            0f,
            rotationAmount,
            0f,
            Space.World
        );

        float currentY = transform.eulerAngles.y;

        float clampedY = Mathf.Clamp(
            Mathf.DeltaAngle(rotation2.eulerAngles.y, currentY),
            -20f,
            20f
        );

        float targetY = rotation2.eulerAngles.y + clampedY;

        transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
            targetY,
            transform.eulerAngles.z
        );

        UpdateAngleIndicator();

        lastHandAngle = currentHandAngle;
    }

    private float GetHandAngle()
    {
        Vector3 direction =
            currentInteractor.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return lastHandAngle;

        return Mathf.Atan2(
            direction.x,
            direction.z
        ) * Mathf.Rad2Deg;
    }

    private void BeginSnap()
    {
        float currentY = transform.eulerAngles.y;

        float rotation1Y = rotation1.eulerAngles.y;
        float rotation2Y = rotation2.eulerAngles.y;
        float rotation3Y = rotation3.eulerAngles.y;

        float difference1 =
            Mathf.Abs(Mathf.DeltaAngle(currentY, rotation1Y));

        float difference2 =
            Mathf.Abs(Mathf.DeltaAngle(currentY, rotation2Y));

        float difference3 =
            Mathf.Abs(Mathf.DeltaAngle(currentY, rotation3Y));

        float closestDifference = difference1;

        targetRotationY = rotation1Y;
        targetConfigurationIndex = 0;

        if (difference2 < closestDifference)
        {
            closestDifference = difference2;
            targetRotationY = rotation2Y;
            targetConfigurationIndex = 1;
        }

        if (difference3 < closestDifference)
        {
            closestDifference = difference3;
            targetRotationY = rotation3Y;
            targetConfigurationIndex = 2;
        }

        // No valid target:
        // return to Rotation_2.
        if (closestDifference > selectionRadius)
        {
            targetRotationY = rotation2Y;
            targetConfigurationIndex = 1;
        }

        isSnapping = true;
    }

    private void SmoothSnap()
    {
        float currentY = transform.eulerAngles.y;

        float newY = Mathf.LerpAngle(
            currentY,
            targetRotationY,
            snapSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(
            0f,
            newY,
            0f
        );

        UpdateAngleIndicator();

        float remainingAngle =
            Mathf.Abs(Mathf.DeltaAngle(newY, targetRotationY));

        if (remainingAngle < 0.1f)
        {
            transform.rotation = Quaternion.Euler(
                0f,
                targetRotationY,
                0f
            );

            UpdateAngleIndicator();

            isSnapping = false;

            if (configurator != null)
            {
                configurator.SnapToConfiguration(
                    targetConfigurationIndex
                );
            }
        }
    }

    private void UpdateAngleIndicator()
    {
        if (arrowAnchor == null)
            return;

        float chairAngle =
            Mathf.DeltaAngle(
                rotation2.eulerAngles.y,
                transform.eulerAngles.y
            );

        float indicatorAngle =
            Mathf.Lerp(
                -70f,
                70f,
                Mathf.InverseLerp(-20f, 20f, chairAngle)
            );

        arrowAnchor.localRotation =
            Quaternion.Euler(
                0f,
                indicatorAngle,
                0f
            );
    }

    private void FaceAngleClockToPlayer()
    {
        if (angleClock == null)
            return;

        Camera playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        // Position the clock in front of the player's camera.
        angleClock.position =
            playerCamera.transform.position +
            playerCamera.transform.forward *
            clockPositionOffset.z +
            playerCamera.transform.right *
            clockPositionOffset.x +
            Vector3.up *
            clockPositionOffset.y;

        Vector3 clockForward =
            clockInitialRotation * Vector3.up;

        Vector3 cameraForward =
            playerCamera.transform.position - angleClock.position;

        clockForward.y = 0f;
        cameraForward.y = 0f;

        if (clockForward.sqrMagnitude < 0.001f ||
            cameraForward.sqrMagnitude < 0.001f)
            return;

        clockForward.Normalize();
        cameraForward.Normalize();

        float yaw =
            Vector3.SignedAngle(
                clockForward,
                cameraForward,
                Vector3.up
            );

        angleClock.rotation =
            Quaternion.AngleAxis(
                yaw,
                Vector3.up
            ) * clockInitialRotation;
    }

}