using UnityEngine;

public class TypingMetrics : MonoBehaviour
{
    
    private const string KEY_BEST_WPM = "BEST_WPM";
    private const string KEY_BEST_ACC = "BEST_ACC";

    
    private float sessionStartTime;
    private float sessionDurationSec; 

  
    private float wordStartTime;

   
    private int totalWordsAttempted;
    private int completedWords;       
    private int totalTargetLetters;
    private int correctLetters;

    
    private int errorWords;           

   
    private float sumReactionTime;
    private int reactionSamples;

  
    private int currentStreak;
    private int maxStreak;

  
    private float bestWpm;
    private float bestAccuracy;

    public void BeginSession(float durationSeconds)
    {
        sessionStartTime = Time.time;
        sessionDurationSec = Mathf.Max(durationSeconds, 0.01f);

        totalWordsAttempted = 0;
        completedWords = 0;
        totalTargetLetters = 0;
        correctLetters = 0;
        errorWords = 0;

        sumReactionTime = 0f;
        reactionSamples = 0;

        currentStreak = 0;
        maxStreak = 0;

        LoadPersonalBests();
    }

    public void BeginWord(string target)
    {
        wordStartTime = Time.time;
       
    }

    
    public void EndWord(string target, string typed, bool success)
    {
        totalWordsAttempted++;

       
        float rt = Mathf.Max(Time.time - wordStartTime, 0f);
        sumReactionTime += rt;
        reactionSamples++;

        
        int targetLen = target.Length;
        totalTargetLetters += targetLen;

        int correct = 0;
        int limit = Mathf.Min(targetLen, typed.Length);
        for (int i = 0; i < limit; i++)
        {
            if (typed[i] == target[i]) correct++;
        }
        correctLetters += correct;

       
        if (success)
        {
            completedWords++;
            currentStreak++;
            if (currentStreak > maxStreak) maxStreak = currentStreak;
        }
        else
        {
            errorWords++;
            currentStreak = 0;
        }
    }

    public void EndSessionAndUpdatePersonalBest()
    {
        
        float wpm = GetWPM();
        float acc = GetAccuracyPercent();

        
        bool changed = false;
        if (wpm > bestWpm)
        {
            bestWpm = wpm;
            PlayerPrefs.SetFloat(KEY_BEST_WPM, bestWpm);
            changed = true;
        }

        if (acc > bestAccuracy)
        {
            bestAccuracy = acc;
            PlayerPrefs.SetFloat(KEY_BEST_ACC, bestAccuracy);
            changed = true;
        }

        if (changed)
            PlayerPrefs.Save();
    }

    private void LoadPersonalBests()
    {
        bestWpm = PlayerPrefs.GetFloat(KEY_BEST_WPM, 0f);
        bestAccuracy = PlayerPrefs.GetFloat(KEY_BEST_ACC, 0f);
    }

 

    public int GetCompletedWords() => completedWords;
    public int GetTotalAttempts() => totalWordsAttempted;

    public float GetWPM()
    {
        return (completedWords / sessionDurationSec) * 60f;
    }

    public float GetAccuracyPercent()
    {
        return (totalTargetLetters > 0)
            ? (correctLetters / (float)totalTargetLetters) * 100f
            : 0f;
    }

    public float GetErrorRatePercent()
    {
        return (totalWordsAttempted > 0)
            ? (errorWords / (float)totalWordsAttempted) * 100f
            : 0f;
    }

    public float GetAvgReactionTimeSeconds()
    {
        return (reactionSamples > 0)
            ? (sumReactionTime / reactionSamples)
            : 0f;
    }

    public int GetMaxStreak() => maxStreak;

    public float GetBestWPM() => bestWpm;
    public float GetBestAccuracyPercent() => bestAccuracy;
}
