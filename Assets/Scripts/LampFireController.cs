using UnityEngine;

public class LampFireController : MonoBehaviour
{
    [Header("Fire Effect")]
    [SerializeField] private GameObject fireEffect;

    private bool isOnFire = false;

    private void Awake()
    {
        if( fireEffect != null )
            fireEffect.SetActive(false);
    }

    public void Ignite()
    {
        if (isOnFire)
            return;

        isOnFire = true;

        if( fireEffect != null )
            fireEffect.SetActive(true);

        Debug.Log("Fire has started");
    }
}
