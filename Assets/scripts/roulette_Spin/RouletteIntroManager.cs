using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class RouletteIntroManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dramaticText;

    [Header("Text Animation Settings")]
    public float fadeInDuration = 1.5f;
    public float displayDuration = 2.0f;
    public float fadeOutDuration = 1.0f;

    [Header("Text Sequences")]
    public string[] textSequences = {
        "THE CHAMBER FALLS SILENT...",
        "A DECREE AWAITS...",
        "THE WHEEL OF FATE TURNS..."
    };

    void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // Initially hide text
        if (dramaticText != null)
            dramaticText.color = new Color(dramaticText.color.r, dramaticText.color.g, dramaticText.color.b, 0);

        // Play each text sequence
        foreach (string text in textSequences)
        {
            yield return StartCoroutine(AnimateText(text));
        }

        // Wait a moment then transition
        yield return new WaitForSeconds(0.5f);

        // Load curtain scene
        SceneManager.LoadScene("RouletteCurtainScene");
    }

    IEnumerator AnimateText(string text)
    {
        if (dramaticText == null) yield break;

        // Set text
        dramaticText.text = text;

        // Fade in
        yield return StartCoroutine(FadeText(0, 1, fadeInDuration));

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeText(1, 0, fadeOutDuration));
    }

    IEnumerator FadeText(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0;
        Color originalColor = dramaticText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            dramaticText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        dramaticText.color = new Color(originalColor.r, originalColor.g, originalColor.b, toAlpha);
    }
}