using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUIManager : MonoBehaviour
{
    [Header("Front Door UI (Start Screen)")]
    public GameObject frontDoorCanvas; 
    public Slider volumeSlider;
    public AudioSource bgmSource;

    [Header("Login UI (Name Input)")]
    public GameObject loginCanvas;
    public TMP_InputField usernameInput;

    [Header("Main Menu UI (Mode Selection)")]
    public GameObject startMenuCanvas; 
    public GameObject mainButtonsContainer; 
    public GameObject difficultyPanel;     

    void Start()
    {
        if (frontDoorCanvas != null) frontDoorCanvas.SetActive(true);
        if (loginCanvas != null) loginCanvas.SetActive(false);
        if (startMenuCanvas != null) startMenuCanvas.SetActive(false);

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        if (bgmSource != null) bgmSource.volume = savedVolume;
        
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void OnFrontDoorPlayClicked()
    {
        if (frontDoorCanvas != null) frontDoorCanvas.SetActive(false);

        if (PlayerProfileManager.Instance != null && PlayerProfileManager.Instance.HasProfile())
        {
            if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
            if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
        }
        else
        {
            if (loginCanvas != null) loginCanvas.SetActive(true);
        }
    }

    public void OnLoginButtonClicked()
    {
        string enteredName = usernameInput.text.Trim();
        if (!string.IsNullOrEmpty(enteredName) && enteredName.Length >= 3)
        {
            if (PlayerProfileManager.Instance != null)
            {
                PlayerProfileManager.Instance.SaveName(enteredName);
            }
            
            if (loginCanvas != null) loginCanvas.SetActive(false);
            if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
            if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
        }
    }

    public void SetVolume(float vol)
    {
        if (bgmSource != null) bgmSource.volume = vol;
        PlayerPrefs.SetFloat("MusicVolume", vol); 
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}