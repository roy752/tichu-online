using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public List<Global.Card> cards = new List<Global.Card>();

    [HideInInspector]
    public List<Global.Card> cardsObjectPool = new List<Global.Card>();

    [HideInInspector]
    public GamePlayer[] players;

    [HideInInspector]
    public GamePlayer currentPlayer;

    [HideInInspector]
    public Global.Card currentCard;

    [HideInInspector]
    public SlotSelectHandler currentSlot;

    [HideInInspector]
    public GameObject cardsParent;

    [HideInInspector]
    public bool phaseChangeFlag;

    [HideInInspector]
    public bool isMultipleSelectionAllowed;

    [HideInInspector]
    public bool isSelectionEnabled;

    [HideInInspector]
    public static GameManager instance;


    private int splitCardIdx = 0;

    private void Awake()
    {
        InitializeVariables();
        InitializePlayers();
        MakeCards();
        Global.ShuffleCards(ref cards);

        instance = this;
    }

    void Start()
    {
        StartCoroutine(StartPlay());
    }

    private void Update()
    {
        HandleSelection();
    }

    IEnumerator StartPlay()
    {
        SplitCardsToPlayer(Global.numberOfCardsLargeTichuPhase);
        StartCoroutine(StartLargeTichuPhaseCoroutine());
        yield return new WaitUntil(()=>phaseChangeFlag);

        SplitCardsToPlayer(Global.numberOfCardsSmallTichuPhase);
        StartCoroutine(StartExchangeCardPhaseCoroutine());
        yield return new WaitUntil(() => phaseChangeFlag);
    }

    IEnumerator StartLargeTichuPhaseCoroutine()
    {
        phaseChangeFlag = false;

        foreach (var player in players)
        {
            currentPlayer = player;
            player.ChooseLargeTichu();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }
        phaseChangeFlag = true;
    }
    
    IEnumerator StartExchangeCardPhaseCoroutine()
    {
        phaseChangeFlag = false;

        isSelectionEnabled = true;
        foreach(var player in players)
        {
            currentPlayer = player;
            player.ExchangeCards();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }
        phaseChangeFlag = true;
    }

    void SplitCardsToPlayer(int num)
    {
        foreach(var player in players)
        {
            player.AddCards(cards.GetRange(splitCardIdx, num));
            splitCardIdx += num;
        }
    }

    void MakeCards()
    {
        int type = 0;
        int id = 0;

        foreach (string cardName in Enum.GetNames(typeof(Global.GeneralCardName)))
        {
            for (int i = 1; i <= Global.numberOfCardsGeneral; ++i)
            {
                Global.Card cardInstance = new Global.Card();

                cardInstance.cardName = Global.GetCardName(cardName, i);
                cardInstance.type     = type;
                cardInstance.value    = Global.generalCardsValue[i];
                cardInstance.id       = id;

                cards.Add(cardInstance);
                id++;
            }
            type++;
        }
        int idx = 0;
        foreach (string cardName in Enum.GetNames(typeof(Global.SpecialCardName)))
        {
            Global.Card cardInstance = new Global.Card();

            cardInstance.cardName = cardName;
            cardInstance.type     = type;
            cardInstance.value    = Global.specialCardsValue[idx];
            cardInstance.id       = id;

            cards.Add(cardInstance);
            id++;
            idx++;
            type++;
        }

        foreach (var item in cards)
        {
            item.cardObject = Instantiate(Resources.Load(Global.prefabPath + item.cardName),
                                          Global.hiddenCardPosition,
                                          Global.initialCardRotation,
                                          cardsParent.transform) as GameObject;
            item.cardObject.name = item.cardName;
        }
    }

    void InitializePlayers()
    {
        players = new GamePlayer[Global.numberOfPlayers];

        for(int idx = 0; idx<Global.numberOfPlayers; ++idx)
        {
            players[idx] = GameObject.Find(Global.playerObjectNames[idx]).GetComponent<GamePlayer>();
            players[idx].playerNumber = idx;
            players[idx].playerName   = Global.playerNames[idx];
        }
    }

    void InitializeVariables()
    {
        cardsParent = GameObject.Find(Global.cardsParentObjectName);
    }

    public void SetCurrentCard(GameObject inputObject)
    {
        foreach(var item in cards)
        {
            if(item.cardObject==inputObject)
            {
                currentCard = item;
                break;
            }
        }
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0) && isSelectionEnabled)
        {
            GameObject hitObject = Global.GetHitObject( Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (hitObject != null) hitObject.GetComponent<SelectionHandler>().ToggleSelection();
        }
    }
}
