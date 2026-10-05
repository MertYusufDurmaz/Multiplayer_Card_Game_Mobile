using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public enum GameState { Setup, PlayerTurn, WaitOpponent, Simulation, EndTurn, GameOver }

public class TurnManager : MonoBehaviour
{
    private BattleManager battleManager;
    private DeckManager deckManager;
    private NetworkManager networkManager;
    private BotManager botManager;
    private SkillManager skillManager;
    private AtmosphereManager atmosphereManager;

    [Header("Online Opponent Settings")]
    public Transform opponentPlayArea;    

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverMessageText;

    [Header("Game Values")]
    public GameState currentState;
    public int currentTurn = 1;
    public float turnTimer = 30f; 
    public bool hasPlayedCardThisTurn = false;

    [Header("Played Card References")]
    public CardInteraction currentPlayedCard = null; 
    public GameObject currentOpponentCard = null;
    public int currentOpponentSkillId = -1; // SYNC: Store opponent's skill

    [Header("UI Elements")]
    public GameObject gameplayCanvas; 
    public TMP_Text timerText;
    public GameObject endTurnButton; 
    public TMP_Text turnIndicatorText; 

    [Header("Board Status")]
    public bool isMyCardOnBoard = false;
    public bool isOpponentCardOnBoard = false;

    [Header("Cinemachine Cameras")]
    public GameObject vcamWide;
    public GameObject vcamHand;

    [Header("Tutorial")]
    public TutorialArrow tutorialArrow;

    public void RegisterPlayedCard(CardInteraction card)
    {
        currentPlayedCard = card;
        hasPlayedCardThisTurn = true;
    }

    public void UnregisterPlayedCard()
    {
        currentPlayedCard = null;
        hasPlayedCardThisTurn = false;
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
        deckManager = FindAnyObjectByType<DeckManager>();
        networkManager = FindAnyObjectByType<NetworkManager>();
        botManager = FindAnyObjectByType<BotManager>();
        skillManager = GetComponent<SkillManager>();
        atmosphereManager = AtmosphereManager.Instance;
    }

    public void StartGame()
    {
        ChangeState(GameState.PlayerTurn);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        if (endTurnButton != null)
        {
            endTurnButton.SetActive(currentState == GameState.PlayerTurn);
        }

        switch (currentState)
        {
            case GameState.Setup:
                if(vcamWide != null && vcamHand != null) { vcamWide.SetActive(true); vcamHand.SetActive(false); }
                if (gameplayCanvas != null) gameplayCanvas.SetActive(false);

                if (deckManager != null && deckManager.deckSelectionCanvas != null)
                {
                    deckManager.deckSelectionCanvas.SetActive(true);
                }
                break;
                
            case GameState.PlayerTurn:
                turnTimer = 30f;

                if (turnIndicatorText != null)
                {
                    turnIndicatorText.gameObject.SetActive(true);
                    turnIndicatorText.text = "YOUR TURN!";
                    turnIndicatorText.color = Color.green;
                }

                if (currentTurn == 1 && !hasPlayedCardThisTurn && tutorialArrow != null)
                {
                    tutorialArrow.Show();
                }
                
                if(vcamWide != null && vcamHand != null) { vcamWide.SetActive(false); vcamHand.SetActive(true); }
                if (gameplayCanvas != null) gameplayCanvas.SetActive(true);
                
                if (battleManager != null) battleManager.UpdateHPUI();

                if (skillManager != null) skillManager.AssignRandomSkill();
                if (atmosphereManager != null) atmosphereManager.FocusOnPlayer();
                break;
                
            case GameState.WaitOpponent:
                if (turnIndicatorText != null)
                {
                    turnIndicatorText.gameObject.SetActive(true);
                    turnIndicatorText.text = "OPPONENT'S TURN...";
                    turnIndicatorText.color = Color.yellow;
                }
                if(vcamWide != null && vcamHand != null) { vcamWide.SetActive(false); vcamHand.SetActive(true); }
                if (gameplayCanvas != null) gameplayCanvas.SetActive(true);
                
                if (battleManager != null) battleManager.UpdateHPUI();

                if (networkManager != null && !networkManager.isMultiplayer)
                {
                    if (botManager != null)
                        StartCoroutine(botManager.BotPlayRoutine());
                }
                if (atmosphereManager != null) atmosphereManager.FocusOnOpponent();
                break;
                
            case GameState.Simulation:
                if (turnIndicatorText != null) turnIndicatorText.gameObject.SetActive(false);
                if(vcamWide != null && vcamHand != null) { vcamWide.SetActive(true); vcamHand.SetActive(false); }
                if(battleManager != null) battleManager.StartCombat();
                if (atmosphereManager != null) atmosphereManager.FocusOnCenter();
                break;
                
            case GameState.EndTurn:
                currentTurn++;
                
                if (currentTurn > 6 || battleManager == null || battleManager.playerHP <= 0 || battleManager.opponentHP <= 0)
                    ChangeState(GameState.GameOver);
                else
                {
                    if (networkManager != null && networkManager.isMultiplayer)
                        ChangeState(networkManager.isMyTurnFirst ? GameState.PlayerTurn : GameState.WaitOpponent);
                    else
                        ChangeState(GameState.PlayerTurn);
                }
                break;
                
            case GameState.GameOver:
                Debug.Log("Game Over!");
                
                if (gameOverPanel != null) gameOverPanel.SetActive(true);

                if (battleManager != null)
                {
                    if (battleManager.playerHP > battleManager.opponentHP)
                    {
                        if (gameOverMessageText != null) { gameOverMessageText.text = "YOU WIN! 🎉"; gameOverMessageText.color = Color.green; }
                    }
                    else if (battleManager.opponentHP > battleManager.playerHP)
                    {
                        if (gameOverMessageText != null) { gameOverMessageText.text = "YOU LOSE! 💀"; gameOverMessageText.color = Color.red; }
                    }
                    else
                    {
                        if (gameOverMessageText != null) { gameOverMessageText.text = "DRAW! 🤝"; gameOverMessageText.color = Color.yellow; }
                    }
                }
                if (atmosphereManager != null) atmosphereManager.FocusOnCenter();
                break;
        }
    }

