using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DynamicScoreDashboard : MonoBehaviour
{
    [Header("UI References")]
    public Transform playerListContainer;
    public GameObject playerRowPrefab;
    public TextMeshProUGUI justiceTotal;
    public TextMeshProUGUI corruptTotal;

    [Header("Character Data")]
    public CharacterData[] allCharacters;

    [Header("Player Data")]
    public List<PlayerData> players = new List<PlayerData>();

    private int numberOfPlayers;

    void Start()
    {
        LoadPlayerCount();
        LoadPlayerAssignments();
        GeneratePlayerRows();
        UpdateTotals();
    }

    void LoadPlayerCount()
    {
        numberOfPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
        Debug.Log($"Dashboard: Loading for {numberOfPlayers} players");
    }

    void LoadPlayerAssignments()
    {
        Debug.Log("Dashboard: Loading player character assignments...");
        players.Clear();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            string characterName = PlayerPrefs.GetString($"Player{i}_CharacterName", "Judge");
            string playerName = PlayerPrefs.GetString($"Player{i}_PlayerName", $"Player {i + 1}");

            CharacterData assignedCharacter = FindCharacterByName(characterName);
            Sprite characterAvatar = assignedCharacter?.characterAvatar;

            PlayerData newPlayer = new PlayerData
            {
                playerIndex = i,
                playerName = playerName,
                characterName = characterName,
                characterData = assignedCharacter,
                characterAvatar = characterAvatar,
                coins = Random.Range(1, 10),
                cards = Random.Range(1, 8),
                state = GetRandomPlayerState(),
                rowGameObject = null
            };

            players.Add(newPlayer);
            Debug.Log($"Dashboard: Loaded {playerName} as {characterName}");
        }
    }

    CharacterData FindCharacterByName(string characterName)
    {
        if (allCharacters == null)
        {
            Debug.LogError("Dashboard: allCharacters array not assigned!");
            return null;
        }

        foreach (CharacterData character in allCharacters)
        {
            if (character != null && character.characterName == characterName)
            {
                return character;
            }
        }

        Debug.LogWarning($"Dashboard: Character '{characterName}' not found!");
        return null;
    }

    void GeneratePlayerRows()
    {
        // Clear existing rows
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"Dashboard: Generating {players.Count} player rows");

        foreach (PlayerData player in players)
        {
            CreatePlayerRow(player);
        }
    }

    void CreatePlayerRow(PlayerData player)
    {
        GameObject playerRow = Instantiate(playerRowPrefab, playerListContainer);
        player.rowGameObject = playerRow;
        SetupPlayerRowUI(playerRow, player);
    }

    void SetupPlayerRowUI(GameObject row, PlayerData player)
    {
        // Find components
        Image playerAvatar = row.transform.Find("PlayerAvatar")?.GetComponent<Image>();
        TextMeshProUGUI playerNameText = row.transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI coinsText = row.transform.Find("CoinsText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardsText = row.transform.Find("CardsText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI stateText = row.transform.Find("StateText")?.GetComponent<TextMeshProUGUI>();

        // Set player info
        if (playerNameText != null)
        {
            playerNameText.text = $"{player.playerName}\n<size=12><color=#CCCCCC>({player.characterName})</color></size>";
        }

        // Set avatar
        if (playerAvatar != null && player.characterAvatar != null)
        {
            playerAvatar.sprite = player.characterAvatar;
            Debug.Log($"Dashboard: ? Set avatar for {player.playerName}");
        }

        // Set stats
        if (coinsText != null) coinsText.text = player.coins.ToString();
        if (cardsText != null) cardsText.text = player.cards.ToString();
        if (stateText != null)
        {
            stateText.text = player.state.ToString();
            stateText.color = GetStateColor(player.state);
        }
    }

    PlayerState GetRandomPlayerState()
    {
        PlayerState[] states = { PlayerState.Active, PlayerState.Eliminated, PlayerState.Protected };
        return states[Random.Range(0, states.Length)];
    }

    Color GetStateColor(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Active: return Color.green;
            case PlayerState.Eliminated: return Color.red;
            case PlayerState.Protected: return Color.blue;
            default: return Color.white;
        }
    }

    void UpdateTotals()
    {
        int justiceCount = 0;
        int corruptCount = 0;

        foreach (var player in players)
        {
            if (player.state == PlayerState.Active)
                justiceCount += player.coins;
            else if (player.state == PlayerState.Eliminated)
                corruptCount += player.coins;
        }

        if (justiceTotal != null)
            justiceTotal.text = $"Justice Total: {justiceCount}";

        if (corruptTotal != null)
            corruptTotal.text = $"Corrupt Total: {corruptCount}";
    }
}