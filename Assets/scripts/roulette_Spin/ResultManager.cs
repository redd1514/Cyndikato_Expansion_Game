using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class RouletteResultManager : MonoBehaviour
{
    [Header("Popup References")]
    public GameObject resultPopup;
    public TextMeshProUGUI eventTitle;
    public TextMeshProUGUI eventName;
    public TextMeshProUGUI eventDescription;
    public Button continueButton;

    [Header("Animation Settings")]
    public float popupAnimationDuration = 0.5f;

    void Start()
    {
        LoadResultData();
        StartCoroutine(ShowResultPopup());
    }

    void LoadResultData()
    {
        string selectedTitle = PlayerPrefs.GetString("SelectedEventTitle", "DEFAULT EVENT");
        string selectedDescription = PlayerPrefs.GetString("SelectedEventDescription", "No description available.");

        if (eventName != null)
            eventName.text = selectedTitle;

        if (eventDescription != null)
            eventDescription.text = selectedDescription;

        // Setup continue button
        if (continueButton != null)
            continueButton.onClick.AddListener(ReturnToMainMenu);
    }

    IEnumerator ShowResultPopup()
    {
        // Initially hide popup
        if (resultPopup != null)
        {
            resultPopup.transform.localScale = Vector3.zero;
            resultPopup.SetActive(true);
        }

        // Animate popup appearing
        float elapsed = 0;
        while (elapsed < popupAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0, 1, elapsed / popupAnimationDuration);

            // Ease out animation
            scale = 1 - Mathf.Pow(1 - scale, 2);

            resultPopup.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        resultPopup.transform.localScale = Vector3.one;
    }

    public void ReturnToMainMenu()
    {
        // Clear saved event data
        PlayerPrefs.DeleteKey("SelectedEventTitle");
        PlayerPrefs.DeleteKey("SelectedEventDescription");

        // Return to main menu
        SceneManager.LoadScene("MenuScene");
    }
}