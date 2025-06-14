using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public GameObject starPrefab; // Prefab for the star UI element
    public GameObject starContainer; // Parent container for the stars
    public Slider progressBar; // Reference to the progress bar UI element
    private List<StarAnimation> _starAnimations = new(); // Array to hold the star animations
    private bool _canPlaying = false; // Flag to check if the animation is playing
    private float _songTime; // Total time of the song
    private float _currentTime = 0; // Current time in the song
    private int _currentStarIndex = 0; // Index of the current star being animated

    private void Update()
    {
        if (_canPlaying)
        {
            _currentTime += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(_currentTime / _songTime); // Update the progress bar value smoothly
        }
    }
    private void FixedUpdate()
    {
        if (_canPlaying)
        {
            // Check if it's time to animate the next star based on fixed time steps
            if (_currentStarIndex < _starAnimations.Count && 
                _currentTime >= _songTime / _starAnimations.Count * (_currentStarIndex + 1))
            {
                _starAnimations[_currentStarIndex].ChangeStatus(); // Trigger the star animation
                _currentStarIndex++;
            }
        }
    }
    [Button("Setup Progress Bar")]
    public void Setup(int numberOfStars, float songTime)
    {
        _songTime = songTime; // Set the total song time
        float width = 700 + (numberOfStars - 3) * (400 / 3); // Adjust width based on the number of stars, assuming 3 is the base case
        ((RectTransform)this.transform).sizeDelta = new Vector2(width, ((RectTransform)this.transform).sizeDelta.y);
        float distanceBetweenStars = width / (numberOfStars);
        for (int i = 0; i < numberOfStars; i++)
        {
            GameObject star = Instantiate(starPrefab, starContainer.transform);
            star.transform.localPosition = new Vector3((i + 1) * distanceBetweenStars, 0, 0);
            star.SetActive(true);
            StarAnimation starAnimation = star.GetComponent<StarAnimation>();
            _starAnimations.Add(starAnimation);
        }
        _canPlaying = true; // Enable the animation
    }
}
