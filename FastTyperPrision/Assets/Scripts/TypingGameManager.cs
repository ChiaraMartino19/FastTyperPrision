using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingGameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TypingUIController ui;

    [Header("Audio (Controlador externo)")]
    [SerializeField] private AudioController audioCtrl;

    [Header("Metrics (externo)")]
    [SerializeField] private TypingMetrics metrics;

    [Header("FASE 1 - Fast Typer (Timer Global)")]
    [SerializeField] private float totalSessionTime = 60f;
    [SerializeField] private float errorRevealDelay = 0.3f;
    [TextArea][SerializeField] private string[] wordBank;

    [Header("Countdown")]
    [SerializeField] private int countdownStartAtSeconds = 10;

    [Header("UI - Settings Panel")]
    [SerializeField] private SettingsPanel settingsPanel;

    [Header("UI - Results Overlay")]
    [SerializeField] private ResultsOverlayUI resultsOverlay;

    private float remainingSessionTime;
    private string currentTarget = "";
    private bool gameEnded;
    private bool transitioning;
    private bool settingsLocked;

    private readonly List<string> shuffled = new();
    private int shuffleIndex;

    private string lastTypedText = "";

    private void Awake()
    {
        ui ??= FindObjectOfType<TypingUIController>();
        audioCtrl ??= FindObjectOfType<AudioController>();
        metrics ??= FindObjectOfType<TypingMetrics>();
        settingsPanel ??= FindObjectOfType<SettingsPanel>();
        resultsOverlay ??= FindObjectOfType<ResultsOverlayUI>();

        if (ui != null && ui.inputField != null)
        {
            ui.inputField.onValueChanged.RemoveAllListeners();
            ui.inputField.onValueChanged.AddListener(OnUserTyping);
        }

        ui?.ShowRetryButton(false);

        if (audioCtrl != null)
            audioCtrl.countdownStartAtSeconds = countdownStartAtSeconds;

        resultsOverlay?.WireButtons();
    }

    private void Start()
    {
        StartPhase();
    }

    private void StartPhase()
    {
        Time.timeScale = 1f;

        EnsureWordBank();

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

        ui?.SetScore(0);
        ui?.SetMessage("");
        ui?.ShowRetryButton(false);
        ui?.SetTimer(remainingSessionTime);

        PrepareNewShuffle();
        LoadNextWord();
    }

    private void EnsureWordBank()
    {
        if (wordBank != null && wordBank.Length > 0) return;

        wordBank = new[]
        {
            "tiempo","mirada","camino","sentir","buscar","formar","cambio","memoria","objeto","idioma",
            "origen","sentido","cuerpo","idea","imagen","punto","enfoque","patrón","símbolo","símbolo"
        };
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

        currentTarget = shuffled[shuffleIndex++];
        lastTypedText = "";

        ui?.SetTargetWord(currentTarget);
        ui?.UpdateTypedFeedback(currentTarget, "");
        ui?.ClearAndFocusInput();

        metrics?.BeginWord(currentTarget);
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
        if (gameEnded || transitioning) return;
        if (string.IsNullOrEmpty(currentTarget)) return;

        if (typedText.Length > lastTypedText.Length)
            audioCtrl?.PlayKey();

        lastTypedText = typedText;

        ui?.UpdateTypedFeedback(currentTarget, typedText);

        if (IsMismatch(currentTarget, typedText))
        {
            StartCoroutine(HandleErrorWithDelay(currentTarget, typedText));
            return;
        }

        if (typedText == currentTarget)
        {
            metrics?.EndWord(currentTarget, typedText, success: true);

            int score = metrics != null ? metrics.GetCompletedWords() : 0;
            ui?.SetScore(score);

            audioCtrl?.PlayCorrect();
            LoadNextWord();
        }
    }

    private bool IsMismatch(string target, string typed)
    {
        if (typed.Length > target.Length) return true;

        int limit = Mathf.Min(typed.Length, target.Length);
        for (int i = 0; i < limit; i++)
        {
            if (typed[i] != target[i]) return true;
        }
        return false;
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

        string resumen = BuildSummary();

        if (resultsOverlay != null)
            resultsOverlay.Show(resumen);
        else
            ui?.SetMessage(resumen);

        resultsOverlay.WireButtons();
        resultsOverlay.Show(resumen);

    }

    private string BuildSummary()
    {
        if (metrics == null)
            return "FASE 1 terminada\n(No se encontró TypingMetrics en la escena)";

        return
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

    private void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
