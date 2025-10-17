using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterCoins : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText; 
    public Button plusButton;        
    public Button minusButton;     

    [Header("Settings")]
    public int startingCoins = 4;

    private int currentCoins;

    private void Start()
    {
 
        currentCoins = startingCoins;
        UpdateCoinText();

        plusButton.onClick.AddListener(AddCoin);
        minusButton.onClick.AddListener(RemoveCoin);
    }

    private void AddCoin()
    {
        currentCoins++;
        UpdateCoinText();
    }

    private void RemoveCoin()
    {
        currentCoins--;
        UpdateCoinText();
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = currentCoins.ToString();
    }
}
