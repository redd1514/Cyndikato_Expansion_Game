using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Background")]
    public Image backgroundImage;

    [Header("Function Buttons (6 Game Functions)")]
    public Button[] functionButtons = new Button[6];
    public string[] functionNames = {
        "Golder Wager",
        "Judgement Challenge",
        "Roulette Spin",
        "Score Dashboard",
        "Council of Fates",
        "Hidden Agenda"
    };

    [Header("Settings")]
    public Button settingsButton;

    [Header("Scene Names (Optional - for future)")]
    public string[] functionSceneNames = {
        "DieRolling",
        "JudgementChallengeScene",
        "RouletteIntroScene",
        "PlayerSetupScene",
        "economy",
        "HiddenAgenda"
    };

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
      
        for (int i = 0; i < functionButtons.Length; i++)  
        {
            if (functionButtons[i] != null)
            {
                int buttonIndex = i; 
                functionButtons[i].onClick.AddListener(() => OnFunctionButtonClicked(buttonIndex));
            }
        }

       
        if (settingsButton != null)  
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);  
        }
    }

    public void OnFunctionButtonClicked(int functionIndex)
    {
        Debug.Log($"Function {functionIndex + 1} clicked: {functionNames[functionIndex]}");

        // For now, just show which function was clicked
        // Later you can load specific scenes:
        SceneManager.LoadScene(functionSceneNames[functionIndex]);
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked!");

        // For now, just log. Later you can:
        // SceneManager.LoadScene("SettingsScene");
        // Or show a settings panel
    }
}