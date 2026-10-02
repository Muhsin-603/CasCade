using UnityEngine;

public class WaterWobbleController : MonoBehaviour
{

    [Header("Wobble")]
    [SerializeField] private float wobbleAmount = 0.015f;
    [SerializeField] private float wobbleSpeed = 4f;

    [Header("References")]
    [SerializeField] private Rigidbody mugRigidbody;

    private Vector3 velocity;
    private Vector3 angularVelocity;

    private Material waterMaterial;
    private MeshRenderer waterRenderer;

    void Start()
    {
        waterRenderer = GetComponent<MeshRenderer>();
        waterMaterial = waterRenderer.material;
    }

    private void Update()
    {
        if (mugRigidbody == null) return;

        velocity = mugRigidbody.linearVelocity;
        angularVelocity = mugRigidbody.angularVelocity;

        float movement = Mathf.Clamp01(velocity.magnitude * 0.1f + angularVelocity.magnitude * 0.02f);

        if (movement < 0.05f)
        {
            waterMaterial.SetFloat("_WaveHeight", 0f);
            waterMaterial.SetFloat("_WaveSpeed", 0f);
            return;
        }

        float finalWobble = movement * wobbleAmount;

        waterMaterial.SetFloat("_WaveHeight", finalWobble);
        waterMaterial.SetFloat("_WaveSpeed", wobbleSpeed + movement * 5f);

    }

}