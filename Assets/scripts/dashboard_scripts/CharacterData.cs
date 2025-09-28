using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public Sprite characterAvatar;
    public Color characterColor = Color.white;

    [Header("Description")]
    [TextArea(3, 5)]
    public string characterDescription;

    [Header("Role")]
    public CharacterRole role;
}

public enum CharacterRole
{
    Senator,
    Lawyer,
    Police,
    Doctor,
    Cindy,
    Accomplice,
    Senator2
}