using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TypingPhase2Manager : MonoBehaviour
{
    [Header("Refs")]
    public TypingParagraphRenderer paragraphRenderer;
    public AudioController audioCtrl;
    public SettingsPanel settingsPanel;
    public ResultsOverlayUI resultsOverlay;
    public TypingUIController ui;

    [Header("Profile")]
    public AdaptiveTypingProfile profile;

    [Header("Config")]
    public float totalSessionTime = 60f;
    public int countdownLockAtSeconds = 10;

    [Header("Text Normalization")]
    public int maxCharacters = 280;


    [TextArea(4, 10)]
    public string[] paragraphs;

    private float remaining;
    private bool ended;
    private bool settingsLocked;

    private string target = "";
    private string typed = "";
    private int cursorIndex = 0;

    private int correctChars = 0;
    private int errorChars = 0;

    private Dictionary<string, int> bigramErrors = new Dictionary<string, int>();

    private void Start()
    {
        if (paragraphRenderer == null) paragraphRenderer = FindObjectOfType<TypingParagraphRenderer>();
        if (audioCtrl == null) audioCtrl = FindObjectOfType<AudioController>();
        if (settingsPanel == null) settingsPanel = FindObjectOfType<SettingsPanel>();
        if (resultsOverlay == null) resultsOverlay = FindObjectOfType<ResultsOverlayUI>();
        if (ui == null) ui = FindObjectOfType<TypingUIController>();
        if (profile == null) profile = FindObjectOfType<AdaptiveTypingProfile>();

        resultsOverlay?.WireButtons();
        StartPhase2();
    }

    public void StartPhase2()
    {
    
        Time.timeScale = 1f;

        ended = false;
        settingsLocked = false;
        remaining = totalSessionTime;

        correctChars = 0;
        errorChars = 0;
        bigramErrors.Clear();

        typed = "";
        cursorIndex = 0;

        target = NormalizeText(PickAdaptiveParagraph());


        if (paragraphRenderer != null)
            paragraphRenderer.Render(target, typed, cursorIndex);

        ui?.SetMessage("");
        ui?.SetScore(0);
        ui?.SetTimer(remaining);

       
        audioCtrl?.SetActive(true);
        audioCtrl?.ResetAll();
        audioCtrl.countdownStartAtSeconds = countdownLockAtSeconds;

        settingsPanel?.SetSettingsButtonEnabled(true);
    }

    private string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (text.Length <= maxCharacters) return text;

        int cut = text.LastIndexOf(' ', maxCharacters);
        if (cut < 0) cut = maxCharacters;

        return text.Substring(0, cut).Trim();
    }


    private void Update()
    {
        
        if (ended) return;

        if (Time.timeScale == 0f) return;
        remaining -= Time.deltaTime;
        if (remaining < 0f) remaining = 0f;

        ui?.SetTimer(remaining);

        if (!settingsLocked && remaining <= countdownLockAtSeconds && remaining > 0f)
        {
            settingsLocked = true;
            settingsPanel?.SetSettingsButtonEnabled(false);
        }

        audioCtrl?.UpdateCountdown(remaining);

        HandleTypingInput();

        if (remaining <= 0f)
            EndPhase2();
    }

    private void HandleTypingInput()
    {
        
        if (Time.timeScale == 0f) return;

        if (!Input.anyKeyDown) return;
        if (cursorIndex >= target.Length) return;

        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char c in input)
        {
            if (cursorIndex >= target.Length) break;

        
            if (c == '\r' || c == '\n') continue;

            char targetChar = target[cursorIndex];
            bool ok = (c == targetChar);

            
            audioCtrl?.PlayKey();

           
            typed += c;

            if (ok)
            {
                correctChars++;
            }
            else
            {
                errorChars++;
                audioCtrl?.PlayError();

                
                profile?.AddCharError(targetChar);

                if (cursorIndex > 0)
                {
                    char prev = target[cursorIndex - 1];
                    string bg = $"{prev}{targetChar}";

                    int prevCount;
                    bigramErrors.TryGetValue(bg, out prevCount);
                    bigramErrors[bg] = prevCount + 1;
                }
            }

            cursorIndex++;
            paragraphRenderer?.Render(target, typed, cursorIndex);
            ui?.SetScore(correctChars);
        }
    }

    private void EndPhase2()
    {
        if (ended) return;
        ended = true;

        audioCtrl?.SetActive(false);
        audioCtrl?.ResetAll();

        settingsPanel?.SetSettingsButtonEnabled(true);

        paragraphRenderer?.Render("", "", 0);

        float minutes = Mathf.Max(totalSessionTime, 0.01f) / 60f;
        float wpm = (typed.Length / 5f) / minutes;

        float accuracy = typed.Length > 0 ? (correctChars / (float)typed.Length) * 100f : 0f;
        float errorRate = 100f - accuracy;

        var topBigrams = bigramErrors
            .OrderByDescending(kv => kv.Value)
            .Take(3)
            .Select(kv => $"{kv.Key} ({kv.Value})")
            .ToArray();

        string bigramLine = topBigrams.Length > 0 ? string.Join(", ", topBigrams) : "—";

        string resumen =
            "FASE 2 terminada\n" +
            $"Caracteres: {typed.Length}\n" +
            $"WPM: {wpm:F1}\n" +
            $"Precisión: {accuracy:F1}%\n" +
            $"Tasa de error: {errorRate:F1}%\n" +
            $"Top bigramas: {bigramLine}";

        if (resultsOverlay != null) resultsOverlay.Show(resumen);
        else ui?.SetMessage(resumen);
    }

    private string PickAdaptiveParagraph()
    {
        if (paragraphs == null || paragraphs.Length == 0)
            return "La lectura es una habilidad cognitiva compleja que requiere atención sostenida.";

        return paragraphs[Random.Range(0, paragraphs.Length)];
    }
}
