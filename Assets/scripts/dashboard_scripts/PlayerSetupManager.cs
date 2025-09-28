using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerSetupManager : MonoBehaviour
{
    [Header("UI References")]
    public Button decreaseButton;
    public Button increaseButton;
    public Button startGameButton;
    public TextMeshProUGUI playerCountDisplay;

    [Header("Settings")]
    public int minPlayers = 4;
    public int maxPlayers = 7;
    public int currentPlayerCount = 4;

    void Start()
    {
        SetupUI();
        UpdatePlayerCountDisplay();
    }

    void SetupUI()
    {
        // Setup decrease button
        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(DecreasePlayerCount);

        // Setup increase button
        if (increaseButton != null)
            increaseButton.onClick.AddListener(IncreasePlayerCount);

        // Setup start game button
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
    }

    public void DecreasePlayerCount()
    {
        if (currentPlayerCount > minPlayers)
        {
            currentPlayerCount--;
            UpdatePlayerCountDisplay();
            Debug.Log($"Player count decreased to: {currentPlayerCount}");
        }
    }

    public void IncreasePlayerCount()
    {
        if (currentPlayerCount < maxPlayers)
        {
            currentPlayerCount++;
            UpdatePlayerCountDisplay();
            Debug.Log($"Player count increased to: {currentPlayerCount}");
        }
    }

    void UpdatePlayerCountDisplay()
    {
        if (playerCountDisplay != null)
        {
            playerCountDisplay.text = currentPlayerCount.ToString();
        }
    }

    public void StartGame()
    {
        // Save the player count for the next scene
        PlayerPrefs.SetInt("PlayerCount", currentPlayerCount);
        PlayerPrefs.Save();

        string targetScene = "CharacterAssignmentScene";
        Debug.Log($"Attempting to load scene: {targetScene}");

        // Check if scene exists in build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Scene {i}: {sceneName}");
        }

        Debug.Log($"Going to character assignment for {currentPlayerCount} players");

        // Load character assignment scene
        SceneManager.LoadScene("CharacterAssignmentScene");
    }
}