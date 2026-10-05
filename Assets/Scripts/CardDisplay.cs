using UnityEngine;
using TMPro; 

public class CardDisplay : MonoBehaviour
{
    [Header("Kart Verisi")]
    public CardData cardData;

    [Header("Arayuz Elementleri (Yazılar)")]
    public TMP_Text nameText;
    public TMP_Text atkText;
    public TMP_Text defText;

    [Header("Görsel Elementler")]
    public SpriteRenderer artworkRenderer; // YENİ EKLENDİ: Resmi basacağımız bileşen

    void Start()
    {
        // Eger karta bir veri atanmissa degerleri yazdir
        if (cardData != null)
        {
            nameText.text = cardData.cardName;
            atkText.text = "ATK: " + cardData.attack.ToString();
            defText.text = "DEF: " + cardData.defense.ToString();

            // Karta özel bir resim atanmışsa onu göster
            if (artworkRenderer != null && cardData.cardArtwork != null)
            {
                artworkRenderer.sprite = cardData.cardArtwork;
            }
        }
    }
}