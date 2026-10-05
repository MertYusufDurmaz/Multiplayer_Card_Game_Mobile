using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening; // DOTWEEN KUTUPHANESI EKLENDI!

public class DeckManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private TurnManager turnManager;

    [Header("Kart Verileri (10 Adet)")]
    public List<CardData> allAvailableCards; 
    public List<CardData> selectedCards = new List<CardData>();

    [Header("Arayuz (UI) Baglantilari")]
    public GameObject deckSelectionCanvas;
    public GameObject playButton;
    public UnityEngine.UI.Button[] uiCardButtons; 
    public GameObject waitingForOpponentPanel; // YENİ EKLENDİ: Rakip bekleniyor paneli

    [Header("3D Obje Referansi")]
    public GameObject cardPrefab3D; 

    void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        turnManager = FindObjectOfType<TurnManager>();
    }

    void Start()
    {
        if (playButton != null)
        {
            playButton.SetActive(false);
        }
        
        for (int i = 0; i < uiCardButtons.Length; i++)
        {
            int index = i; 
            var btn = uiCardButtons[index];
            var txt = btn.GetComponentInChildren<TMP_Text>();
            
            if (index < allAvailableCards.Count)
            {
                CardData card = allAvailableCards[index];
                
                // 1. Yazıları ata
                txt.text = $"ATK: {card.attack} | DEF: {card.defense}";
                
                // --- 2. GÖRSELİ BUL VE ATA (YENİ EKLENDİ) ---
                // Butonun çocukları arasında "ArtworkImage" adındaki Image bileşenini buluyoruz
                Transform artworkObj = btn.transform.Find("ArtworkImage");
                
                if (artworkObj != null)
                {
                    Image artworkImage = artworkObj.GetComponent<Image>();
                    if (artworkImage != null && card.cardArtwork != null)
                    {
                        // Karta ait o özel resmi butona basıyoruz
                        artworkImage.sprite = card.cardArtwork;
                    }
                }
                else
                {
                    Debug.LogWarning($"Buton {index} içinde 'ArtworkImage' adında bir obje bulunamadı!");
                }
                // ----------------------------------------------

                btn.onClick.AddListener(() => OnUICardClicked(index, btn));
            }
        }
    }

    void OnUICardClicked(int index, Button clickedButton)
    {
        CardData clickedData = allAvailableCards[index];

        // Butonun içindeki resim ve zincir objelerini buluyoruz
        Transform artworkObj = clickedButton.transform.Find("ArtworkImage");
        Transform chainObj = clickedButton.transform.Find("ChainImage");

        Image artworkImage = artworkObj != null ? artworkObj.GetComponent<Image>() : null;
        GameObject chainGo = chainObj != null ? chainObj.gameObject : null;

        if (selectedCards.Contains(clickedData))
        {
            // --- SEÇİMİ İPTAL ET (KİLİDİ AÇ) ---
            selectedCards.Remove(clickedData);
            
            // Butonun ana rengini ve içindeki resmi eski (aydınlık) haline getir
            clickedButton.image.color = Color.white; 
            if (artworkImage != null) artworkImage.color = Color.white; 
            
            // Zinciri gizle
            if (chainGo != null) chainGo.SetActive(false); 
        }
        else if (selectedCards.Count < 6)
        {
            // --- KARTI SEÇ (ZİNCİRLE) ---
            selectedCards.Add(clickedData);
            
            // Butonun ana rengini ve resmi karart (Koyu gri yapıyoruz)
            Color darkColor = new Color(0.4f, 0.4f, 0.4f);
            clickedButton.image.color = darkColor; 
            if (artworkImage != null) artworkImage.color = darkColor; 
            
            // Zinciri görünür yap
            if (chainGo != null) chainGo.SetActive(true); 
        }

        playButton.SetActive(selectedCards.Count == 6);
    }

    public void OnPlayButtonClicked()
    {
        deckSelectionCanvas.SetActive(false);
        
        if (networkManager != null && networkManager.isMultiplayer)
        {
            if (waitingForOpponentPanel != null) waitingForOpponentPanel.SetActive(true);
            networkManager.SendPlayerReady();
        }
        else
        {
            SpawnCards();
            if (turnManager != null)
            {
                turnManager.ChangeState(GameState.PlayerTurn);
            }
        }
    }

    public void SpawnCards()
    {
        if (waitingForOpponentPanel != null) waitingForOpponentPanel.SetActive(false);

        int count = selectedCards.Count;
        float spreadAngle = 70f; 
        float startAngle = -spreadAngle / 2f;
        float angleStep = count > 1 ? spreadAngle / (count - 1) : 0;
        
        Vector3 handCenterPos = new Vector3(0f, 1f, -5f); 
        Vector3 spawnStartPos = new Vector3(0f, -3f, -5f); 

        float cameraWaitTime = 1.5f; 

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            
            float xPos = currentAngle * 0.12f; 
            float yPos = Mathf.Abs(currentAngle) * -0.01f;
            float zPos = i * -0.01f;

            Vector3 targetPos = handCenterPos + new Vector3(xPos, yPos, zPos);
            Quaternion targetRot = Quaternion.Euler(0f, 0f, -currentAngle);

            GameObject newCard = ObjectPooler.Instance.GetCardFromPool(spawnStartPos, Quaternion.Euler(0f, 0f, 0f));
            
            newCard.transform.DOMove(targetPos, 0.6f).SetDelay(cameraWaitTime + (i * 0.15f)).SetEase(Ease.OutBack);
            newCard.transform.DORotateQuaternion(targetRot, 0.6f).SetDelay(cameraWaitTime + (i * 0.15f)).SetEase(Ease.OutBack);
            
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            
            // Kartı sahneye atarken verilerini yeniden yüklüyoruz ki o kartın resmi masaüstü kartına da geçsin!
            display.cardData = selectedCards[i];
            
            // Eğer kartın üzerindeki SpriteRenderer varsa resmi ona da atıyoruz
            if (display.artworkRenderer != null && selectedCards[i].cardArtwork != null)
            {
                 display.artworkRenderer.sprite = selectedCards[i].cardArtwork;
            }

            display.nameText.text = selectedCards[i].cardName;
            display.atkText.text = $"ATK: {selectedCards[i].attack}";
            display.defText.text = $"DEF: {selectedCards[i].defense}";

            newCard.GetComponent<CardInteraction>().UpdateOriginalPositionAndRotation(targetPos, targetRot);
            newCard.GetComponent<CardInteraction>().isPlayed = false;
        }
        if (OpponentHandManager.Instance != null)
        {
            OpponentHandManager.Instance.SpawnOpponentHand();
        }
    }
}