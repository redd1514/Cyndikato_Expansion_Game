using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CharacterAssignmentManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform playerAssignmentContainer;
    public GameObject playerAssignmentPrefab;
    public Button confirmButton;
    public TextMeshProUGUI titleText;

    [Header("Character Data")]
    public CharacterData[] availableCharacters;

    [Header("Assignment Data")]
    public List<PlayerAssignment> playerAssignments = new List<PlayerAssignment>();

    private int numberOfPlayers;

    void Start()
    {
        LoadPlayerCount();
        CreatePlayerAssignments();
    }

    void LoadPlayerCount()
    {
        numberOfPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
        if (titleText != null)
            titleText.text = $"ASSIGN CHARACTERS TO {numberOfPlayers} PLAYERS";

        Debug.Log($"Setting up character assignment for {numberOfPlayers} players");
    }

    void CreatePlayerAssignments()
    {
        // Clear existing assignments
        foreach (Transform child in playerAssignmentContainer)
        {
            Destroy(child.gameObject);
        }

        playerAssignments.Clear();

        // Create assignment UI for each player
        for (int i = 0; i < numberOfPlayers; i++)
        {
            CreatePlayerAssignmentRow(i);
        }

        // Setup confirm button
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmAssignments);
    }

    void CreatePlayerAssignmentRow(int playerIndex)
    {
        GameObject assignmentRow = Instantiate(playerAssignmentPrefab, playerAssignmentContainer);

        // Make sure we have characters available
        if (availableCharacters.Length == 0)
        {
            Debug.LogError("No characters assigned! Please assign characters in the inspector.");
            return;
        }

        PlayerAssignment assignment = new PlayerAssignment
        {
            playerIndex = playerIndex,
            playerName = $"Player {playerIndex + 1}",
            assignedCharacter = availableCharacters[playerIndex % availableCharacters.Length], // Cycle through characters
            assignmentRowObject = assignmentRow
        };

        playerAssignments.Add(assignment);
        SetupAssignmentRowUI(assignmentRow, assignment);
    }

    void SetupAssignmentRowUI(GameObject row, PlayerAssignment assignment)
    {
        // More robust way to find components
        TextMeshProUGUI playerNameText = row.GetComponentInChildren<TextMeshProUGUI>();

        // Find all images in the row
        Image[] allImages = row.GetComponentsInChildren<Image>();
        Image characterAvatar = null;

        // Find the character avatar specifically
        foreach (Image img in allImages)
        {
            if (img.gameObject.name == "CharacterAvatar")
            {
                characterAvatar = img;
                break;
            }
        }

        // Find character name text (look for the one that's not the player name)
        TextMeshProUGUI[] allTexts = row.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI characterNameText = null;

        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.gameObject.name == "CharacterNameText")
            {
                characterNameText = text;
                break;
            }
        }

        // Find the button
        Button characterButton = row.GetComponentInChildren<Button>();

        // Debug what we found
        Debug.Log($"Found avatar: {characterAvatar != null}");
        Debug.Log($"Found character name text: {characterNameText != null}");
        Debug.Log($"Found button: {characterButton != null}");

        // Set player name
        if (playerNameText != null)
        {
            playerNameText.text = assignment.playerName;
            Debug.Log($"Set player name to: {assignment.playerName}");
        }

        // Set initial character display
        UpdateCharacterDisplay(assignment, characterAvatar, characterNameText);

        // Setup character selection button
        if (characterButton != null)
        {
            characterButton.onClick.RemoveAllListeners();
            characterButton.onClick.AddListener(() => {
                Debug.Log($"Character button clicked for {assignment.playerName}");
                CycleCharacter(assignment, characterAvatar, characterNameText);
            });
        }
    }

    void UpdateCharacterDisplay(PlayerAssignment assignment, Image avatar, TextMeshProUGUI nameText)
    {
        if (assignment.assignedCharacter == null) return;

        if (avatar != null && assignment.assignedCharacter.characterAvatar != null)
            avatar.sprite = assignment.assignedCharacter.characterAvatar;

        if (nameText != null)
            nameText.text = assignment.assignedCharacter.characterName;
    }

    void CycleCharacter(PlayerAssignment assignment, Image avatar, TextMeshProUGUI nameText)
    {
        // Find current character index
        int currentIndex = System.Array.IndexOf(availableCharacters, assignment.assignedCharacter);

        // Move to next character (cycle back to 0 if at end)
        int nextIndex = (currentIndex + 1) % availableCharacters.Length;

        // Assign new character
        assignment.assignedCharacter = availableCharacters[nextIndex];

        // Update display
        UpdateCharacterDisplay(assignment, avatar, nameText);

        Debug.Log($"{assignment.playerName} assigned to {assignment.assignedCharacter.characterName}");
    }

    public void ConfirmAssignments()
    {
        // Save character assignments to PlayerPrefs
        for (int i = 0; i < playerAssignments.Count; i++)
        {
            PlayerAssignment assignment = playerAssignments[i];
            PlayerPrefs.SetString($"Player{i}_CharacterName", assignment.assignedCharacter.characterName);
            PlayerPrefs.SetString($"Player{i}_PlayerName", assignment.playerName);

            // Also save the avatar sprite name for loading later
            if (assignment.assignedCharacter.characterAvatar != null)
            {
                PlayerPrefs.SetString($"Player{i}_AvatarName", assignment.assignedCharacter.characterAvatar.name);
            }
        }

        PlayerPrefs.Save();

        Debug.Log("Character assignments confirmed and saved!");

        // Print assignments for verification
        for (int i = 0; i < playerAssignments.Count; i++)
        {
            Debug.Log($"Player {i + 1}: {playerAssignments[i].assignedCharacter.characterName}");
        }

        // Load score dashboard
        SceneManager.LoadScene("Function4Scene");
    }
}

[System.Serializable]
public class PlayerAssignment
{
    public int playerIndex;
    public string playerName;
    public CharacterData assignedCharacter;
    public GameObject assignmentRowObject;
}