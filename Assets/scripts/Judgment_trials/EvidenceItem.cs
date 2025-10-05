using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EvidenceItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Evidence Settings")]
    public int evidenceID;
    public int matchingID;
    public Sprite evidenceSprite;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color matchedColor = Color.green;

    private Image itemImage;
    private bool isSelected = false;
    private bool isMatched = false;
    private Vector3 originalPosition;
    private EvidenceGameManager gameManager;

    void Start()
    {
        itemImage = GetComponent<Image>();
        originalPosition = transform.position;
        gameManager = FindObjectOfType<EvidenceGameManager>();

        // Set the evidence sprite
        if (evidenceSprite != null)
            itemImage.sprite = evidenceSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMatched || gameManager.IsGameOver()) return;

        gameManager.SelectEvidence(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Handle drop logic here if needed
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Optional: Allow dragging items
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        itemImage.color = selected ? selectedColor : normalColor;
    }

    public void SetMatched(bool matched)
    {
        isMatched = matched;
        if (matched)
        {
            itemImage.color = matchedColor;
            // Optional: Add particle effect or animation
        }
    }

    public bool IsMatched() => isMatched;
    public int GetEvidenceID() => evidenceID;
    public int GetMatchingID() => matchingID;
}