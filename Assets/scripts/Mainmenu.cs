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
        "Function 1",
        "Function 2",
        "Function 3",
        "Score Dashboard",
        "Function 5",
        "Function 6"
    };

    [Header("Settings")]
    public Button settingsButton;

    [Header("Scene Names (Optional - for future)")]
    public string[] functionSceneNames = {
        "Function1Scene",
        "Function2Scene",
        "Function3Scene",
        "PlayerSetupScene",
        "Function5Scene",
        "Function6Scene"
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