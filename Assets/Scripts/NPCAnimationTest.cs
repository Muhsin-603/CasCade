using UnityEngine;
using UnityEngine.InputSystem;

public class NPCAnimationTest : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            animator.SetTrigger("FallDeath");
        }
    }
}