using UnityEngine;
using System;
using System.Collections;       
using System.Collections.Generic;
using SocketIOClient;
using TMPro;                    

public class MatchResponse
{
    public string room { get; set; }
    public int playerRole { get; set; }
    public bool isMyTurn { get; set; }
    public string opponentName { get; set; }
}

public class PlayCardRequest
{
    public string room { get; set; }
    public OpponentCardData cardData { get; set; }
}

public class OpponentCardResponse
{
    public OpponentCardData cardData { get; set; }
}

public class OpponentCardData
{
    public string cardName { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }
    public int skillId { get; set; } // ADDED: Sent to opponent to prevent desync
}

public class NetworkManager : MonoBehaviour
{
    public bool isMultiplayer = false;
    private SocketIOUnity socket;
    private Queue<Action> executeOnMainThread = new Queue<Action>();

    public string currentRoom = "";
    public string currentOpponentName = "Bot"; 
    public bool isMyTurnFirst = false;

    [Header("Matchmaking UI")]
    public GameObject matchmakingPanel; 
    public TMP_Text timerText;          
    public GameObject mainMenuCanvas;   
    
    private Coroutine timerCoroutine;

    void Start()
    {
        // Unity NetworkManager.cs içindeki satır:
        var uri = new Uri("https://cardcase-server.onrender.com");
        socket = new SocketIOUnity(uri);

        socket.OnConnected += (sender, e) =>
        {
            EnqueueMainThreadAction(() => Debug.Log("🟢 Successfully connected to the server!"));
        };

        socket.OnDisconnected += (sender, e) =>
        {
            EnqueueMainThreadAction(() => Debug.Log("🔴 Disconnected from the server."));
        };

        socket.On("waiting_for_opponent", (response) =>
        {
            EnqueueMainThreadAction(() => Debug.Log("⏳ Waiting for an opponent in queue..."));
        });

        socket.On("match_found", (response) =>
        {
            var data = response.GetValue<MatchResponse>();

            EnqueueMainThreadAction(() => 
            {
                if (timerCoroutine != null) StopCoroutine(timerCoroutine);
                if (matchmakingPanel != null) matchmakingPanel.SetActive(false);

                currentRoom = data.room;
                isMyTurnFirst = data.isMyTurn;
                currentOpponentName = data.opponentName;

                Debug.Log($"⚔️ MATCH FOUND! Room: {currentRoom} | You are Player {data.playerRole}.");
                
                FindObjectOfType<TurnManager>().ChangeState(GameState.Setup);
            });
        });

        socket.On("start_game", (response) =>
        {
            EnqueueMainThreadAction(() => 
            {
                Debug.Log("🚀 Both players ready, starting the game!");
                
                FindObjectOfType<DeckManager>().SpawnCards();

                if (isMyTurnFirst)
                    FindObjectOfType<TurnManager>().ChangeState(GameState.PlayerTurn);
                else
                    FindObjectOfType<TurnManager>().ChangeState(GameState.WaitOpponent);
            });
        });

        socket.On("opponent_played_card", (response) =>
        {
            EnqueueMainThreadAction(() => Debug.Log("📡 RECEIVED PACKET FROM SERVER: " + response.ToString()));

            try 
            {
                var data = response.GetValue<OpponentCardResponse>();

                EnqueueMainThreadAction(() => 
                {
                    string cardName = data.cardData.cardName;
                    int attack = data.cardData.attack;
                    int defense = data.cardData.defense;
                    int skillId = data.cardData.skillId; // SYNC: Receiving opponent's skill

                    Debug.Log($"⚠️ Opponent played: {cardName} (ATK: {attack}, DEF: {defense}, SKILL: {skillId})");
                    
                    FindObjectOfType<TurnManager>().PlayOpponentCardNetwork(cardName, attack, defense, skillId);
                });
            }
            catch (Exception ex)
            {
                EnqueueMainThreadAction(() => Debug.LogError("❌ JSON PARSE ERROR: " + ex.Message));
            }
        });

        socket.Connect();
    }

    void Update()
    {
        lock (executeOnMainThread)
        {
            while (executeOnMainThread.Count > 0)
            {
                executeOnMainThread.Dequeue().Invoke();
            }
        }
    }

    public void FindMatch()
    {
        if (socket != null) 
        {
            string myName = PlayerProfileManager.Instance.PlayerName;
            socket.Emit("find_match", new { playerName = myName });
        }
    }

    public void StartOnlineMatch()
    {
        isMultiplayer = true;
        FindMatch(); 
        
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(MatchmakingTimerRoutine());
    }

    public void CancelMatchmaking()
    {
        Debug.Log("Matchmaking cancelled or timed out.");
        
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (socket != null)
        {
            socket.Emit("leave_queue"); 
        }

        if (matchmakingPanel != null) matchmakingPanel.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        
        isMultiplayer = false;
    }

    private IEnumerator MatchmakingTimerRoutine()
    {
        int timeLeft = 30; 
        
        if (matchmakingPanel != null) matchmakingPanel.SetActive(true);
        
        while (timeLeft > 0)
        {
            if (timerText != null) timerText.text = $"{timeLeft}";
            yield return new WaitForSeconds(1f); 
            timeLeft--;
        }

        CancelMatchmaking();
    }

    public void SendPlayerReady()
    {
        if (socket != null && !string.IsNullOrEmpty(currentRoom))
        {
            socket.Emit("player_ready", new { room = currentRoom });
            Debug.Log("Ready status sent to server. Waiting for opponent...");
        }
    }

    public void SendPlayedCard(CardData playedCard)
    {
        if (socket != null && !string.IsNullOrEmpty(currentRoom) && playedCard != null)
        {
            // SYNC: Read our local active skill to send it to the opponent
            int mySkillId = -1;
            SkillManager sm = FindObjectOfType<SkillManager>();
            if (sm != null) mySkillId = sm.activeSkillId;

            PlayCardRequest requestData = new PlayCardRequest
            {
                room = currentRoom,
                cardData = new OpponentCardData
                {
                    cardName = playedCard.cardName,
                    attack = playedCard.attack,
                    defense = playedCard.defense,
                    skillId = mySkillId 
                }
            };

            socket.Emit("play_card", requestData);
            Debug.Log($"Card ({playedCard.cardName}) sent via DTO!");
        }
    }

    private void EnqueueMainThreadAction(Action action)
    {
        lock (executeOnMainThread)
        {
            executeOnMainThread.Enqueue(action);
        }
    }

    void OnDestroy()
    {
        if (socket != null) socket.Disconnect();
    }
}