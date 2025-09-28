using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomButtonSetUp : MonoBehaviour
{
    [Header("Button Components")]
    public Button startSessionButton;
    public Image buttonImage;
    public Text buttonText;
        
    [Header("Scene Settings")]
    public string mainMenuSceneName = "MenuScene";  

    [Header("Button States")]
    public Sprite normalButtonSprite;
    public Sprite pressedButtonSprite;    
    public Color normalColor = Color.white;
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    void Start()
    {
        SetupButton();
    }

    void SetupButton()
    {
        if (startSessionButton != null)
        {
           
            startSessionButton.onClick.AddListener(OnStartSessionClicked);

            
            var colors = startSessionButton.colors;
            colors.normalColor = normalColor;
            colors.pressedColor = pressedColor;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            startSessionButton.colors = colors;
        }
    }

    public void OnStartSessionClicked()
    {
        Debug.Log("START SESSION clicked!");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}