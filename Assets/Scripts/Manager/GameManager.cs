using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : BaseManager<GameManager>
{
    public float cameraSpeedFactor = 1.0f; // Speed multiplier for camera movement
    public SpawnTileController spawnTileController;
    public CameraMove cameraMove;
    public DataSong currentDataSong; // Current song data being played
    public bool isGameStarted = false; // Flag to check if the game has started
    private void Start() {
        Application.targetFrameRate = 60; // Set target FPS to 60
    }
    [Button("Load Game")]
    public void LoadGame(DataSong dataSong)
    {
        if (dataSong == null)
        {
            dataSong = currentDataSong;
        }
        spawnTileController.SpawnTilesFromData(dataSong);
        AudioManager.Instance.PlayBGM(dataSong.songName); // Play the background music for the song
        // AudioManager.Instance.AttachBGMSource // This line seems incomplete, assuming PlayBGM returns the clip or you have a way to get it.
        AudioClip bgmClip = AudioManager.Instance.AttachBGMSource.clip;
        if (bgmClip != null && cameraMove != null)
        {
            float songDuration = bgmClip.length;
            this.Broadcast(EventID.SetupGameScreen, songDuration); // Broadcast the song duration to setup the game screen
            float calculatedMaxHeight = songDuration * CONST.CAMERA_SPEED * cameraSpeedFactor; // Calculate max height based on song duration and camera speed factor
            cameraMove.SetMaxHeightAndResetPosition(calculatedMaxHeight);
            cameraMove.UpdateSpeed(); // Ensure speed is also updated
            cameraMove.canMove = true; // Allow camera movement
            isGameStarted = true; // Set game started flag to true
        }
        else
        {
            if (bgmClip == null) Debug.LogError("BGM clip is null. Cannot calculate maxHeight.");
            if (cameraMove == null) Debug.LogError("CameraMove reference is not set in GameManager.");
        }
    }

    public void EndGame()
    {
        isGameStarted = false;
        cameraMove.canMove = false; // Stop camera movement
        AudioManager.Instance.AttachBGMSource.Stop(); // Stop the background music
        AudioManager.Instance.PlaySE("gameover");
    }
}
