using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Visual Feedback")]
    public Color assignedCharacterColor = Color.gray; // Color for already assigned characters
    public Color availableCharacterColor = Color.white; // Color for available characters

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
        {
            confirmButton.onClick.AddListener(ConfirmAssignments);
            UpdateConfirmButtonState();
        }
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

        // Find the first available character for this player
        CharacterData initialCharacter = GetFirstAvailableCharacter();

        PlayerAssignment assignment = new PlayerAssignment
        {
            playerIndex = playerIndex,
            playerName = $"Player {playerIndex + 1}",
            assignedCharacter = initialCharacter,
            assignmentRowObject = assignmentRow
        };

        playerAssignments.Add(assignment);
        SetupAssignmentRowUI(assignmentRow, assignment);
    }

    CharacterData GetFirstAvailableCharacter()
    {
        // Get list of already assigned characters
        List<CharacterData> assignedCharacters = playerAssignments
            .Where(p => p.assignedCharacter != null)
            .Select(p => p.assignedCharacter)
            .ToList();

        // Find first character not in assigned list
        foreach (CharacterData character in availableCharacters)
        {
            if (!assignedCharacters.Contains(character))
            {
                return character;
            }
        }

        // If all characters are assigned, return null (this should be handled)
        return null;
    }

    List<CharacterData> GetAvailableCharacters(PlayerAssignment excludeAssignment = null)
    {
        // Get list of already assigned characters (excluding the current player's assignment)
        List<CharacterData> assignedCharacters = playerAssignments
            .Where(p => p.assignedCharacter != null && p != excludeAssignment)
            .Select(p => p.assignedCharacter)
            .ToList();

        // Return characters not in assigned list
        return availableCharacters.Where(c => !assignedCharacters.Contains(c)).ToList();
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
        if (assignment.assignedCharacter == null)
        {
            // No character assigned - show placeholder
            if (avatar != null)
                avatar.color = Color.red; // Visual indicator for missing assignment

            if (nameText != null)
                nameText.text = "No Character Available!";

            return;
        }

        if (avatar != null && assignment.assignedCharacter.characterAvatar != null)
        {
            avatar.sprite = assignment.assignedCharacter.characterAvatar;
            avatar.color = availableCharacterColor; // Reset to normal color
        }

        if (nameText != null)
            nameText.text = assignment.assignedCharacter.characterName;
    }

    void CycleCharacter(PlayerAssignment assignment, Image avatar, TextMeshProUGUI nameText)
    {
        // Get available characters (excluding current player's assignment)
        List<CharacterData> availableChars = GetAvailableCharacters(assignment);

        if (availableChars.Count == 0)
        {
            Debug.LogWarning($"No available characters for {assignment.playerName}");
            return;
        }

        // Find current character index in available list
        int currentIndex = -1;
        if (assignment.assignedCharacter != null)
        {
            currentIndex = availableChars.IndexOf(assignment.assignedCharacter);
        }

        // Move to next available character
        int nextIndex = (currentIndex + 1) % availableChars.Count;

        // Assign new character
        assignment.assignedCharacter = availableChars[nextIndex];

        // Update display
        UpdateCharacterDisplay(assignment, avatar, nameText);

        // Update confirm button state
        UpdateConfirmButtonState();

        Debug.Log($"{assignment.playerName} assigned to {assignment.assignedCharacter.characterName}");
    }

    void UpdateConfirmButtonState()
    {
        if (confirmButton == null) return;

        // Check if all players have unique character assignments
        bool allAssigned = true;
        HashSet<CharacterData> usedCharacters = new HashSet<CharacterData>();

        foreach (PlayerAssignment assignment in playerAssignments)
        {
            if (assignment.assignedCharacter == null)
            {
                allAssigned = false;
                break;
            }

            if (usedCharacters.Contains(assignment.assignedCharacter))
            {
                allAssigned = false; // Duplicate found
                break;
            }

            usedCharacters.Add(assignment.assignedCharacter);
        }

        confirmButton.interactable = allAssigned;

        
    }

    public void ConfirmAssignments()
    {
        // Validate assignments before saving
        if (!ValidateAssignments())
        {
            Debug.LogError("Cannot confirm assignments - validation failed!");
            return;
        }

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

    bool ValidateAssignments()
    {
        HashSet<CharacterData> usedCharacters = new HashSet<CharacterData>();

        foreach (PlayerAssignment assignment in playerAssignments)
        {
            if (assignment.assignedCharacter == null)
            {
                Debug.LogError($"Player {assignment.playerName} has no character assigned!");
                return false;
            }

            if (usedCharacters.Contains(assignment.assignedCharacter))
            {
                Debug.LogError($"Character {assignment.assignedCharacter.characterName} is assigned to multiple players!");
                return false;
            }

            usedCharacters.Add(assignment.assignedCharacter);
        }

        // Check if we have enough characters for all players
        if (availableCharacters.Length < numberOfPlayers)
        {
            Debug.LogError($"Not enough characters ({availableCharacters.Length}) for all players ({numberOfPlayers})!");
            return false;
        }

        return true;
    }

    // Public method to check if a character is already assigned
    public bool IsCharacterAssigned(CharacterData character, PlayerAssignment excludePlayer = null)
    {
        return playerAssignments.Any(p => p.assignedCharacter == character && p != excludePlayer);
    }

    // Public method to get all assigned characters
    public List<CharacterData> GetAssignedCharacters()
    {
        return playerAssignments
            .Where(p => p.assignedCharacter != null)
            .Select(p => p.assignedCharacter)
            .ToList();
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