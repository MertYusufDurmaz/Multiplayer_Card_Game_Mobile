using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "BirchGames/CardData")]
public class CardData : ScriptableObject
{
    [Header("Kart Degerleri")]
    public string cardName = "Yeni Kart";
    public int attack;
    public int defense;

    [Header("Gorsel")]
    public Sprite cardArtwork; // YENİ EKLENDİ: Kartın üzerindeki resim
}