using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuCanvas; 
    
    [Header("Menü Panelleri")]
    public GameObject mainButtonsContainer; // İcinde "Bot ile Oyna" ve "Online" butonlari olan obje
    public GameObject difficultyPanel;      // Zorluk butonlarini iceren panel

    // Bot ile oyna butonuna basilinca
    public void OpenDifficultyPanel()
    {
        if (difficultyPanel != null) difficultyPanel.SetActive(true);
        
        // YENI EKLENEN: Zorluk paneli acilinca arkadaki ana butonlari gizle!
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(false);
    }

    // YENI EKLENEN: Oyuncu zorluk secmekten vazgecip geri donmek isterse
    public void CloseDifficultyPanel()
    {
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true); // Ana butonlari geri getir
    }

    public void PlayWithBot_Easy()
    {
        BotManager.currentDifficulty = BotDifficulty.Easy; 
        StartBotMatch();
    }

    public void PlayWithBot_Medium()
    {
        BotManager.currentDifficulty = BotDifficulty.Medium; 
        StartBotMatch();
    }

    public void PlayWithBot_Hard()
    {
        BotManager.currentDifficulty = BotDifficulty.Hard; 
        StartBotMatch();
    }

    private void StartBotMatch()
    {
        FindObjectOfType<NetworkManager>().isMultiplayer = false; 
        
        // Oyuna gecildiginde butun ana menuyu kapat
        mainMenuCanvas.SetActive(false); 
        
        FindObjectOfType<TurnManager>().ChangeState(GameState.Setup);
    }

    public void PlayOnline()
    {
        FindObjectOfType<NetworkManager>().isMultiplayer = true; 
        
        // Oyuna gecildiginde butun ana menuyu kapat
        mainMenuCanvas.SetActive(false); 
        
        FindObjectOfType<NetworkManager>().StartOnlineMatch();
    }
}