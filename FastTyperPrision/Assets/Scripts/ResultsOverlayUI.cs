using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ResultsOverlayUI : MonoBehaviour
{
    public enum OverlayMode
    {
        Phase1Results, 
        Phase2Results  
    }

    [Header("Mode")]
    [SerializeField] private OverlayMode mode = OverlayMode.Phase1Results;

    [Header("UI References")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button phase2Button;

    [Header("Countdown (solo Phase1Results)")]
    [SerializeField] private CountdownTransition countdown;

    [Header("Scene Loading (Build Index)")]
    [SerializeField] private int homeBuildIndex = 0;
    [SerializeField] private int phase2BuildIndex = 2;

    private bool wired;

    private void Awake()
    {
        if (overlayRoot == null) overlayRoot = gameObject;
        overlayRoot.SetActive(false);

        WireButtons();
    }

    public void Show(string msg)
    {
        if (overlayRoot == null)
        {
            Debug.LogError("ResultsOverlayUI: overlayRoot no asignado.");
            return;
        }

        overlayRoot.SetActive(true);

        if (messageText != null)
            messageText.text = msg;

        bool showRetry = (mode == OverlayMode.Phase1Results);
        bool showPhase2 = (mode == OverlayMode.Phase1Results);

        SetButtonVisible(retryButton, showRetry);
        SetButtonVisible(homeButton, true);
        SetButtonVisible(phase2Button, showPhase2);

        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (overlayRoot != null) overlayRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void WireButtons()
    {
        if (wired) return;
        wired = true;

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(homeBuildIndex);
            });
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() =>
            {
                if (mode != OverlayMode.Phase1Results) return;

                Time.timeScale = 1f;
                int current = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(current);
            });
        }

        if (phase2Button != null)
        {
            phase2Button.onClick.RemoveAllListeners();
            phase2Button.onClick.AddListener(() =>
            {
                if (mode != OverlayMode.Phase1Results) return;

                if (overlayRoot != null) overlayRoot.SetActive(false);
                Time.timeScale = 1f;

                if (countdown != null) countdown.Play(phase2BuildIndex);
                else SceneManager.LoadScene(phase2BuildIndex);
            });
        }
    }

    private void SetButtonVisible(Button b, bool visible)
    {
        if (b == null) return;
        b.gameObject.SetActive(visible);
        b.interactable = visible;
    }
}
