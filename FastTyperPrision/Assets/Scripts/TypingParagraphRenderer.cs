using System.Text;
using TMPro;
using UnityEngine;

public class TypingParagraphRenderer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text paragraphText;

    [Header("Colors")]
    public string pendingColor = "white";
    public string correctColor = "green";
    public string errorColor = "red";

    public string cursorMarkColor = "#FFFFFF33"; 

    public void Render(string target, string typed, int cursorIndex)
    {
        if (paragraphText == null) return;

        var sb = new StringBuilder(target.Length * 10);

        for (int i = 0; i < target.Length; i++)
        {
            char targetChar = target[i];

            bool hasTyped = i < typed.Length;
            bool isCorrect = hasTyped && typed[i] == targetChar;

            string color = !hasTyped ? pendingColor : (isCorrect ? correctColor : errorColor);

      
            bool isCursor = (i == cursorIndex);

            if (isCursor)
                sb.Append($"<mark={cursorMarkColor}>");

            sb.Append($"<color={color}>");
            sb.Append(targetChar);
            sb.Append("</color>");

            if (isCursor)
                sb.Append("</mark>");
        }

        paragraphText.text = sb.ToString();
    }
}
