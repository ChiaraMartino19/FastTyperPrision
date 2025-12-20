using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeStartCountdown : MonoBehaviour
{
    [Header("UI")]
    public GameObject countdownPanel;  
    public TMP_Text countdownText;       

    [Header("Config")]
    public int startFrom = 5;
    public string phase1SceneName = "Phase1";

    private bool isCounting = false;

    private void Awake()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (isCounting) return;
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        isCounting = true;

        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        for (int i = startFrom; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

    
        if (countdownText != null)
        {
            countdownText.text = "go!";
            yield return new WaitForSeconds(0.5f);
        }

        SceneManager.LoadScene(phase1SceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
