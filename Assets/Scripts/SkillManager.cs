using UnityEngine;
using TMPro;

public class SkillManager : MonoBehaviour
{
    private const int SkillCount = 6;

    [Header("Yetenek Arayuzu")]
    public TMP_Text skillText;
    public GameObject useSkillButton;

    // TurnManager'in okuyabilmesi icin aktif yetenegi burada tutuyoruz
    public int activeSkillId = -1; 
    
    private int currentSkillId = -1;
    private bool isSkillUsedThisTurn = false;

    // A, B, C, D, E, F, G degerlerini sabit sayilarla (Orn: 10, 15, 20) doldurduk
    private string[] skillDescriptions = new string[]
    {
        "Increases the player's health by 20.",
        "Increases the played card's attack by 10.",
        "Increases the played card's defense by 10.",
        "Decreases the opponent's card attack by 10.",
        "Decreases the opponent's card defense by 10.",
        "Provides a shield (absorbs 15 damage), increases opponent's attack by 5."
    };

    public void AssignRandomSkill()
    {
        currentSkillId = Random.Range(0, SkillCount);
        activeSkillId = -1; // Yeni tur basladiginda onceki turun yetenegini sifirla
        isSkillUsedThisTurn = false;
        
        if (skillText != null)
        {
            skillText.text = $"Incoming Skill:\n{skillDescriptions[currentSkillId]}";
        }
        
        if (useSkillButton != null)
        {
            useSkillButton.SetActive(true);
        }
    }

    public void UseSkill()
    {
        if (!isSkillUsedThisTurn)
        {
            isSkillUsedThisTurn = true;
            activeSkillId = currentSkillId; // Yetenegi aktif olarak isaretle
            
            if (useSkillButton != null)
            {
                useSkillButton.SetActive(false);
            }
            
            Debug.Log($"Yetenek kullanildi! Savas sirasinda uygulanacak ID: {activeSkillId}");
        }
    }
    
    public void DisableSkill()
    {
        if (useSkillButton != null)
        {
            useSkillButton.SetActive(false);
        }
    }
}