using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource sfxSource;        
    public AudioSource countdownSource; 

    [Header("Clips")]
    public AudioClip keySound;
    public AudioClip correctSound;
    public AudioClip errorSound;
    public AudioClip countdownTickSound;

    [Header("Countdown")]
    public int countdownStartAtSeconds = 10;

    [Header("Settings (se guardan)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    public bool muteAll = false;

    [Range(0f, 1f)] public float keyVolume = 1f;
    [Range(0f, 1f)] public float correctVolume = 1f;
    [Range(0f, 1f)] public float errorVolume = 1f;
    [Range(0f, 1f)] public float countdownVolume = 1f;

    
    private bool isActive = false;
    private int lastCountdownSecond = -1;

  
    private const string K_MasterVol = "audio_masterVol";
    private const string K_MuteAll = "audio_muteAll";
    private const string K_KeyVol = "audio_keyVol";
    private const string K_CorrectVol = "audio_correctVol";
    private const string K_ErrorVol = "audio_errorVol";
    private const string K_CountVol = "audio_countVol";

    private void Awake()
    {
        
        LoadSettings();

       
        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        if (countdownSource != null)
        {
            countdownSource.playOnAwake = false;
            countdownSource.loop = false;
        }
    }

   

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
            ResetAll();
    }

    
    public void ResetAll()
    {
        lastCountdownSecond = -1;

        if (sfxSource != null)
            sfxSource.Stop();

        if (countdownSource != null)
            countdownSource.Stop();
    }

    public void PlayKey()
    {
        if (!isActive) return;
        if (sfxSource == null || keySound == null) return;

        float v = FinalVol(keyVolume);
        if (v > 0f) sfxSource.PlayOneShot(keySound, v);
    }

    public void PlayCorrect()
    {
        if (!isActive) return;
        if (sfxSource == null || correctSound == null) return;

        float v = FinalVol(correctVolume);
        if (v > 0f) sfxSource.PlayOneShot(correctSound, v);
    }

    public void PlayError()
    {
        if (!isActive) return;
        if (sfxSource == null || errorSound == null) return;

        float v = FinalVol(errorVolume);
        if (v > 0f) sfxSource.PlayOneShot(errorSound, v);
    }

    
    public void UpdateCountdown(float remainingSeconds)
    {
        if (!isActive) return;
        if (countdownSource == null || countdownTickSound == null) return;

        int sec = Mathf.CeilToInt(remainingSeconds);

        if (sec <= countdownStartAtSeconds && sec > 0)
        {
            if (sec != lastCountdownSecond)
            {
                float v = FinalVol(countdownVolume);
                if (v > 0f) countdownSource.PlayOneShot(countdownTickSound, v);
                lastCountdownSecond = sec;
            }
        }
    }

   

    public void SetMuteAll(bool mute)
    {
        muteAll = mute;
        SaveSettings();
    }

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        SaveSettings();
    }

    public void SetKeyVolume(float v)
    {
        keyVolume = Mathf.Clamp01(v);
        SaveSettings();
    }

    public void SetCorrectVolume(float v)
    {
        correctVolume = Mathf.Clamp01(v);
        SaveSettings();
    }

    public void SetErrorVolume(float v)
    {
        errorVolume = Mathf.Clamp01(v);
        SaveSettings();
    }

    public void SetCountdownVolume(float v)
    {
        countdownVolume = Mathf.Clamp01(v);
        SaveSettings();
    }

 
    private float FinalVol(float perSoundVol)
    {
        if (muteAll) return 0f;
        return Mathf.Clamp01(masterVolume * perSoundVol);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(K_MasterVol, masterVolume);
        PlayerPrefs.SetInt(K_MuteAll, muteAll ? 1 : 0);

        PlayerPrefs.SetFloat(K_KeyVol, keyVolume);
        PlayerPrefs.SetFloat(K_CorrectVol, correctVolume);
        PlayerPrefs.SetFloat(K_ErrorVol, errorVolume);
        PlayerPrefs.SetFloat(K_CountVol, countdownVolume);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(K_MasterVol, 1f);
        muteAll = PlayerPrefs.GetInt(K_MuteAll, 0) == 1;

        keyVolume = PlayerPrefs.GetFloat(K_KeyVol, 1f);
        correctVolume = PlayerPrefs.GetFloat(K_CorrectVol, 1f);
        errorVolume = PlayerPrefs.GetFloat(K_ErrorVol, 1f);
        countdownVolume = PlayerPrefs.GetFloat(K_CountVol, 1f);
    }
    public void StopCountdownOnly()
    {
        lastCountdownSecond = -1;
        if (countdownSource != null)
            countdownSource.Stop();
    }

 
    public void StopAllNow()
    {
        ResetAll();
    }
}
