using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : BaseScreen
{
    public TextMeshProUGUI scoreText;
    public ProgressBarController progressBarController;
    public VFXController vfxController;
    public GameObject panel;
    public Image decor;
    private void Start()
    {
        this.Register(EventID.SetupGameScreen, Setup);
    }
    public override void Init()
    {
        base.Init();
        // Initialize text fields to default values.
        // Actual values will be set by HandleScoreUpdate via ScoreManager's event or in Show().
        scoreText.text = "0";
    }

    public override void Show(object data)
    {
        base.Show(data);
        ScoreManager.Instance.OnScoreUpdated += HandleScoreUpdate; // Subscribe when screen is shown
    }
    public void StartBtn()
    {
        GameManager.Instance.LoadGame(null);
        panel.SetActive(false); // Hide the start panel when the game starts
    }
    public void Setup(object data)
    {
        ScoreManager.Instance.OnScoreUpdated += HandleScoreUpdate; // Subscribe when screen is shown

        scoreText.text = "0";
        if(data is float songTime)
        {
            progressBarController.Setup(6, songTime); // Assuming 6 stars and passing the song time
        }
        else
        {
            Debug.LogWarning("GameScreen.Setup: Expected float data for song time, but received: " + data);
        }
    }
    public override void Hide()
    {
        ScoreManager.Instance.OnScoreUpdated -= HandleScoreUpdate; // Unsubscribe when screen is hidden
        base.Hide();
    }

    private void HandleScoreUpdate(int newScore, string newRateText, int multiplier)
    {
        scoreText.text = newScore.ToString();
        vfxController.PlayVFXWithMultiplier(newRateText, multiplier);
        decor.DOFade(1, 0.1f).OnComplete(() =>
        {
            decor.DOFade(0.6f, 0.2f);
        });
    }
}
