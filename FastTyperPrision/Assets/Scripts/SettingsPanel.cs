using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("Referencias")]
    public AudioController audioCtrl;
    public TypingUIController uiCtrl;

    [Header("Managers (auto-detect)")]
    public TypingGameManager phase1Manager;       
    public TypingPhase2Manager phase2Manager;    

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

    private bool syncing = false;

    
    private float prevTimeScale = 1f;

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
        if (audioCtrl == null) audioCtrl = FindObjectOfType<AudioController>();
        if (uiCtrl == null) uiCtrl = FindObjectOfType<TypingUIController>();

       
        if (phase1Manager == null) phase1Manager = FindObjectOfType<TypingGameManager>();
        if (phase2Manager == null) phase2Manager = FindObjectOfType<TypingPhase2Manager>();

        
        if (muteAllToggle != null) muteAllToggle.onValueChanged.AddListener(OnMuteAllChanged);
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);

        if (keySlider != null) keySlider.onValueChanged.AddListener(_ => OnPerSoundChanged());
        if (correctSlider != null) correctSlider.onValueChanged.AddListener(_ => OnPerSoundChanged());
        if (errorSlider != null) errorSlider.onValueChanged.AddListener(_ => OnPerSoundChanged());
        if (countdownSlider != null) countdownSlider.onValueChanged.AddListener(_ => OnPerSoundChanged());
    }

    public void OpenPanel()
    {
        if (panelRoot == null) return;
        if (panelRoot.activeSelf) return;

        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        audioCtrl?.StopCountdownOnly();

        if (uiCtrl != null && uiCtrl.inputField != null)
            uiCtrl.inputField.interactable = false;

        panelRoot.SetActive(true);
        SyncUIFromSavedSettings();
    }

    public void ClosePanel()
    {
        if (panelRoot == null) return;
        if (!panelRoot.activeSelf) return;

        panelRoot.SetActive(false);

        Time.timeScale = prevTimeScale;

        if (uiCtrl != null && uiCtrl.inputField != null)
        {
            uiCtrl.inputField.interactable = true;
            uiCtrl.inputField.ActivateInputField();
            uiCtrl.inputField.Select();
        }
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
        if (openButton != null)
            openButton.interactable = enabled;
    }
}
