using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CountdownTransition : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text counterText;

    [Header("Config")]
    [SerializeField] private int startFrom = 5;
    [SerializeField] private float goDuration = 0.5f;

    private bool running;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Play(int sceneBuildIndex)
    {
        if (running) return;
        StartCoroutine(CoCountdown(sceneBuildIndex));
    }

    private IEnumerator CoCountdown(int sceneBuildIndex)
    {
        running = true;

        if (panel != null)
        {
            panel.SetActive(true);

            var rt = panel.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            panel.transform.SetAsLastSibling();
        }

        for (int i = startFrom; i > 0; i--)
        {
            if (counterText != null) counterText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        if (counterText != null)
        {
            counterText.text = "GO!";
            yield return PunchScale(goDuration);
        }

        yield return new WaitForEndOfFrame();

        SceneManager.LoadScene(sceneBuildIndex);
    }

    private IEnumerator PunchScale(float duration)
    {
        if (counterText == null) yield break;

        Transform t = counterText.transform;
        Vector3 baseScale = t.localScale;

        float half = duration * 0.5f;
        float time = 0f;

        while (time < half)
        {
            time += Time.unscaledDeltaTime;
            float k = time / half;
            t.localScale = Vector3.Lerp(baseScale, baseScale * 1.25f, k);
            yield return null;
        }

        time = 0f;

        while (time < half)
        {
            time += Time.unscaledDeltaTime;
            float k = time / half;
            t.localScale = Vector3.Lerp(baseScale * 1.25f, baseScale, k);
            yield return null;
        }

        t.localScale = baseScale;
    }
}
