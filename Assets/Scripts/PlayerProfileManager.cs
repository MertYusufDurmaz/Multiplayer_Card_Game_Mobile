using UnityEngine;

public class PlayerProfileManager : MonoBehaviour
{
    public static PlayerProfileManager Instance;

    public string PlayerName { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadProfile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Profil var mi diye kontrol eder
    public bool HasProfile()
    {
        return PlayerPrefs.HasKey("PlayerName");
    }

    // Ismi cihaza kaydeder
    public void SaveName(string newName)
    {
        PlayerPrefs.SetString("PlayerName", newName);
        PlayerPrefs.Save();
        PlayerName = newName;
    }

    // Ismi cihazdan okur
    private void LoadProfile()
    {
        PlayerName = PlayerPrefs.GetString("PlayerName", "Oyuncu");
    }
}