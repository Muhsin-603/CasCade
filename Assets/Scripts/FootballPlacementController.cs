using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FootballPlacementController : MonoBehaviour
{
    [Header("Position Ring Visuals")]
    [SerializeField] private GameObject[] positionRings;

    [Header("Ring Projections")]
    [SerializeField] private GameObject[] projections;

    [Header("Selection")]
    [SerializeField] private float selectionRadius = 0.25f;

    [Header("Feather Placement")]
    [SerializeField] private float smoothTime = 0.35f;

    private XRGrabInteractable grabInteractable;
    private PuzzleObjectConfigurator configurator;

    private bool isHeld;
    private bool isSettling;

    private int currentCandidate = -1;

    private Vector3 restingPosition;
    private Quaternion restingRotation;

    private Vector3 settleVelocity;
    private Vector3 settleTarget;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        configurator = GetComponent<PuzzleObjectConfigurator>();

        restingPosition = transform.position;
        restingRotation = transform.rotation;
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

    private void Update()
    {
        if (isHeld)
        {
            UpdateCandidate();
            return;
        }

        if (isSettling)
        {
            UpdateSettlement();
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        isSettling = false;

        settleVelocity = Vector3.zero;

        ShowRings();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;

        int selectedConfiguration = currentCandidate;

        if (selectedConfiguration < 0)
        {
            // No valid ring selected.
            // Return to the middle/default position.
            selectedConfiguration = 1;
        }

        StartSettlement(selectedConfiguration);

        currentCandidate = -1;

        HideRings();
    }

    private void UpdateCandidate()
    {
        int closestRing = FindClosestRing();

        if (closestRing == currentCandidate)
            return;

        currentCandidate = closestRing;

        UpdateProjection();
    }

    private int FindClosestRing()
    {
        int closestIndex = -1;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < positionRings.Length; i++)
        {
            if (positionRings[i] == null)
                continue;

            float distance = Mathf.Abs(
                transform.position.x -
                positionRings[i].transform.position.x
            );

            if (distance <= selectionRadius &&
                distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void UpdateProjection()
    {
        HideAllProjections();

        if (currentCandidate < 0)
            return;

        if (currentCandidate >= projections.Length)
            return;

        if (projections[currentCandidate] == null)
            return;

        projections[currentCandidate].SetActive(true);
    }

    private void StartSettlement(int configurationIndex)
    {
        if (positionRings == null ||
            configurationIndex < 0 ||
            configurationIndex >= positionRings.Length)
        {
            return;
        }

        Transform targetRing = positionRings[configurationIndex].transform;

        settleTarget = new Vector3(
            targetRing.position.x,
            restingPosition.y,
            restingPosition.z
        );

        settleVelocity = Vector3.zero;
        isSettling = true;
    }

    private void UpdateSettlement()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            settleTarget,
            ref settleVelocity,
            smoothTime
        );

        transform.rotation = restingRotation;

        if (Vector3.Distance(transform.position, settleTarget) < 0.005f)
        {
            transform.position = settleTarget;
            transform.rotation = restingRotation;

            isSettling = false;
            settleVelocity = Vector3.zero;

            FinishSettlement();
        }
    }

    private void FinishSettlement()
    {
        if (configurator == null)
            return;

        int configurationIndex = FindConfigurationFromTarget();

        configurator.SnapToConfiguration(configurationIndex);

        HideRings();
    }

    private int FindConfigurationFromTarget()
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < positionRings.Length; i++)
        {
            if (positionRings[i] == null)
                continue;

            float distance = Mathf.Abs(
                settleTarget.x -
                positionRings[i].transform.position.x
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public void ShowRings()
    {
        SetRingsVisible(true);
    }

    public void HideRings()
    {
        SetRingsVisible(false);
        HideAllProjections();
    }

    private void SetRingsVisible(bool visible)
    {
        foreach (GameObject ring in positionRings)
        {
            if (ring != null)
                ring.SetActive(visible);
        }
    }

    private void HideAllProjections()
    {
        foreach (GameObject projection in projections)
        {
            if (projection != null)
                projection.SetActive(false);
        }
    }
}