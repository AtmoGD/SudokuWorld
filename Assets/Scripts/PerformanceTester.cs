using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class PerformanceTester : MonoBehaviour
{
    [SerializeField] private DifficultySetting randomDifficulty;
    [SerializeField] private int iterations = 1000;

    private void Start()
    {
        TestPerformance();
    }

    private async void TestPerformance()
    {
        float totalTime = 0;
        float maxTime = 0;
        float minTime = float.MaxValue;

        for (int i = 0; i < iterations; i++)
        {
            float startTime = Time.realtimeSinceStartup;
            int currentDifficulty = Random.Range(randomDifficulty.range.x, randomDifficulty.range.y);
            await Task.Run(() => SudokuGenerator.GeneratePuzzle(currentDifficulty));
            float endTime = Time.realtimeSinceStartup;
            float time = endTime - startTime;
            totalTime += time;
            maxTime = Mathf.Max(maxTime, time);
            minTime = Mathf.Min(minTime, time);
        }

        float averageTime = totalTime / iterations;

        Debug.Log($"Average time: {averageTime} seconds");
        Debug.Log($"Max time: {maxTime} seconds");
        Debug.Log($"Min time: {minTime} seconds");
        Debug.Log($"Total time: {totalTime} seconds");
    }
}
