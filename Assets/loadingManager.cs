using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class loadingManager : MonoBehaviour
{
    public Slider loadingBar;
    public Text loadingText;

    void Start()
    {
        Debug.Log("SimpleStageLoaderTest started");
        StartCoroutine(LoadWithStages());
    }

    IEnumerator LoadWithStages()
    {
        Debug.Log("Starting staged loading coroutine");

        if (loadingText != null) loadingText.text = "Initializing...";
        Debug.Log("Stage 1: Filling to 25%");
        yield return StartCoroutine(FillToProgress(0.25f, 1.0f));
        Debug.Log("Stage 1 complete, pausing...");
        yield return new WaitForSeconds(0.8f);

        
        if (loadingText != null) loadingText.text = "Loading assets...";
        Debug.Log("Stage 2: Filling to 50%");
        yield return StartCoroutine(FillToProgress(0.5f, 1.2f));
        Debug.Log("Stage 2 complete, pausing...");
        yield return new WaitForSeconds(1.0f);

        
        if (loadingText != null) loadingText.text = "Preparing game...";
        Debug.Log("Stage 3: Filling to 80%");
        yield return StartCoroutine(FillToProgress(0.8f, 1.5f));
        Debug.Log("Stage 3 complete, pausing...");
        yield return new WaitForSeconds(0.9f);

       
        if (loadingText != null)
        Debug.Log("Stage 4: Filling to 100%");
        yield return StartCoroutine(FillToProgress(1.0f, 2.0f));
        Debug.Log("All stages complete!");
        yield return new WaitForSeconds(0.5f);

      
        if (loadingText != null) loadingText.text = "Loading Complete!";
        Debug.Log("Loading sequence finished!");

        SceneManager.LoadScene("MainMenuScene");
    }

    IEnumerator FillToProgress(float targetProgress, float duration)
    {
        if (loadingBar == null)
        {
            Debug.Log("ERROR: LoadingBar is null in FillToProgress!");
            yield break;
        }

        float startProgress = loadingBar.value;
        float elapsed = 0f;

        Debug.Log($"Filling from {startProgress} to {targetProgress} over {duration} seconds");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentProgress = Mathf.Lerp(startProgress, targetProgress, elapsed / duration);
            loadingBar.value = currentProgress;

            yield return null;
        }

        loadingBar.value = targetProgress;
        Debug.Log($"Reached target progress: {targetProgress}");
    }
}