    void Update()
    {
        if (currentState == GameState.PlayerTurn)
        {
            turnTimer -= Time.deltaTime;
            if (timerText != null) timerText.text = $"Time: {Mathf.RoundToInt(turnTimer)}";

            if (turnTimer <= 0) EndPlayerTurn();
        }
    }

    public void EndPlayerTurn()
    {
        if (currentState == GameState.PlayerTurn)
        {
            if (endTurnButton != null) endTurnButton.SetActive(false); 

            if (skillManager != null) skillManager.DisableSkill();
            
            if (currentPlayedCard != null)
            {
                string pName = (PlayerProfileManager.Instance != null) ? PlayerProfileManager.Instance.PlayerName : "Player";
                CardData myData = currentPlayedCard.GetComponent<CardDisplay>().cardData;
                CombatLogManager.Instance.AddLog($"[{pName}] played: {myData.cardName}", Color.green);
            }
            else
            {
                 CombatLogManager.Instance.AddLog("Turn skipped! No card played.", Color.gray);
            }

            if (networkManager != null && networkManager.isMultiplayer)
            {
                if (currentPlayedCard != null)
                {
                    CardData myCardData = currentPlayedCard.GetComponent<CardDisplay>().cardData;
                    networkManager.SendPlayedCard(myCardData);
                    isMyCardOnBoard = true; 
                }
                
                if (isOpponentCardOnBoard) ChangeState(GameState.Simulation);
                else ChangeState(GameState.WaitOpponent);
            }
            else
            {
                ChangeState(GameState.WaitOpponent);
            }
        }
    }

    // SYNC: Added skillId parameter to match network payload
    public void PlayOpponentCardNetwork(string cardName, int atk, int def, int skillId)
    {
        string oppName = (networkManager != null && networkManager.isMultiplayer) ? networkManager.currentOpponentName : "Bot";
        CombatLogManager.Instance.AddLog($"{oppName} played: {cardName}", new Color(1f, 0.4f, 0.4f));

        currentOpponentSkillId = skillId; // Save it for BattleManager

        if (opponentPlayArea != null)
        {
            GameObject dummyCard = OpponentHandManager.Instance.TakeOneCardFromHand();

            if (dummyCard != null)
            {
                dummyCard.transform.DOScale(dummyCard.transform.localScale * 1.2f, 0.3f).SetLoops(2, LoopType.Yoyo);
                dummyCard.transform.DOMove(opponentPlayArea.position, 0.6f).SetEase(Ease.OutQuad);
                
                dummyCard.transform.DORotate(new Vector3(90f, 0f, 0f), 0.6f)
                    .OnComplete(() =>
                    {
                        Destroy(dummyCard);
                        RevealRealOpponentCard(cardName, atk, def);
                    });
            }
            else
            {
                RevealRealOpponentCard(cardName, atk, def);
            }
        }
    }

    private void RevealRealOpponentCard(string cardName, int atk, int def)
    {
        GameObject oppCard = ObjectPooler.Instance.GetCardFromPool(opponentPlayArea.position, Quaternion.Euler(90f, 0f, 0f));
        currentOpponentCard = oppCard; 
        
        CardInteraction interaction = oppCard.GetComponent<CardInteraction>();
        if (interaction != null) interaction.enabled = false;
        
        CardDisplay display = oppCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            CardData realData = null;
            
            if (deckManager != null && deckManager.allAvailableCards != null)
                realData = deckManager.allAvailableCards.Find(c => c.cardName == cardName);

            if (realData != null)
            {
                display.cardData = realData;
                if (display.artworkRenderer != null) 
                    display.artworkRenderer.sprite = realData.cardArtwork;
            }
            else
            {
                display.cardData = ScriptableObject.CreateInstance<CardData>();
                display.cardData.cardName = cardName;
                display.cardData.attack = atk;
                display.cardData.defense = def;
            }

            display.nameText.text = cardName;
            display.atkText.text = $"ATK: {atk}";
            display.defText.text = $"DEF: {def}";
        }
        
        oppCard.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 10, 1);

        isOpponentCardOnBoard = true; 
        
        if (isMyCardOnBoard) ChangeState(GameState.Simulation);
        else ChangeState(GameState.PlayerTurn);
    }

    public void ReturnToMainMenu()
    {
        if (networkManager != null && networkManager.isMultiplayer)
        {
            networkManager.isMultiplayer = false;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}