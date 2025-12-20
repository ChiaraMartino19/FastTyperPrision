using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TypingUIController : MonoBehaviour
{
    [Header("Textos")]
    public TMP_Text targetWordText;      
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public TMP_Text messageText;       
    public TMP_Text typedFeedbackText;   

    [Header("Input")]
    public TMP_InputField inputField;

    [Header("Botones")]
    public Button retryButton;

    public void SetTargetWord(string word)
    {
        if (targetWordText != null)
            targetWordText.text = word;

        if (typedFeedbackText != null)
            typedFeedbackText.text = $"<color=white>{word}</color>";
    }

    public void SetTimer(float time)
    {
        if (timerText != null)
            timerText.text = "Time: " + time.ToString("0.0");
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Highscore: " + score;
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    public void ClearAndFocusInput()
    {
        if (inputField == null) return;

        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void ShowRetryButton(bool show)
    {
        if (retryButton != null)
            retryButton.gameObject.SetActive(show);
    }

    
    public void UpdateTypedFeedback(string target, string typed)
    {
        if (typedFeedbackText == null)
            return;

        string result = "";
        int typedLength = typed.Length;
        int targetLength = target.Length;

        int currentIndex = Mathf.Clamp(typedLength, 0, Mathf.Max(targetLength - 1, 0));

        for (int i = 0; i < targetLength; i++)
        {
            char targetChar = target[i];

            bool isTyped = i < typedLength;
            bool isCorrect = isTyped && typed[i] == targetChar;

            string colorTag;
            if (!isTyped) colorTag = "white";
            else if (isCorrect) colorTag = "green";
            else colorTag = "red";

            bool underline = (i == currentIndex);

            if (underline) result += "<u>";
            result += $"<color={colorTag}>{targetChar}</color>";
            if (underline) result += "</u>";
        }

        typedFeedbackText.text = result;
    }

 
    public void MarkWholeWordAsError(string word)
    {
        if (typedFeedbackText == null)
            return;

        string result = "";
        for (int i = 0; i < word.Length; i++)
        {
            result += $"<color=red>{word[i]}</color>";
        }

        typedFeedbackText.text = result;
    }
}
