using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Objects")]
    [SerializeField] private PuzzleObjectConfigurator football;
    [SerializeField] private PuzzleObjectConfigurator chair;
    [SerializeField] private PuzzleObjectConfigurator mug;

    public void CheckConfiguration()
    {
        Debug.Log(
            $"CONFIGURATION | " +
            $"Football: {football.CurrentConfiguration} | " +
            $"Chair: {chair.CurrentConfiguration} | " +
            $"Mug: {mug.CurrentConfiguration}"
        );
    }
}