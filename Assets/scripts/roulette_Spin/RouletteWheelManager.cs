using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RouletteWheelManager : MonoBehaviour
{
    [Header("Wheel References")]
    public Transform wheelContainer;
    public Button wheelButton;
    public TextMeshProUGUI tapInstruction;
    public Transform wheelPointer; 
    [Header("Wheel Visual Components")]
    public GameObject resultPanel;
    public Image[] wheelSegments; 
    public TextMeshProUGUI[] segmentTexts; 

    [Header("Roulette Events")]
    public RouletteEventData[] rouletteEvents;

    [Header("Spin Settings")]
    [Range(2.0f, 6.0f)]
    public float minSpinDuration = 2.5f;
    [Range(3.0f, 8.0f)]
    public float maxSpinDuration = 5.0f;
    [Range(360f, 720f)]
    public float minSpinSpeed = 540f;
    [Range(720f, 2160f)]
    public float maxSpinSpeed = 1800f;

    [Header("Visual Feedback")]
    public ParticleSystem spinParticles; 
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); 
    [Header("Audio")]
    public AudioSource spinAudioSource;
    public AudioClip spinSound;
    public AudioClip tickSound; 
    public AudioClip stopSound;

    [Header("Result Panel")]
    public GameObject ResultPanel; 
    public TextMeshProUGUI resultEventTitle;
    public TextMeshProUGUI resultEventDescription;
    public Button resultContinueButton;
    public float popupAnimationDuration = 0.5f;

    [Header("Debug")]
    public bool debugMode = true;

  
    private bool isSpinning = false;
    private RouletteEventData selectedEvent;
    private float currentWheelRotation = 0f;
    private int totalSpins = 0;

    #region Initialization
    void Start()
    {
        SetupWheel();
        ValidateSetup();

        if (resultPanel != null)
            ResultPanel.SetActive(false);
    }

    void SetupWheel()
    {
        // Setup wheel button
        if (wheelButton != null)
        {
            wheelButton.onClick.AddListener(SpinWheel);
            wheelButton.interactable = true;
        }
        else
        {
            Debug.LogError("Wheel button not assigned!");
        }

        // Setup wheel segments with event data
        SetupWheelSegments();

        // Start tap instruction animation
        if (tapInstruction != null)
        {
            StartCoroutine(AnimateTapInstruction());
        }

        // Initialize wheel rotation
        if (wheelContainer != null)
        {
            currentWheelRotation = wheelContainer.eulerAngles.z;
        }
    }

    void SetupWheelSegments()
    {
        if (rouletteEvents == null || rouletteEvents.Length == 0)
        {
            Debug.LogError("No roulette events assigned!");
            return;
        }

        // Setup visual segments if available
        if (wheelSegments != null && wheelSegments.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(wheelSegments.Length, rouletteEvents.Length); i++)
            {
                if (wheelSegments[i] != null && rouletteEvents[i] != null)
                {
                    wheelSegments[i].color = rouletteEvents[i].eventColor;
                }
            }
        }

        // Setup segment texts if available
        if (segmentTexts != null && segmentTexts.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(segmentTexts.Length, rouletteEvents.Length); i++)
            {
                if (segmentTexts[i] != null && rouletteEvents[i] != null)
                {
                    segmentTexts[i].text = rouletteEvents[i].eventTitle;
                    segmentTexts[i].color = GetContrastingTextColor(rouletteEvents[i].eventColor);
                }
            }
        }

        if (debugMode)
        {
            Debug.Log($"Wheel setup complete with {rouletteEvents.Length} events");
        }
    }

    void ValidateSetup()
    {
        List<string> errors = new List<string>();

        if (wheelContainer == null) errors.Add("Wheel Container not assigned");
        if (wheelButton == null) errors.Add("Wheel Button not assigned");
        if (rouletteEvents == null || rouletteEvents.Length == 0) errors.Add("No Roulette Events assigned");

        if (errors.Count > 0)
        {
            Debug.LogError("RouletteWheelManager Setup Errors:\n" + string.Join("\n", errors));
        }
    }
    #endregion

    #region Animation Methods
    IEnumerator AnimateTapInstruction()
    {
        if (tapInstruction == null) yield break;

        while (!isSpinning && tapInstruction.gameObject.activeInHierarchy)
        {
            // Fade in
            yield return StartCoroutine(FadeText(tapInstruction, 0.3f, 1.0f, 1.0f));
            // Brief pause
            yield return new WaitForSeconds(0.5f);
            // Fade out
            yield return StartCoroutine(FadeText(tapInstruction, 1.0f, 0.3f, 1.0f));
            // Brief pause
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator FadeText(TextMeshProUGUI text, float fromAlpha, float toAlpha, float duration)
    {
        if (text == null) yield break;

        float elapsed = 0;
        Color originalColor = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, toAlpha);
    }
    #endregion

    #region Spin Logic
    public void SpinWheel()
    {
        if (isSpinning)
        {
            if (debugMode) Debug.Log("Wheel is already spinning!");
            return;
        }

        if (rouletteEvents == null || rouletteEvents.Length == 0)
        {
            Debug.LogError("Cannot spin: No roulette events available!");
            return;
        }

        StartCoroutine(SpinWheelCoroutine());
    }

    IEnumerator SpinWheelCoroutine()
    {
        isSpinning = true;
        totalSpins++;

        if (debugMode) Debug.Log($"Starting spin #{totalSpins}");

        // Disable wheel button during spin
        if (wheelButton != null)
            wheelButton.interactable = false;

        // Hide tap instruction
        if (tapInstruction != null)
            tapInstruction.gameObject.SetActive(false);

        // Pre-select the winning event
        selectedEvent = SelectRandomEvent();
        if (debugMode) Debug.Log($"Pre-selected event: {selectedEvent.eventTitle}");

        // Start visual effects
        if (spinParticles != null)
            spinParticles.Play();

        // Start audio
        PlaySpinAudio();

        // Calculate spin parameters
        float spinDuration = Random.Range(minSpinDuration, maxSpinDuration);
        float baseSpinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);

        // Calculate target angle for selected event
        float targetAngle = CalculateTargetAngle(selectedEvent);
        float totalRotation = baseSpinSpeed * spinDuration + targetAngle;

        if (debugMode) Debug.Log($"Spin duration: {spinDuration}s, Target angle: {targetAngle}°, Total rotation: {totalRotation}°");

        // Perform the spin animation
        yield return StartCoroutine(PerformSpinAnimation(spinDuration, totalRotation));

        // Stop visual effects
        if (spinParticles != null)
            spinParticles.Stop();

        // Play stop sound
        if (spinAudioSource != null && stopSound != null)
        {
            spinAudioSource.PlayOneShot(stopSound);
        }

        // Brief pause for effect
        yield return new WaitForSeconds(0.5f);

        // Save result and transition
        SaveResultAndTransition();
    }

    IEnumerator PerformSpinAnimation(float duration, float totalRotation)
    {
        if (wheelContainer == null) yield break;

        float elapsed = 0;
        float startRotation = currentWheelRotation;
        float endRotation = startRotation + totalRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Apply spin curve for realistic deceleration
            float curveValue = spinCurve.Evaluate(progress);
            float currentRotation = Mathf.Lerp(startRotation, endRotation, curveValue);

            // Apply rotation
            wheelContainer.rotation = Quaternion.Euler(0, 0, currentRotation);
            currentWheelRotation = currentRotation;

            // Optional: Play tick sounds as wheel slows down
            if (tickSound != null && progress > 0.7f)
            {
                // Play occasional tick sounds
                if (Random.Range(0f, 1f) < 0.1f) // 10% chance per frame
                {
                    spinAudioSource.PlayOneShot(tickSound, 0.3f);
                }
            }

            yield return null;
        }

        // Ensure final rotation
        wheelContainer.rotation = Quaternion.Euler(0, 0, endRotation);
        currentWheelRotation = endRotation;

        if (debugMode) Debug.Log($"Final wheel rotation: {endRotation}°");
    }
    #endregion

    #region Event Selection
    RouletteEventData SelectRandomEvent()
    {
        if (rouletteEvents == null || rouletteEvents.Length == 0) return null;

        // Simple random selection (you can add weighting here)
        int randomIndex = Random.Range(0, rouletteEvents.Length);
        return rouletteEvents[randomIndex];
    }

    float CalculateTargetAngle(RouletteEventData targetEvent)
    {
        if (rouletteEvents == null || rouletteEvents.Length == 0) return 0f;

        int eventIndex = System.Array.IndexOf(rouletteEvents, targetEvent);
        if (eventIndex == -1) return 0f;

        // Calculate the angle for this event's segment
        float segmentAngle = 360f / rouletteEvents.Length;
        float eventCenterAngle = eventIndex * segmentAngle;

        // Add some randomness within the segment
        float randomOffset = Random.Range(-segmentAngle * 0.3f, segmentAngle * 0.3f);

        return eventCenterAngle + randomOffset;
    }
    #endregion

    #region Audio
    void PlaySpinAudio()
    {
        if (spinAudioSource == null) return;

        if (spinSound != null)
        {
            spinAudioSource.clip = spinSound;
            spinAudioSource.loop = true;
            spinAudioSource.Play();
        }
    }

    void StopSpinAudio()
    {
        if (spinAudioSource == null) return;

        spinAudioSource.loop = false;
        spinAudioSource.Stop();
    }
    #endregion

    #region Utility Methods
    Color GetContrastingTextColor(Color backgroundColor)
    {
        // Calculate luminance
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }

    void SaveResultAndTransition()
    {
        if (selectedEvent == null)
        {
            Debug.LogError("No event selected!");
            return;
        }

        // Save selected event data for result scene
        PlayerPrefs.SetString("SelectedEventTitle", selectedEvent.eventTitle);
        PlayerPrefs.SetString("SelectedEventDescription", selectedEvent.eventDescription);
        PlayerPrefs.SetString("SelectedEventType", selectedEvent.eventType.ToString());
        PlayerPrefs.SetInt("SelectedEventIsPositive", selectedEvent.isPositive ? 1 : 0);
        PlayerPrefs.Save();

        if (debugMode) Debug.Log($"Saved result: {selectedEvent.eventTitle}");

        // Load result scene
        ShowResultPanel();
    }
    void ShowResultPanel()
    {
        // Set the result panel content
        if (resultEventTitle != null)
            resultEventTitle.text = selectedEvent.eventTitle;

        if (resultEventDescription != null)
            resultEventDescription.text = selectedEvent.eventDescription;
        if (resultContinueButton != null)
            resultContinueButton.onClick.AddListener(ReturnToMainMenu);
        // Show the panel with animation
        StartCoroutine(ShowResultPopupAnimation());
    }
    public void ReturnToMainMenu()
    {
        // Clear saved event data
        PlayerPrefs.DeleteKey("SelectedEventTitle");
        PlayerPrefs.DeleteKey("SelectedEventDescription");

        // Return to main menu
        SceneManager.LoadScene("MenuScene");
    }

    IEnumerator ShowResultPopupAnimation()
    {
        // Initially set panel to zero scale
        if (ResultPanel != null)
        {
            ResultPanel.transform.localScale = Vector3.zero;
            ResultPanel.SetActive(true);
        }

        // Animate popup appearing
        float elapsed = 0;
        while (elapsed < popupAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0, 1, elapsed / popupAnimationDuration);

            // Ease out animation
            scale = 1 - Mathf.Pow(1 - scale, 2);

            ResultPanel.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        ResultPanel.transform.localScale = Vector3.one;
    }
  
    public void ForceStopSpin()
    {
        StopAllCoroutines();
        isSpinning = false;

        if (wheelButton != null)
            wheelButton.interactable = true;

        if (spinParticles != null)
            spinParticles.Stop();

        StopSpinAudio();
    }
    #endregion

    #region Debug Methods
    [ContextMenu("Test Spin")]
    void TestSpin()
    {
        if (Application.isPlaying)
        {
            SpinWheel();
        }
    }

    [ContextMenu("Debug Event List")]
    void DebugEventList()
    {
        if (rouletteEvents == null || rouletteEvents.Length == 0)
        {
            Debug.Log("No roulette events assigned!");
            return;
        }

        Debug.Log("=== ROULETTE EVENTS ===");
        for (int i = 0; i < rouletteEvents.Length; i++)
        {
            RouletteEventData evt = rouletteEvents[i];
            if (evt != null)
            {
                Debug.Log($"{i}: {evt.eventTitle} - {evt.eventDescription}");
            }
            else
            {
                Debug.Log($"{i}: NULL EVENT");
            }
        }
    }
    #endregion
}