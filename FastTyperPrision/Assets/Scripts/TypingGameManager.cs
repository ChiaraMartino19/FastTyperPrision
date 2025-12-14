using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingGameManager : MonoBehaviour
{
    [Header("Referencias")]
    public TypingUIController ui;

    [Header("FASE 1 - Fast Typer (Timer Global)")]
    public float totalSessionTime = 60f;     // tiempo total de la fase (recomendado 60s)
    public float errorRevealDelay = 0.1f;    // delay para ver el rojo (0.05 - 0.15)
    [TextArea] public string[] wordBank;     // banco de palabras

    // Estado
    private float remainingSessionTime;
    private string currentTarget = "";
    private bool gameEnded = false;

    // Random sin repetir
    private List<string> shuffled = new List<string>();
    private int shuffleIndex = 0;

    // Métricas
    private float sessionStartTime;
    private int totalWordsAttempted = 0;
    private int completedWords = 0;
    private int totalTargetLetters = 0;
    private int correctLetters = 0;
    private string lastTypedText = "";

    // Para evitar que se lancen varias coroutines de error al mismo tiempo
    private bool transitioning = false;

    private void Start()
    {
        if (ui == null) ui = FindObjectOfType<TypingUIController>();

        if (ui.retryButton != null)
        {
            ui.retryButton.onClick.RemoveAllListeners();
            ui.retryButton.onClick.AddListener(StartPhase);
        }

        if (ui.inputField != null)
        {
            ui.inputField.onValueChanged.RemoveAllListeners();
            ui.inputField.onValueChanged.AddListener(OnUserTyping);
        }

        StartPhase();
    }

    // ---------------- FASE ----------------

    private void StartPhase()
    {
        // Banco por defecto si está vacío
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

        // Reset métricas
        sessionStartTime = Time.time;
        remainingSessionTime = totalSessionTime;
        totalWordsAttempted = 0;
        completedWords = 0;
        totalTargetLetters = 0;
        correctLetters = 0;
        lastTypedText = "";

        PrepareNewShuffle();

        ui.ShowRetryButton(false);
        ui.SetScore(0);
        ui.SetMessage(""); // limpiar mensaje durante el juego (dejamos solo el resumen final)

        LoadNextWord();
        ui.SetTimer(remainingSessionTime);
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
        {
            PrepareNewShuffle();
        }

        currentTarget = shuffled[shuffleIndex];
        shuffleIndex++;

        ui.SetTargetWord(currentTarget);
        ui.UpdateTypedFeedback(currentTarget, "");
        ui.ClearAndFocusInput();

        lastTypedText = "";
    }

    private void Update()
    {
        if (gameEnded) return;

        remainingSessionTime -= Time.deltaTime;
        if (remainingSessionTime < 0f) remainingSessionTime = 0f;

        ui.SetTimer(remainingSessionTime);

        if (remainingSessionTime <= 0f)
        {
            EndPhase();
        }
    }

    private void OnUserTyping(string typedText)
    {
        if (gameEnded) return;
        if (transitioning) return; // mientras está mostrando rojo y cambiando

        lastTypedText = typedText;

        // Feedback visual normal
        ui.UpdateTypedFeedback(currentTarget, typedText);

        // 1) Error inmediato por letra distinta
        int limit = Mathf.Min(typedText.Length, currentTarget.Length);
        for (int i = 0; i < limit; i++)
        {
            if (typedText[i] != currentTarget[i])
            {
                StartCoroutine(HandleErrorWithDelay(currentTarget, typedText));
                return;
            }
        }

        // 2) Error inmediato si escribió de más
        if (typedText.Length > currentTarget.Length)
        {
            StartCoroutine(HandleErrorWithDelay(currentTarget, typedText));
            return;
        }

        // 3) Éxito si coincide exacto
        if (typedText == currentTarget)
        {
            RegisterWordResult(currentTarget, typedText);
            ui.SetScore(completedWords);
            LoadNextWord();
        }
    }

    private IEnumerator HandleErrorWithDelay(string target, string typed)
    {
        transitioning = true;

        // Pintar palabra completa roja
        ui.MarkWholeWordAsError(target);

        // Registrar métricas
        RegisterWordResult(target, typed);

        // Esperar para que el rojo se vea
        yield return new WaitForSeconds(errorRevealDelay);

        // Pasar a siguiente
        LoadNextWord();

        transitioning = false;
    }

    private void RegisterWordResult(string target, string typed)
    {
        totalWordsAttempted++;
        totalTargetLetters += target.Length;

        int correct = 0;
        int limit = Mathf.Min(target.Length, typed.Length);
        for (int i = 0; i < limit; i++)
        {
            if (typed[i] == target[i])
                correct++;
        }

        correctLetters += correct;

        if (typed == target)
            completedWords++;
    }

    private void EndPhase()
    {
        gameEnded = true;
        transitioning = false;

        float sessionTime = Mathf.Max(totalSessionTime, 0.01f);
        float wpm = (completedWords / sessionTime) * 60f;
        float accuracy = (totalTargetLetters > 0)
            ? (correctLetters / (float)totalTargetLetters) * 100f
            : 0f;

        ui.SetTargetWord("");
        ui.UpdateTypedFeedback("", "");
        ui.SetTimer(0f);

        // ✅ Resumen final (esto sí lo querías)
        ui.SetMessage(
            "FASE 1 terminada\n" +
            $"Palabras correctas: {completedWords}\n" +
            $"WPM: {wpm:F1}\n" +
            $"Precisión: {accuracy:F1}%"
        );

        ui.ShowRetryButton(true);
    }

    // ---------------- UTIL ----------------

    private void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
