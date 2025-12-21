using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Referencias")]
    public AudioController audioCtrl;
    public TypingUIController uiCtrl;

    [Header("UI")]
    public GameObject panelRoot;
    public Button openButton;
    public Button closeButton;

    public Toggle muteAllToggle;
    public Slider masterSlider;
    public Slider keySlider;
    public Slider correctSlider;
    public Slider errorSlider;
    public Slider countdownSlider;

    private bool syncing;
    private float prevTimeScale = 1f;

  
    private ISettingsPausable pausable;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

      
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void Start()
    {
        audioCtrl ??= FindObjectOfType<AudioController>();
        uiCtrl ??= FindObjectOfType<TypingUIController>();

        pausable = FindFirstActivePausable();

        muteAllToggle?.onValueChanged.AddListener(OnMuteAllChanged);
        masterSlider?.onValueChanged.AddListener(OnMasterChanged);

        keySlider?.onValueChanged.AddListener(_ => OnPerSoundChanged());
        correctSlider?.onValueChanged.AddListener(_ => OnPerSoundChanged());
        errorSlider?.onValueChanged.AddListener(_ => OnPerSoundChanged());
        countdownSlider?.onValueChanged.AddListener(_ => OnPerSoundChanged());
    }

    public void OpenPanel()
    {
        if (panelRoot == null || panelRoot.activeSelf) return;

        pausable ??= FindFirstActivePausable();

        PauseGame();
        BlockInput(true);

        panelRoot.SetActive(true);
        SyncUIFromSavedSettings();
    }

    public void ClosePanel()
    {
        if (panelRoot == null || !panelRoot.activeSelf) return;

        panelRoot.SetActive(false);

        ResumeGame();
        BlockInput(false);
    }

    private void PauseGame()
    {
        audioCtrl?.StopCountdownOnly();

        if (pausable != null)
        {
            pausable.SetPausedBySettings(true);
            return;
        }

        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        if (pausable != null)
        {
            pausable.SetPausedBySettings(false);
            return;
        }

        Time.timeScale = prevTimeScale;
    }

    private void BlockInput(bool block)
    {
        if (uiCtrl?.inputField == null) return;

        uiCtrl.inputField.interactable = !block;

        if (!block)
        {
            uiCtrl.inputField.ActivateInputField();
            uiCtrl.inputField.Select();
        }
    }

    private ISettingsPausable FindFirstActivePausable()
    {

        var behaviours = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            if (b is ISettingsPausable p && b.isActiveAndEnabled)
                return p;
        }
        return null;
    }


    private void SyncUIFromSavedSettings()
    {
        if (audioCtrl == null) return;

        syncing = true;
        audioCtrl.LoadSettings();

        if (muteAllToggle != null) muteAllToggle.isOn = audioCtrl.muteAll;
        if (masterSlider != null) masterSlider.value = audioCtrl.masterVolume;

        if (keySlider != null) keySlider.value = audioCtrl.keyVolume;
        if (correctSlider != null) correctSlider.value = audioCtrl.correctVolume;
        if (errorSlider != null) errorSlider.value = audioCtrl.errorVolume;
        if (countdownSlider != null) countdownSlider.value = audioCtrl.countdownVolume;

        syncing = false;
    }

    private void OnMuteAllChanged(bool isMuted)
    {
        if (syncing || audioCtrl == null) return;
        audioCtrl.SetMuteAll(isMuted);
        audioCtrl.ResetAll();
    }

    private void OnMasterChanged(float v)
    {
        if (syncing || audioCtrl == null) return;
        audioCtrl.SetMasterVolume(v);
    }

    private void OnPerSoundChanged()
    {
        if (syncing || audioCtrl == null) return;

        if (keySlider != null) audioCtrl.SetKeyVolume(keySlider.value);
        if (correctSlider != null) audioCtrl.SetCorrectVolume(correctSlider.value);
        if (errorSlider != null) audioCtrl.SetErrorVolume(errorSlider.value);
        if (countdownSlider != null) audioCtrl.SetCountdownVolume(countdownSlider.value);
    }

    public void SetSettingsButtonEnabled(bool enabled)
    {
        if (openButton != null) openButton.interactable = enabled;
    }
}
