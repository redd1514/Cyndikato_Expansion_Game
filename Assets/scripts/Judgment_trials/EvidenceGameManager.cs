using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EvidenceGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button proveButton;
    public Button failButton;
    public Transform evidenceContainer;

    [Header("Game Settings")]
    public float gameTime = 30f;
    public GameObject evidenceItemPrefab;
    public Sprite[] evidenceSprites; // Assign in inspector
    public Vector2 spawnAreaMin = new Vector2(-400, -200);
    public Vector2 spawnAreaMax = new Vector2(400, 200);

    [Header("Evidence Pairs")]
    public EvidencePair[] evidencePairs;

    private List<EvidenceItem> evidenceItems = new List<EvidenceItem>();
    private EvidenceItem selectedEvidence = null;
    private float currentTime;
    private bool gameActive = true;
    private int totalPairs;
    private int matchedPairs = 0;

    [System.Serializable]
    public class EvidencePair
    {
        public int id1;
        public int id2;
        public Sprite sprite1;
        public Sprite sprite2;
    }

    void Start()
    {
        InitializeGame();
        SetupButtons();
    }

    void InitializeGame()
    {
        currentTime = gameTime;
        gameActive = true;
        matchedPairs = 0;
        resultPanel.SetActive(false);

        SpawnEvidenceItems();
        totalPairs = evidencePairs.Length;

        StartCoroutine(GameTimer());
    }

    void SpawnEvidenceItems()
    {
        List<Vector2> usedPositions = new List<Vector2>();

        foreach (EvidencePair pair in evidencePairs)
        {
            // Spawn first item of pair
            SpawnEvidenceItem(pair.id1, pair.id2, pair.sprite1, usedPositions);
            // Spawn second item of pair
            SpawnEvidenceItem(pair.id2, pair.id1, pair.sprite2, usedPositions);
        }
    }

    void SpawnEvidenceItem(int id, int matchingId, Sprite sprite, List<Vector2> usedPositions)
    {
        Vector2 spawnPosition;
        int attempts = 0;

        do
        {
            spawnPosition = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
        }
        while (IsPositionTooClose(spawnPosition, usedPositions) && attempts < 50);

        usedPositions.Add(spawnPosition);

        GameObject evidenceObj = Instantiate(evidenceItemPrefab, evidenceContainer);
        evidenceObj.transform.localPosition = spawnPosition;

        EvidenceItem evidenceItem = evidenceObj.GetComponent<EvidenceItem>();
        evidenceItem.evidenceID = id;
        evidenceItem.matchingID = matchingId;
        evidenceItem.evidenceSprite = sprite;

        evidenceItems.Add(evidenceItem);
    }

    bool IsPositionTooClose(Vector2 newPos, List<Vector2> existingPositions)
    {
        float minDistance = 300f; // Minimum distance between items

        foreach (Vector2 pos in existingPositions)
        {
            if (Vector2.Distance(newPos, pos) < minDistance)
                return true;
        }
        return false;
    }

    public void SelectEvidence(EvidenceItem evidence)
    {
        if (!gameActive || evidence.IsMatched()) return;

        if (selectedEvidence == null)
        {
            // First selection
            selectedEvidence = evidence;
            evidence.SetSelected(true);
        }
        else if (selectedEvidence == evidence)
        {
            // Deselect same item
            selectedEvidence.SetSelected(false);
            selectedEvidence = null;
        }
        else
        {
            // Second selection - check for match
            CheckMatch(selectedEvidence, evidence);
        }
    }

    void CheckMatch(EvidenceItem item1, EvidenceItem item2)
    {
        if (item1.GetMatchingID() == item2.GetEvidenceID())
        {
            
            item1.SetMatched(true);
            item2.SetMatched(true);
            item1.SetSelected(false);

            matchedPairs++;

            
            if (matchedPairs >= totalPairs)
            {
                WinGame();
            }
        }
        else
        {
            
            item1.SetSelected(false);
        }

        selectedEvidence = null;
    }

    IEnumerator GameTimer()
    {
        while (currentTime > 0 && gameActive)
        {
            timerText.text = Mathf.Ceil(currentTime).ToString("00") + ":" +
                           ((currentTime % 1) * 100).ToString("00");

            currentTime -= Time.deltaTime;
            yield return null;
        }

        if (gameActive)
        {
            LoseGame();
        }
    }

    void WinGame()
    {
        gameActive = false;
        resultText.text = "YOU WIN!";
        resultText.color = Color.yellow;
        resultPanel.SetActive(true);

        proveButton.gameObject.SetActive(true);
        failButton.gameObject.SetActive(false);
    }

    void LoseGame()
    {
        gameActive = false;
        resultText.text = "TIME'S UP!";
        resultText.color = Color.red;
        resultPanel.SetActive(true);

        proveButton.gameObject.SetActive(false);
        failButton.gameObject.SetActive(true);
    }

    void SetupButtons()
    {
        proveButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MenuScene"); 
        });

        failButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MenuScene"); 
        });
    }

    public bool IsGameOver() => !gameActive;
}