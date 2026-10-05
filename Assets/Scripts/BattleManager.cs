using UnityEngine;
using TMPro;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    private NetworkManager networkManager;

    [Header("Health Values")]
    public int playerHP = 100;
    public int opponentHP = 100;

    [Header("UI References")]
    public TMP_Text hpText;

    [Header("Effects")]
    public GameObject floatingTextPrefab; 

    void Awake()
    {
        networkManager = FindAnyObjectByType<NetworkManager>();
    }

    void Start()
    {
        UpdateHPUI();
    }

    public void UpdateHPUI()
    {
        if (hpText == null)
        {
            return;
        }

        string myName = ResolvePlayerName();
        string oppName = ResolveOpponentName();

        int displayPlayerHP = Mathf.Max(0, playerHP);
        int displayOpponentHP = Mathf.Max(0, opponentHP);

        hpText.text = $"{myName}: {displayPlayerHP}  |  {oppName}: {displayOpponentHP}";
    }

    private string ResolvePlayerName()
    {
        string myName = (PlayerProfileManager.Instance != null) ? PlayerProfileManager.Instance.PlayerName : "Player";
        return string.IsNullOrEmpty(myName) ? "Player" : myName;
    }

    private string ResolveOpponentName()
    {
        if (networkManager != null && networkManager.isMultiplayer && !string.IsNullOrEmpty(networkManager.currentOpponentName))
        {
            return networkManager.currentOpponentName;
        }

        return "Bot";
    }

    public void StartCombat()
    {
        StartCoroutine(SimulationRoutine());
    }

    private void ShowFloatingText(string message, Color color, Vector3 position)
    {
        if (floatingTextPrefab != null)
        {
            Vector3 spawnPos = position + new Vector3(0, 1f, 0);
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
            
            FloatingText floatingText = textObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.Setup(message, color);
            }
        }
    }

    private void LogAndFloat(string message, Color color, Vector3 position)
    {
        ShowFloatingText(message, color, position);
        AddCombatLog(message, color);
    }

    private void AddCombatLog(string message, Color color)
    {
        if (CombatLogManager.Instance != null)
        {
            CombatLogManager.Instance.AddLog(message, color);
        }
    }

    // SYNC: Symmetrical skill applier for deterministic multiplayer
    private void ApplySkill(int skillId, bool isLocalPlayer, ref int myHP, ref int myAtk, ref int myDef, ref int enemyAtk, ref int enemyDef, ref int myShield, Vector3 myPos, Vector3 enemyPos)
    {
        string name = isLocalPlayer ? "You" : "Opponent";
        Color buffColor = isLocalPlayer ? Color.green : Color.red;

        switch (skillId)
        {
            case 0: 
                myHP += 20;
                LogAndFloat($"{name}: +20 HP", buffColor, myPos);
                break;
            case 1: 
                myAtk += 10;
                LogAndFloat($"{name}: +10 ATK", Color.yellow, myPos);
                break;
            case 2: 
                myDef += 10;
                LogAndFloat($"{name}: +10 DEF", Color.blue, myPos);
                break;
            case 3: 
                enemyAtk = Mathf.Max(0, enemyAtk - 10);
                LogAndFloat($"{name}: Enemy -10 ATK", Color.magenta, enemyPos);
                break;
            case 4: 
                enemyDef = Mathf.Max(0, enemyDef - 10);
                LogAndFloat($"{name}: Enemy -10 DEF", Color.magenta, enemyPos);
                break;
            case 5: 
                myShield = 15;
                enemyAtk += 5; 
                LogAndFloat($"{name}: SHIELD ACTIVE", new Color(0, 1f, 1f), myPos);
                break;
        }
    }

    private IEnumerator SimulationRoutine()
    {
        TurnManager turnManager = FindAnyObjectByType<TurnManager>();
        if (turnManager == null)
        {
            yield break;
        }

        CombatState state = CreateCombatState(turnManager);
        yield return new WaitForSeconds(1.5f);

        ApplySkillPhase(state, turnManager);
        yield return new WaitForSeconds(1.2f);

        UpdateHPUI();
        yield return ResolveCombatActions(state);

        yield return new WaitForSeconds(1.5f);
        CleanupCombat(turnManager, state);
        turnManager.ChangeState(GameState.EndTurn);
    }

    private CombatState CreateCombatState(TurnManager turnManager)
    {
        CardInteraction playerCard = turnManager.currentPlayedCard;
        GameObject botCardObj = turnManager.currentOpponentCard ?? GameObject.Find("BotPlayedCard");
        CardDisplay botCard = botCardObj != null ? botCardObj.GetComponent<CardDisplay>() : null;

        bool playerHasCard = playerCard != null;
        bool botHasCard = botCardObj != null;

        return new CombatState
        {
            PlayerCard = playerCard,
            BotCardObject = botCardObj,
            BotCard = botCard,
            PlayerHasCard = playerHasCard,
            BotHasCard = botHasCard,
            PlayerAttack = playerHasCard ? playerCard.GetComponent<CardDisplay>().cardData.attack : 0,
            PlayerDefense = playerHasCard ? playerCard.GetComponent<CardDisplay>().cardData.defense : 0,
            BotAttack = botHasCard && botCard != null ? botCard.cardData.attack : 0,
            BotDefense = botHasCard && botCard != null ? botCard.cardData.defense : 0,
            PlayerPosition = playerHasCard ? playerCard.transform.position : new Vector3(0, 1f, -3f),
            BotPosition = botHasCard && botCardObj != null ? botCardObj.transform.position : new Vector3(0, 1f, 3f)
        };
    }

    private void ApplySkillPhase(CombatState state, TurnManager turnManager)
    {
        SkillManager skillManager = FindAnyObjectByType<SkillManager>();
        int localSkill = (skillManager != null && state.PlayerHasCard) ? skillManager.activeSkillId : -1;
        int oppSkill = state.BotHasCard ? turnManager.currentOpponentSkillId : -1;

        if (localSkill != -1)
        {
            ApplySkill(localSkill, true, ref playerHP, ref state.PlayerAttack, ref state.PlayerDefense, ref state.BotAttack, ref state.BotDefense, ref state.PlayerShield, state.PlayerPosition, state.BotPosition);
        }

        if (oppSkill != -1)
        {
            ApplySkill(oppSkill, false, ref opponentHP, ref state.BotAttack, ref state.BotDefense, ref state.PlayerAttack, ref state.PlayerDefense, ref state.OpponentShield, state.BotPosition, state.PlayerPosition);
        }
    }

    private IEnumerator ResolveCombatActions(CombatState state)
    {
        if (state.PlayerHasCard)
        {
            yield return ResolvePlayerAttack(state, true, state.PlayerAttack, state.BotDefense, state.BotPosition, state.BotHasCard);
            UpdateHPUI();
            yield return new WaitForSeconds(1f);
        }
        else
        {
            LogAndFloat("NO CARD PLAYED!", Color.red, state.PlayerPosition);
            yield return new WaitForSeconds(1f);
        }

        if (state.BotHasCard)
        {
            yield return ResolveOpponentAttack(state, true, state.BotAttack, state.PlayerDefense, state.PlayerPosition, state.PlayerHasCard);
            UpdateHPUI();
        }
    }

    private void CleanupCombat(TurnManager turnManager, CombatState state)
    {
        if (state.PlayerCard != null) ObjectPooler.Instance.ReturnToPool(state.PlayerCard.gameObject);
        if (state.BotCardObject != null) ObjectPooler.Instance.ReturnToPool(state.BotCardObject);

        turnManager.isMyCardOnBoard = false;
        turnManager.isOpponentCardOnBoard = false;
        turnManager.UnregisterPlayedCard();
        turnManager.currentOpponentCard = null;
        turnManager.currentOpponentSkillId = -1;
    }

    private class CombatState
    {
        public CardInteraction PlayerCard;
        public GameObject BotCardObject;
        public CardDisplay BotCard;
        public bool PlayerHasCard;
        public bool BotHasCard;
        public int PlayerAttack;
        public int PlayerDefense;
        public int BotAttack;
        public int BotDefense;
        public int PlayerShield;
        public int OpponentShield;
        public Vector3 PlayerPosition;
        public Vector3 BotPosition;
    }

    private IEnumerator ResolvePlayerAttack(CombatState state, bool playerHasCard, int pAtk, int bDef, Vector3 botPos, bool botHasCard)
    {
        if (!playerHasCard)
        {
            LogAndFloat("NO CARD PLAYED!", Color.red, botPos);
            yield break;
        }

        if (pAtk > bDef)
        {
            int damageGiven = pAtk - bDef;

            if (state.OpponentShield > 0)
            {
                if (damageGiven <= state.OpponentShield)
                {
                    LogAndFloat("Opponent: SHIELD BLOCKED", new Color(0, 1f, 1f), botPos);
                    state.OpponentShield -= damageGiven;
                    damageGiven = 0;
                }
                else
                {
                    damageGiven -= state.OpponentShield;
                    LogAndFloat("Opponent: SHIELD BROKEN", new Color(0, 1f, 1f), botPos);
                    state.OpponentShield = 0;
                    yield return new WaitForSeconds(0.6f);
                }
            }

            if (damageGiven > 0)
            {
                opponentHP = Mathf.Max(0, opponentHP - damageGiven);
                LogAndFloat($"Opponent: -{damageGiven} HP", Color.red, botPos);
            }
        }
        else if (botHasCard)
        {
            LogAndFloat("Opponent Blocked!", Color.gray, botPos);
        }
    }

    private IEnumerator ResolveOpponentAttack(CombatState state, bool playerHasCard, int bAtk, int pDef, Vector3 playerPos, bool botHasCard)
    {
        if (bAtk > pDef)
        {
            int incomingDamage = bAtk - pDef;

            if (state.PlayerShield > 0)
            {
                if (incomingDamage <= state.PlayerShield)
                {
                    LogAndFloat("You: SHIELD BLOCKED", new Color(0, 1f, 1f), playerPos);
                    state.PlayerShield -= incomingDamage;
                    incomingDamage = 0;
                }
                else
                {
                    incomingDamage -= state.PlayerShield;
                    LogAndFloat("You: SHIELD BROKEN", new Color(0, 1f, 1f), playerPos);
                    state.PlayerShield = 0;
                    yield return new WaitForSeconds(0.6f);
                }
            }

            if (incomingDamage > 0)
            {
                playerHP = Mathf.Max(0, playerHP - incomingDamage);
                LogAndFloat($"You: -{incomingDamage} HP", Color.red, playerPos);
            }
        }
        else if (playerHasCard)
        {
            LogAndFloat("You Blocked!", Color.gray, playerPos);
        }
    }
}