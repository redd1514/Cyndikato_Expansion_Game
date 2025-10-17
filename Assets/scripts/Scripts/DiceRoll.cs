using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    private Rigidbody rb;

    [Header("UI")]
    public TMP_Text numberText;
    public TMP_Text statusText;
    public GameObject gamblePanel;
    public Button yesButton;
    public Button noButton;

    [Header("Dice Object")]
    public GameObject diceModel;

    [Header("Card Selection")]
    public GameObject cardPanel;
    public Image[] cardImages;
    public Sprite cardBackSprite;
    public Sprite[] cardFrontSprites; // 5 front images

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip voiceLine;

    [Header("Dice Physics")]
    public float settleVelocity = 0.05f;
    public float settleAngularVelocity = 0.05f;
    public float settleDuration = 0.6f;
    public LayerMask groundLayers = ~0;

    private float lastMovingTime;
    private bool hasResult = false;
    private int gameRoll = -1;
    private int playerRoll = -1;

    private enum GameState { Intro, Idle, GameRolling, PlayerRolling, Compare, Reward }
    private GameState state = GameState.Intro;

    private RectTransform cardPanelRect;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastMovingTime = Time.time;

        if (numberText != null) numberText.gameObject.SetActive(false);
        if (diceModel != null) diceModel.SetActive(false);
        if (cardPanel != null) cardPanel.SetActive(false);

        gamblePanel.SetActive(true);
        yesButton.onClick.AddListener(OnGambleYes);
        noButton.onClick.AddListener(OnGambleNo);

        if (statusText != null)
            statusText.text = "";

        // 🎧 Play intro voiceline
        if (audioSource != null && voiceLine != null)
            StartCoroutine(PlayIntroVoiceLine());

        // Setup cards
        foreach (Image img in cardImages)
        {
            img.sprite = cardBackSprite;
            img.transform.localScale = Vector3.one * 3.616f;
            img.raycastTarget = true; // Ensure clickable

            // Add EventTrigger if not present
            EventTrigger trigger = img.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = img.gameObject.AddComponent<EventTrigger>();

            // Clear old triggers
            trigger.triggers.Clear();

            // Create click entry
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((eventData) => { OnCardSelected(img); });
            trigger.triggers.Add(entry);
        }

        cardPanelRect = cardPanel.GetComponent<RectTransform>();
    }

    IEnumerator PlayIntroVoiceLine()
    {
        yield return new WaitForSeconds(2f);
        audioSource.clip = voiceLine;
        audioSource.Play();
    }

    void Update()
    {
        if (state == GameState.PlayerRolling)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                RollDice();
            if (Input.GetMouseButtonDown(0))
                RollDice();
        }

        if (rb.linearVelocity.magnitude > settleVelocity || rb.angularVelocity.magnitude > settleAngularVelocity)
        {
            lastMovingTime = Time.time;
            hasResult = false;
            if (numberText != null) numberText.gameObject.SetActive(false);
            return;
        }

        if (!hasResult && (Time.time - lastMovingTime) > settleDuration && IsGrounded())
        {
            int result = DetermineTopFace();
            hasResult = true;

            if (numberText != null)
            {
                numberText.text = result.ToString();
                numberText.gameObject.SetActive(true);
            }

            HandleRollResult(result);
        }
    }

    private void OnGambleYes()
    {
        gamblePanel.SetActive(false);
        statusText.text = "The game rolls first...";
        if (diceModel != null) diceModel.SetActive(true);

        state = GameState.GameRolling;
        AutoRoll();
    }

    private void OnGambleNo()
    {
        gamblePanel.SetActive(false);
        statusText.text = "You chose not to gamble.";
        if (diceModel != null) diceModel.SetActive(true);

        state = GameState.Idle;
    }

    private void HandleRollResult(int result)
    {
        if (state == GameState.GameRolling)
        {
            gameRoll = result;
            statusText.text = $"Game rolled: {result}. Now it's your turn!";
            state = GameState.PlayerRolling;
        }
        else if (state == GameState.PlayerRolling)
        {
            playerRoll = result;
            state = GameState.Compare;
            CompareResults();
        }
    }

    private void CompareResults()
    {
        if (playerRoll > gameRoll)
        {
            statusText.text = $"You rolled {playerRoll} vs {gameRoll}. You win! Choose a card!";
            StartCoroutine(SlideInCardPanel());
        }
        else
        {
            statusText.text = $"You rolled {playerRoll} vs {gameRoll}. No Buff Acquired!";
            state = GameState.Idle;
        }
    }

    IEnumerator SlideInCardPanel()
    {
        state = GameState.Reward;
        cardPanel.SetActive(true);

        // Start position (off-screen top)
        Vector2 startPos = new Vector2(0, 1000f);
        Vector2 endPos = Vector2.zero;

        float duration = 0.8f;
        float t = 0f;

        cardPanelRect.anchoredPosition = startPos;

        // Reset cards
        foreach (Image img in cardImages)
        {
            img.sprite = cardBackSprite;
            img.transform.localScale = Vector3.one * 3.616f;
            img.raycastTarget = true;
        }

        // Slide down animation
        while (t < duration)
        {
            t += Time.deltaTime;
            cardPanelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / duration);
            yield return null;
        }

        cardPanelRect.anchoredPosition = endPos;
    }

    private void OnCardSelected(Image selectedCard)
    {
        if (state != GameState.Reward) return;

        foreach (Image img in cardImages)
            img.raycastTarget = false;

        StartCoroutine(FlipCard(selectedCard));
    }

    IEnumerator FlipCard(Image card)
    {
        float duration = 0.3f;
        float t = 0f;

        Vector3 startScale = Vector3.one * 3.616f;
        Vector3 midScale = new Vector3(0, 3.616f, 1);

        // Phase 1: Shrink horizontally
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            card.transform.localScale = Vector3.Lerp(startScale, midScale, progress);
            yield return null;
        }

        // Change to front sprite
        int cardIndex = System.Array.IndexOf(cardImages, card);
        if (cardIndex >= 0 && cardIndex < cardFrontSprites.Length)
            card.sprite = cardFrontSprites[cardIndex];

        // Phase 2: Expand and zoom in
        t = 0f;
        Vector3 targetScale = Vector3.one * 5f; // Zoom in bigger for visibility
        Vector3 targetPos = new Vector3(0, 150, 0);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        Vector3 originalPos = cardRect.anchoredPosition;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            card.transform.localScale = Vector3.Lerp(midScale, targetScale, progress);
            cardRect.anchoredPosition = Vector3.Lerp(originalPos, targetPos, progress);
            yield return null;
        }

        card.transform.localScale = targetScale;
        cardRect.anchoredPosition = targetPos;

        statusText.text = "You received a buff card!";
        yield return new WaitForSeconds(2f);

        state = GameState.Idle;
    }

    private void AutoRoll() => RollDice();

    public void RollDice()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = new Vector3(0, 2, 0);
        transform.rotation = Random.rotation;

        Vector3 force = new Vector3(Random.Range(-2f, 2f), 5f, Random.Range(-2f, 2f));
        Vector3 torque = Random.insideUnitSphere * 10f;

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        lastMovingTime = Time.time;
        hasResult = false;

        if (numberText != null) numberText.gameObject.SetActive(false);
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    private int DetermineTopFace()
    {
        Vector3[] axes = new Vector3[]
        {
            transform.up, -transform.up,
            transform.forward, -transform.forward,
            transform.right, -transform.right
        };

        int[] axisToValue = new int[] { 2, 5, 1, 6, 4, 3 };

        float bestDot = -Mathf.Infinity;
        int best = 0;
        for (int i = 0; i < axes.Length; i++)
        {
            float d = Vector3.Dot(axes[i], Vector3.up);
            if (d > bestDot)
            {
                bestDot = d;
                best = i;
            }
        }
        return axisToValue[best];
    }

    private bool IsGrounded()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return false;

        float distance = col.bounds.extents.y + 0.05f;
        int mask = groundLayers.value == 0 ? Physics.DefaultRaycastLayers : groundLayers.value;
        return Physics.Raycast(transform.position, Vector3.down, distance + 0.1f, mask);
    }
}
