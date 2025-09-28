using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Player Info")]
    public int playerIndex;
    public string playerName;

    [Header("Character Info")]
    public string characterName;
    public CharacterData characterData;
    public Sprite characterAvatar;

    [Header("Game Stats")]
    public int coins;
    public int cards;
    public PlayerState state;

    [Header("UI Reference")]
    public GameObject rowGameObject;
}

public enum PlayerState
{
    Active,
    Eliminated,
    Protected
}