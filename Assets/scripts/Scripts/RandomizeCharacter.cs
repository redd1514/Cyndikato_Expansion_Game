using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CharacterRandomizer : MonoBehaviour
{
    [Header("Character Settings")]
    public Sprite[] characterSprites;
    public Image displayImage;
    public float randomizeDuration = 4f;
    public float startInterval = 0.05f;
    public float endInterval = 0.3f;
    public TMP_Text xYouAreTheJester;
    private Sprite chosenCharacter;

    [Header("Agenda Settings")]
    public string[] agendas;
    public TMP_Text displayAgenda;
    private string chosenAgenda;

    [Header("UI Elements")]
    public Image bg;
    public GameObject Title;
    public TMP_Text clickToPlay;
    public GameObject settingsPanel;
    public Button SettingsButton;

    [Header("Title Animation")]
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.1f;
    private Vector3 originalScale;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip backgroundMusic;
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    private AudioSource bgMusicSource;

    [Header("Settings Panel Animation")]
    public float slideDuration = 0.4f;
    private RectTransform settingsRect;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool settingsVisible = false;
    private CanvasGroup settingsGroup;
    private Coroutine slideCoroutine;

    // Rotation
    private RectTransform settingsButtonRect;
    public float rotationSpeed = 40f;

    // State management
    private bool hasStarted = false;
    private bool isInitialized = false;
    private bool isPaused = false;
    private Coroutine characterRoutine;
    private Coroutine agendaRoutine;

    void Start()
    {
        displayImage.enabled = false;
        originalScale = Title.transform.localScale;

        // Click sound setup
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.Stop();
        }

        // 🎵 Start background music
        if (backgroundMusic != null)
        {
            bgMusicSource = gameObject.AddComponent<AudioSource>();
            bgMusicSource.clip = backgroundMusic;
            bgMusicSource.loop = true;
            bgMusicSource.volume = 0.4f;
            bgMusicSource.Play();
        }

        // Settings button setup
        if (SettingsButton != null)
        {
            SettingsButton.onClick.AddListener(ToggleSettingsPanel);
            settingsButtonRect = SettingsButton.GetComponent<RectTransform>();
        }

        // Settings panel setup
        if (settingsPanel != null)
        {
            settingsRect = settingsPanel.GetComponent<RectTransform>();
            settingsGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsGroup == null)
                settingsGroup = settingsPanel.AddComponent<CanvasGroup>();

            shownPos = settingsRect.anchoredPosition;
            hiddenPos = shownPos + new Vector2(0, 600);
            settingsRect.anchoredPosition = hiddenPos;
            settingsGroup.alpha = 0f;
            settingsGroup.blocksRaycasts = false;
            settingsPanel.SetActive(true);
        }

        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        yield return null;
        isInitialized = true;
    }

    void Update()
    {
        // Animate title and text before start
        if (!hasStarted)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            Title.transform.localScale = originalScale * scale;

            Color textColor = clickToPlay.color;
            float alpha = Mathf.PingPong(Time.time, 1f);
            clickToPlay.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

            if (isInitialized && (Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
            {
                StartRandomizer();
            }
        }

        // Rotate settings button slowly if visible
        if (settingsVisible && settingsButtonRect != null)
        {
            settingsButtonRect.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    // Start roulette
    public void StartRandomizer()
    {
        displayImage.enabled = true;
        hasStarted = true;
        characterRoutine = StartCoroutine(RandomizeCharacters());
        bg.color = new Color(0.37f, 0.37f, 0.37f);
        Title.SetActive(false);
        clickToPlay.gameObject.SetActive(false);
    }

    private IEnumerator RandomizeCharacters()
    {
        float timer = 0f;

        while (timer < randomizeDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            int randomIndex = Random.Range(0, characterSprites.Length);
            displayImage.sprite = characterSprites[randomIndex];

            if (clickSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
                audioSource.PlayOneShot(clickSound);
            }

            float t = timer / randomizeDuration;
            float currentInterval = Mathf.Lerp(startInterval, endInterval, t * t);
            timer += currentInterval;
            yield return new WaitForSeconds(currentInterval);
        }

        int finalIndex = Random.Range(0, characterSprites.Length);
        chosenCharacter = characterSprites[finalIndex];
        displayImage.sprite = chosenCharacter;

        string trimmedName = chosenCharacter.name.Replace("_0", "");
        xYouAreTheJester.text = $"{trimmedName}, you are the Jester!";

        agendaRoutine = StartCoroutine(RandomizeAgenda());
    }

    private IEnumerator RandomizeAgenda()
    {
        float timer = 0f;

        while (timer < randomizeDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            int randomIndex = Random.Range(0, agendas.Length);
            displayAgenda.text = agendas[randomIndex];

            if (clickSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
                audioSource.PlayOneShot(clickSound);
            }

            float t = timer / randomizeDuration;
            float currentInterval = Mathf.Lerp(startInterval, endInterval, t * t);
            timer += currentInterval;
            yield return new WaitForSeconds(currentInterval);
        }

        int finalIndex = Random.Range(0, agendas.Length);
        chosenAgenda = agendas[finalIndex];
        displayAgenda.text = chosenAgenda;
    }

    // ---------------------------------------
    // SETTINGS PANEL TOGGLE + ANIMATION
    // ---------------------------------------
    public void ToggleSettingsPanel()
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        // Pause or resume roulette
        isPaused = !settingsVisible;

        // Pause/resume background music
        if (bgMusicSource != null)
        {
            if (!settingsVisible)
                bgMusicSource.Pause();
            else
                bgMusicSource.UnPause();
        }

        // Animate panel
        if (settingsVisible)
            slideCoroutine = StartCoroutine(SlidePanel(shownPos, hiddenPos, false));
        else
            slideCoroutine = StartCoroutine(SlidePanel(hiddenPos, shownPos, true));

        settingsVisible = !settingsVisible;
    }

    IEnumerator SlidePanel(Vector2 from, Vector2 to, bool showing)
    {
        float elapsed = 0f;

        settingsGroup.blocksRaycasts = true;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);

            settingsRect.anchoredPosition = Vector2.Lerp(from, to, t);
            settingsGroup.alpha = Mathf.Lerp(showing ? 0f : 1f, showing ? 1f : 0f, t);

            yield return null;
        }

        settingsRect.anchoredPosition = to;
        settingsGroup.alpha = showing ? 1f : 0f;

        if (!showing)
            settingsGroup.blocksRaycasts = false;
    }
}
