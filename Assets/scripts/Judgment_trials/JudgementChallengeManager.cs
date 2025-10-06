using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class JudgementChallengeManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI challengeText;
    public TextMeshProUGUI challengerNameText;
    public Button acceptButton;

    [Header("Animation Settings")]
    public float textAnimationDuration = 2.0f;
    public float buttonDelayTime = 2.0f;
    public float sceneLoadDelay = 1.0f; 

    void Start()
    {
        SetupChallenge();
       
        StartCoroutine(WaitForSceneLoadThenAnimate());
    }

    void SetupChallenge()
    {
        
        if (challengeText != null)
            challengeText.alpha = 0;
        if (challengerNameText != null)
            challengerNameText.alpha = 0;

        acceptButton.onClick.AddListener(AcceptChallenge);
        acceptButton.gameObject.SetActive(false);
    }

    IEnumerator WaitForSceneLoadThenAnimate()
    {
      
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(sceneLoadDelay);

       
        yield return StartCoroutine(AnimateChallenge());
    }

    IEnumerator AnimateChallenge()
    {
        yield return StartCoroutine(AnimateTextAppear(challengeText));
        yield return StartCoroutine(AnimateTextAppear(challengerNameText));

        yield return new WaitForSeconds(1.0f);

        yield return new WaitForSeconds(buttonDelayTime);

        acceptButton.gameObject.SetActive(true);
    }

    IEnumerator AnimateTextAppear(TextMeshProUGUI text)
    {
        if (text == null) yield break;

        text.alpha = 0;
        float elapsed = 0;

        while (elapsed < textAnimationDuration)
        {
            elapsed += Time.deltaTime;
            text.alpha = Mathf.Lerp(0, 1, elapsed / textAnimationDuration);
            yield return null;
        }

        text.alpha = 1;
    }

    public void AcceptChallenge()
    {
        SceneManager.LoadScene("JudgementGameScene");
    }
}