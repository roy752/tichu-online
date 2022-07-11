using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public List<Global.Card> cards = new List<Global.Card>();

    [HideInInspector]
    public List<Global.Card> cardsObjectPool = new List<Global.Card>();

    [HideInInspector]
    public Stack<Global.Trick> trickStack = new Stack<Global.Trick>();

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
    public bool isMultipleSelectionEnabled;

    [HideInInspector]
    public bool isSelectionEnabled;

    [HideInInspector]
    public bool isTrickEnd;

    [HideInInspector]
    public bool isRoundEnd;

    [HideInInspector]
    public bool isGameEnd;

    [HideInInspector]
    public bool isBirdUsed;

    [HideInInspector]
    public bool isFirstTrick;

    [HideInInspector]
    public int startPlayerIdx;

    [HideInInspector]
    public static GameManager instance;

    private int splitCardIdx;

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
        /*
        SplitCardsToPlayer(Global.numberOfCardsLargeTichuPhase);

        StartCoroutine(StartLargeTichuPhaseCoroutine()); //카드 8장 나눠주고 라지 티츄 결정
        yield return new WaitUntil(() => phaseChangeFlag);

        SplitCardsToPlayer(Global.numberOfCardsSmallTichuPhase);

        StartCoroutine(StartExchangeCardPhaseCoroutine()); //카드 6장 마저 나눠주고 교환,스몰티츄 결정
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartReceiveCardPhaseCoroutine()); //교환한 카드 확인, 스몰티츄 결정
        yield return new WaitUntil(() => phaseChangeFlag);
        */
        SplitCardsToPlayer(Global.numberOfCardsForLineInPlayPhase);
        StartCoroutine(StartMainPlayPhaseCoroutine()); //1,2,3,4등이 나뉠 때까지 플레이
        yield return new WaitUntil(() => phaseChangeFlag);

        //플레이 결과에 따른 점수 계산, 디스플레이. 다시 게임을 시작할지 아니면 게임이 끝났는지 결정.
    }
    
    IEnumerator StartMainPlayPhaseCoroutine()
    {
        phaseChangeFlag = false;

        isSelectionEnabled = true;
        isMultipleSelectionEnabled = true;
        
        while(isRoundEnd==false)
        {
            StartCoroutine(StartTrickCoroutine());
            yield return new WaitUntil(() => isTrickEnd);
        }

        phaseChangeFlag = true;
    }

    public bool trickFinishFlag = false;
    
    IEnumerator StartTrickCoroutine()
    {
        //시작 플레이어 찾고
        //플레이어가 낼 족보 결정하고
        //모두 패스일때까지 카드 내기 반복
        isTrickEnd = false;
        trickFinishFlag = false;
        isFirstTrick = true;

        FindStartPlayer();
        int idx = startPlayerIdx;

        while(trickFinishFlag==false)
        {
            currentPlayer = players[(idx++) % Global.numberOfPlayers];
            currentPlayer.SelectTrick();
            yield return new WaitUntil(() => currentPlayer.coroutineFinishFlag); //폭탄 구현은 어떻게?
        }

        isTrickEnd = true;
    }

    IEnumerator StartReceiveCardPhaseCoroutine()
    {
        phaseChangeFlag = false;

        isSelectionEnabled = false;
        foreach(var player in players)
        {
            currentPlayer = player;
            player.ReceiveCard();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }

        phaseChangeFlag = true;
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
        int typeNumber = 0;
        int idNumber = 0;
        foreach (string cardName in Enum.GetNames(typeof(Global.GeneralCardName)))
        {
            for (int i = 1; i <= Global.numberOfCardsGeneral; ++i)
            {
                cards.Add(new Global.Card() { cardName = Global.GetCardName(cardName, i), type = typeNumber, value = Global.generalCardsValue[i], id = idNumber });
                idNumber++;
            }
            typeNumber++;
        }

        int idx = 0;
        foreach (string nowCardName in Enum.GetNames(typeof(Global.SpecialCardName)))
        {
            cards.Add(new Global.Card() { cardName = nowCardName, type = typeNumber, value = Global.specialCardsValue[idx], id = idNumber });
            idNumber++; idx++; typeNumber++;
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

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0) && isSelectionEnabled)
        {
            GameObject hitObject = Global.GetHitObject( Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (hitObject != null) hitObject.GetComponent<SelectionHandler>().ToggleSelection();
        }
    }

    private void FindStartPlayer()
    {
        if(isFirstTrick)
        {
            isFirstTrick = false;
            for(int idx = 0; idx<Global.numberOfPlayers; ++idx)
            {
                if(players[idx].cards.Any(x=>x.value==Global.specialCardsValue[(int)Global.SpecialCardName.Bird])==true)
                {
                    startPlayerIdx = idx;
                    return;
                }
            }
        }
    }
}
