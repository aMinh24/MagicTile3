using UnityEngine;

public class ScoreManager : BaseManager<ScoreManager>
{

    public int CurrentScore { get; private set; }
    public int PerfectCombo { get; private set; }
    public string CurrentRateText { get; private set; } = "";

    private const int BASE_PERFECT_SCORE = 10;
    private const int BASE_GOOD_SCORE = 5;

    public event System.Action<int, string, int> OnScoreUpdated; // int: new score, string: rate text

    public void RecordHit(bool isPerfect)
    {
        if (isPerfect)
        {
            PerfectCombo++;
            CurrentScore += BASE_PERFECT_SCORE * PerfectCombo;
            CurrentRateText = "perfect";
        }
        else
        {
            PerfectCombo = 0; // Reset combo on a "Good" hit or miss
            CurrentScore += BASE_GOOD_SCORE;
            CurrentRateText = "good";
        }
        OnScoreUpdated?.Invoke(CurrentScore, CurrentRateText, PerfectCombo);
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        PerfectCombo = 0;
        CurrentRateText = ""; // Initial rate text (e.g., empty or "Start!")
        OnScoreUpdated?.Invoke(CurrentScore, CurrentRateText, 0);
    }
}
