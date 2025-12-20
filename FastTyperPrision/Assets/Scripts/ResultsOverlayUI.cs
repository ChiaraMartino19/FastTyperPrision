using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultsOverlayUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject overlayRoot;     
    public TMP_Text messageText;      
    public Button retryButton;         
    public Button homeButton;          
    public Button phase2Button;        

    [Header("Scene Names")]
    public string homeSceneName = "Home";
    public string phase1SceneName = "Phase1";
    public string phase2SceneName = "Phase2";
    
    private void Awake()
    {
        if (overlayRoot == null)
            overlayRoot = gameObject;

        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    public void Show(string msg)
    {
        if (overlayRoot != null) overlayRoot.SetActive(true);

        
        if (retryButton != null) retryButton.gameObject.SetActive(true);
        if (homeButton != null) homeButton.gameObject.SetActive(true);
        if (phase2Button != null) phase2Button.gameObject.SetActive(true);

        if (retryButton != null) retryButton.interactable = true;
        if (homeButton != null) homeButton.interactable = true;
        if (phase2Button != null) phase2Button.interactable = true;

        if (messageText != null) messageText.text = msg;

        Time.timeScale = 0f;
    }


    public void Hide()
    {
        if (overlayRoot != null) overlayRoot.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void WireButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(phase1SceneName);
            });
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(homeSceneName);
            });
        }
        if (phase2Button != null)
        {
            phase2Button.onClick.RemoveAllListeners();
            phase2Button.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(phase2SceneName);
            });
        }



    }


}
