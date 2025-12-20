using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdaptiveTypingProfile : MonoBehaviour
{
    private const string KEY_CHAR = "TT_CHAR_ERR_";
    private const string KEY_BIGRAM = "TT_BIGRAM_ERR_";

    public void AddCharError(char c)
    {
        string k = KEY_CHAR + c;
        PlayerPrefs.SetInt(k, PlayerPrefs.GetInt(k, 0) + 1);
    }

    public void AddBigramError(string bigram)
    {
        string k = KEY_BIGRAM + bigram;
        PlayerPrefs.SetInt(k, PlayerPrefs.GetInt(k, 0) + 1);
    }

    public List<(char ch, int count)> GetTopCharErrors(int topN)
    {
        var list = new List<(char, int)>();
        string candidates = "abcdefghijklmnopqrstuvwxyzñáéíóúABCDEFGHIJKLMNOPQRSTUVWXYZÑÁÉÍÓÚ ,.;:-_!?\"()";

        foreach (char c in candidates)
        {
            int v = PlayerPrefs.GetInt(KEY_CHAR + c, 0);
            if (v > 0) list.Add((c, v));
        }

        return list.OrderByDescending(x => x.Item2).Take(topN).ToList();
    }

    public List<(string bg, int count)> GetTopBigramErrors(int topN)
    {
        return new List<(string, int)>();
    }
}
