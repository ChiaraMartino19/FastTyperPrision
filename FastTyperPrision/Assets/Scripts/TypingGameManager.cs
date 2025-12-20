using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingGameManager : MonoBehaviour
{
    [Header("Referencias")]
    public TypingUIController ui;

    [Header("Audio (Controlador externo)")]
    public AudioController audioCtrl;

    [Header("Metrics (externo)")]
    public TypingMetrics metrics;

    [Header("FASE 1 - Fast Typer (Timer Global)")]
    public float totalSessionTime = 60f;
    public float errorRevealDelay = 0.3f;
    [TextArea] public string[] wordBank;

    [Header("Countdown")]
    public int countdownStartAtSeconds = 10;

    [Header("UI - Settings Panel")]
    public SettingsPanel settingsPanel;

    [Header("UI - Results Overlay")]
    public ResultsOverlayUI resultsOverlay;

   
    private float remainingSessionTime;
    private string currentTarget = "";
    private bool gameEnded = false;
    private bool transitioning = false;
    private bool settingsLocked = false;

   
    private readonly List<string> shuffled = new List<string>();
    private int shuffleIndex = 0;

    
    private string lastTypedText = "";

    
    private bool overlayWired = false;

    private void Awake()
    {
       
        if (ui == null) ui = FindObjectOfType<TypingUIController>();
        if (audioCtrl == null) audioCtrl = FindObjectOfType<AudioController>();
        if (metrics == null) metrics = FindObjectOfType<TypingMetrics>();
        if (settingsPanel == null) settingsPanel = FindObjectOfType<SettingsPanel>();
        if (resultsOverlay == null) resultsOverlay = FindObjectOfType<ResultsOverlayUI>();

       
        if (audioCtrl != null)
            audioCtrl.countdownStartAtSeconds = countdownStartAtSeconds;

        
        if (ui != null && ui.inputField != null)
        {
            ui.inputField.onValueChanged.RemoveAllListeners();
            ui.inputField.onValueChanged.AddListener(OnUserTyping);
        }

       
        ui?.ShowRetryButton(false);
    }

    private void Start()
    {
        
        WireOverlayOnce();

        StartPhase();
    }

    private void WireOverlayOnce()
    {
        if (overlayWired) return;
        if (resultsOverlay != null)
        {
            resultsOverlay.WireButtons();
            overlayWired = true;
        }
    }

    

    private void StartPhase()
    {
        Time.timeScale = 1f;

        if (resultsOverlay != null)
            resultsOverlay.Hide();

        
        if (wordBank == null || wordBank.Length == 0)
        {
            wordBank = new string[]
            {
                "casa","gato","pato","mesa","sol","pan","nube","mano","taza","vaso",
                "rojo","azul","luz","boca","dedo","tren","rio","lana","flor","salto"
            };
        }

        gameEnded = false;
        transitioning = false;

        settingsLocked = false;
        settingsPanel?.SetSettingsButtonEnabled(true);

        remainingSessionTime = totalSessionTime;

        lastTypedText = "";

        
        metrics?.BeginSession(totalSessionTime);

        
        if (audioCtrl != null)
        {
            audioCtrl.countdownStartAtSeconds = countdownStartAtSeconds;
            audioCtrl.SetActive(true);
            audioCtrl.ResetAll();
        }

        PrepareNewShuffle();

        ui?.SetScore(0);
        ui?.SetMessage(""); 
        ui?.ShowRetryButton(false); 

        LoadNextWord();
        ui?.SetTimer(remainingSessionTime);
    }

    private void PrepareNewShuffle()
    {
        shuffled.Clear();
        shuffled.AddRange(wordBank);
        ShuffleList(shuffled);
        shuffleIndex = 0;
    }

    private void LoadNextWord()
    {
        if (shuffleIndex >= shuffled.Count)
            PrepareNewShuffle();

        currentTarget = shuffled[shuffleIndex];
        shuffleIndex++;

        ui?.SetTargetWord(currentTarget);
        ui?.UpdateTypedFeedback(currentTarget, "");
        ui?.ClearAndFocusInput();

       
        metrics?.BeginWord(currentTarget);

        lastTypedText = "";
    }

    private void Update()
    {
        if (gameEnded) return;

        remainingSessionTime -= Time.deltaTime;
        if (remainingSessionTime < 0f) remainingSessionTime = 0f;

        ui?.SetTimer(remainingSessionTime);

        if (!settingsLocked && remainingSessionTime <= countdownStartAtSeconds && remainingSessionTime > 0f)
        {
            settingsLocked = true;
            settingsPanel?.SetSettingsButtonEnabled(false);
        }

      
        audioCtrl?.UpdateCountdown(remainingSessionTime);

        if (remainingSessionTime <= 0f)
            EndPhase();
    }

   

    private void OnUserTyping(string typedText)
    {
        if (gameEnded) return;
        if (transitioning) return;

        
        if (typedText.Length > lastTypedText.Length)
            audioCtrl?.PlayKey();

        lastTypedText = typedText;

        
        ui?.UpdateTypedFeedback(currentTarget, typedText);

        
        int limit = Mathf.Min(typedText.Length, currentTarget.Length);
        for (int i = 0; i < limit; i++)
        {
            if (typedText[i] != currentTarget[i])
            {
                StartCoroutine(HandleErrorWithDelay(currentTarget, typedText));
                return;
            }
        }

        
        if (typedText.Length > currentTarget.Length)
        {
            StartCoroutine(HandleErrorWithDelay(currentTarget, typedText));
            return;
        }

        
        if (typedText == currentTarget)
        {
            metrics?.EndWord(currentTarget, typedText, success: true);

            int score = (metrics != null) ? metrics.GetCompletedWords() : 0;
            ui?.SetScore(score);

            audioCtrl?.PlayCorrect();

            LoadNextWord();
        }
    }

   

    private IEnumerator HandleErrorWithDelay(string target, string typed)
    {
        transitioning = true;

        ui?.MarkWholeWordAsError(target);
        audioCtrl?.PlayError();

        metrics?.EndWord(target, typed, success: false);

       
        yield return new WaitForSecondsRealtime(errorRevealDelay);

        LoadNextWord();
        transitioning = false;
    }

   

    private void EndPhase()
    {
        if (gameEnded) return;
        gameEnded = true;
        transitioning = false;

        
        if (audioCtrl != null)
        {
            audioCtrl.SetActive(false);
            audioCtrl.ResetAll();
        }

       
        settingsPanel?.SetSettingsButtonEnabled(true);

        
        metrics?.EndSessionAndUpdatePersonalBest();

        
        ui?.SetTargetWord("");
        ui?.UpdateTypedFeedback("", "");
        ui?.SetTimer(0f);

       
        string resumen;
        if (metrics != null)
        {
            resumen =
                "FASE 1 terminada\n" +
                $"Palabras correctas: {metrics.GetCompletedWords()}\n" +
                $"WPM: {metrics.GetWPM():F1}\n" +
                $"Precisión: {metrics.GetAccuracyPercent():F1}%\n" +
                $"Tasa de error: {metrics.GetErrorRatePercent():F1}%\n" +
                $"Tiempo reacción prom.: {metrics.GetAvgReactionTimeSeconds():F2}s\n" +
                $"Racha máxima: {metrics.GetMaxStreak()}\n" +
                $"Mejor marca (WPM): {metrics.GetBestWPM():F1}\n" +
                $"Mejor marca (Precisión): {metrics.GetBestAccuracyPercent():F1}%";
        }
        else
        {
            resumen = "FASE 1 terminada\n(No se encontró TypingMetrics en la escena)";
        }

        
        if (resultsOverlay != null)
        {
            WireOverlayOnce(); 
            resultsOverlay.Show(resumen);
        }
        else
        {
            Debug.LogError("ResultsOverlayUI no asignado/encontrado. Mostrando resumen en ui.SetMessage().");
            ui?.SetMessage(resumen);
        }
    }

    

    private void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
