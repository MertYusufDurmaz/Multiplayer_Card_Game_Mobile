using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class OpponentHandManager : MonoBehaviour
{
    // Her yerden tek satırla ulaşabilmek için Singleton
    public static OpponentHandManager Instance;

    [Header("Görsel Ayarlar")]
    public GameObject cardBackPrefab; // Rakibin sadece arkası görünen kukla kartı
    public Transform opponentHandCenter; // Rakibin kartlarının duracağı orta nokta

    // Rakibin elindeki görsel kartların listesi
    private List<GameObject> opponentVisualCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Oyun başladığında 6 adet kapalı kartı dizer
    public void SpawnOpponentHand(int cardCount = 6)
    {
        float spreadAngle = 70f;
        float startAngle = -spreadAngle / 2f;
        float angleStep = cardCount > 1 ? spreadAngle / (cardCount - 1) : 0;

        for (int i = 0; i < cardCount; i++)
        {
            float currentAngle = startAngle + (i * angleStep);

            // X eksenini ters çeviriyoruz ki rakibin eline göre yayılsın
            float xPos = currentAngle * -0.12f; 
            float yPos = Mathf.Abs(currentAngle) * -0.01f;
            float zPos = i * -0.01f;

            Vector3 targetPos = opponentHandCenter.position + new Vector3(xPos, yPos, zPos);
            
            // ÇÖZÜM NOKTASI: X ekseni 0 (dik durur), Y ekseni 180 (arkasını döner)
            // X eksenini 90 yaparak kartların senin sahnende düz (dik) görünmesini sağlıyoruz
            Quaternion targetRot = Quaternion.Euler(90f, 180f, -currentAngle);

            Vector3 spawnStartPos = opponentHandCenter.position + new Vector3(0, -3f, 5f);
            
            // Doğduğu anda da aynı şekilde X:90 açısıyla oluşturuyoruz
            GameObject cardBack = Instantiate(cardBackPrefab, spawnStartPos, Quaternion.Euler(90f, 180f, 0f));
            
            float cameraWaitTime = 1.5f; 
            cardBack.transform.DOMove(targetPos, 0.6f).SetDelay(cameraWaitTime + (i * 0.15f)).SetEase(Ease.OutBack);
            cardBack.transform.DORotateQuaternion(targetRot, 0.6f).SetDelay(cameraWaitTime + (i * 0.15f)).SetEase(Ease.OutBack);
            
            opponentVisualCards.Add(cardBack);
        }
    }

    // Rakip hamle yaptığında, elindeki kapalı kartlardan RASTGELE birini bize verecek
    public GameObject TakeOneCardFromHand()
    {
        if (opponentVisualCards.Count > 0)
        {
            // Elindeki kart sayısı içinden rastgele bir sayı seç (Örn: 0 ile 5 arası)
            int randomIndex = Random.Range(0, opponentVisualCards.Count);
            
            // Rastgele seçilen o kartı al
            GameObject cardToPlay = opponentVisualCards[randomIndex];
            
            // Seçilen kartı listeden sil ki eli azalsın
            opponentVisualCards.RemoveAt(randomIndex); 
            
            return cardToPlay; 
        }
        return null;
    }
}