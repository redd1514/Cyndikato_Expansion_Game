using UnityEngine;

[CreateAssetMenu(fileName = "New RouletteEvent", menuName = "Game/RouletteEvent")]
public class RouletteEventData : ScriptableObject
{
    [Header("Event Information")]
    public string eventTitle = "Event Title";

    [TextArea(2, 4)]
    public string eventDescription = "Event description goes here...";

    [Header("Visual Properties")]
    public Color eventColor = Color.white;
    public Sprite eventIcon;

    [Header("Wheel Properties")]
    [Range(0, 360)]
    public float segmentAngle = 45f; // Degrees for this segment on wheel

    [Header("Game Effect")]
    public EventType eventType = EventType.Neutral;
    public bool isPositive = true;
}

public enum EventType
{
    TurnOrder,      // Changes turn order
    ResourceBonus,  // Gives extra coins/cards
    Protection,     // Provides immunity/protection
    Challenge,      // Creates a challenge
    Skip,          // Skip turns
    Double,        // Double effects
    Neutral        // No major impact
}