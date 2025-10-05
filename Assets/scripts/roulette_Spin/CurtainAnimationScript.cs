using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class RouletteCurtainManager : MonoBehaviour
{
    [Header("Curtain References")]
    public RectTransform leftCurtain;
    public RectTransform rightCurtain;

    [Header("Animation Settings")]
    public float curtainCloseDuration = 2.0f;
    public float pauseBeforeTransition = 1.0f;

    [Header("Audio")]
    public AudioSource curtainAudioSource;
    public AudioClip curtainCloseSound;

    void Start()
    {
        StartCoroutine(CloseCurtainsSequence());
    }

    IEnumerator CloseCurtainsSequence()
    {
        // Play curtain sound
        if (curtainAudioSource != null && curtainCloseSound != null)
        {
            curtainAudioSource.PlayOneShot(curtainCloseSound);
        }

        // Animate curtains closing
        yield return StartCoroutine(CloseCurtains());

        // Pause for dramatic effect
        yield return new WaitForSeconds(pauseBeforeTransition);

        // Load roulette wheel scene
        SceneManager.LoadScene("RouletteWheelScene");
    }

    IEnumerator CloseCurtains()
    {
        float elapsed = 0;
        Vector2 leftStartPos = leftCurtain.anchoredPosition;
        Vector2 rightStartPos = rightCurtain.anchoredPosition;

        // Target positions (center of screen)
        Vector2 leftTargetPos = new Vector2(0, 0);      // Left curtain moves to center
        Vector2 rightTargetPos = new Vector2(0, 0);     // Right curtain moves to center

        while (elapsed < curtainCloseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / curtainCloseDuration;

            // Use ease-out curve for smooth closing
            float easeProgress = 1 - Mathf.Pow(1 - progress, 2);

            // Animate curtain positions
            leftCurtain.anchoredPosition = Vector2.Lerp(leftStartPos, leftTargetPos, easeProgress);
            rightCurtain.anchoredPosition = Vector2.Lerp(rightStartPos, rightTargetPos, easeProgress);

            yield return null;
        }

        // Ensure final positions
        leftCurtain.anchoredPosition = leftTargetPos;
        rightCurtain.anchoredPosition = rightTargetPos;
    }
}