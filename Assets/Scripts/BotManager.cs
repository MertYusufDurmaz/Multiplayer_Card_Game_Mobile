using UnityEngine;
using System.Collections.Generic;
using System.Collections; 
using DG.Tweening;

public enum BotDifficulty { Easy, Medium, Hard }

public class BotManager : MonoBehaviour
{
    [Header("Difficulty Level")]
    public static BotDifficulty currentDifficulty = BotDifficulty.Medium; 
    public int criticalHpThreshold = 30; 

    [Header("Card Pool & Deck")]
    public List<CardData> allAvailableCards; 
    private List<CardData> botDeck = new List<CardData>();
    
    [Header("Visuals")]
    public GameObject cardPrefab; 
    public Transform botPlayArea; 

    void Start()
    {
        CreateBotDeck();
    }

    void CreateBotDeck()
    {
        List<CardData> tempPool = new List<CardData>(allAvailableCards);
        for (int i = 0; i < 6; i++)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            botDeck.Add(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex);
        }
        Debug.Log("Bot prepared its 6-card deck.");
    }

    public void PlayRandomCard()
    {
        StartCoroutine(BotPlayRoutine());
    }

    public IEnumerator BotPlayRoutine()
    {
        Debug.Log($"Bot is thinking... (Difficulty: {currentDifficulty})");
        yield return new WaitForSeconds(1.5f); 

        if (botDeck.Count > 0)
        {
            int selectedIndex = 0;
            
            float blunderChance = 0f;
            if (currentDifficulty == BotDifficulty.Easy) blunderChance = 0.50f; 
            else if (currentDifficulty == BotDifficulty.Medium) blunderChance = 0.20f; 

            if (Random.value < blunderChance)
            {
                Debug.Log("Bot blundered! Picking a random card.");
                selectedIndex = Random.Range(0, botDeck.Count);
            }
            else
            {
                Debug.Log("Bot is making a logical decision...");
                selectedIndex = GetBestCardIndex();
            }

            CardData playedCardData = botDeck[selectedIndex];
            botDeck.RemoveAt(selectedIndex);

            if (CombatLogManager.Instance != null)
            {
                CombatLogManager.Instance.AddLog($"Bot played: {playedCardData.cardName} (ATK: {playedCardData.attack} | DEF: {playedCardData.defense})", new Color(1f, 0.4f, 0.4f));
            }

            if (OpponentHandManager.Instance != null)
            {
                GameObject dummyCard = OpponentHandManager.Instance.TakeOneCardFromHand();
                
                if (dummyCard != null)
                {
                    dummyCard.transform.DOScale(dummyCard.transform.localScale * 1.2f, 0.3f).SetLoops(2, LoopType.Yoyo);
                    dummyCard.transform.DOMove(botPlayArea.position, 0.6f).SetEase(Ease.OutQuad);
                    
                    dummyCard.transform.DORotate(new Vector3(90f, 0f, 0f), 0.6f)
                        .OnComplete(() =>
                        {
                            Destroy(dummyCard); 
                            RevealBotCard(playedCardData); 
                        });
                }
                else
                {
                    RevealBotCard(playedCardData);
                }
            }
            else
            {
                RevealBotCard(playedCardData);
            }
        }
    }

    private void RevealBotCard(CardData playedCardData)
    {
        GameObject botCardObj = ObjectPooler.Instance.GetCardFromPool(botPlayArea.position, Quaternion.Euler(90f, 0f, 0f));
        
        botCardObj.name = "BotPlayedCard";
        
        Destroy(botCardObj.GetComponent<CardInteraction>());
        
        CardDisplay display = botCardObj.GetComponent<CardDisplay>();
        if(display != null)
        {
            display.cardData = playedCardData;
            display.nameText.text = playedCardData.cardName;
            display.atkText.text = "ATK: " + playedCardData.attack;
            display.defText.text = "DEF: " + playedCardData.defense;
        }
        
        Debug.Log($"🤖 Bot played '{playedCardData.cardName}'!");

        botCardObj.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 10, 1);

        FindObjectOfType<TurnManager>().currentOpponentCard = botCardObj;
        FindObjectOfType<TurnManager>().ChangeState(GameState.Simulation);
    }

    private int GetBestCardIndex()
    {
        BattleManager battleManager = FindObjectOfType<BattleManager>();
        int bestIndex = 0;
        int bestScore = -1;

        for (int i = 0; i < botDeck.Count; i++)
        {
            CardData card = botDeck[i];
            int currentCardScore = 0;

            if (battleManager.playerHP <= criticalHpThreshold)
                currentCardScore = card.attack; 
            else if (battleManager.opponentHP <= criticalHpThreshold)
                currentCardScore = card.defense; 
            else
                currentCardScore = card.attack + card.defense;

            if (currentCardScore > bestScore)
            {
                bestScore = currentCardScore;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}