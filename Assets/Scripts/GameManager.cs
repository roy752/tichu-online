using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public List<Card> cards = new List<Card>();

    [HideInInspector]
    public List<Card> cardsObjectPool = new List<Card>();

    [HideInInspector]
    public Stack<Global.Trick> trickStack = new Stack<Global.Trick>();

    [HideInInspector]
    public GamePlayer[] players;

    [HideInInspector]
    public GamePlayer currentPlayer;

    [HideInInspector]
    public Card currentCard;

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
    public bool isFirstRound;

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
        //UIManager.instance.RenderTrickCard(cards.Take(5).ToList());
        StartCoroutine(StartPlay());
    }

    private void Update()
    {
        HandleSelection();
    }

    IEnumerator StartPlay()
    {
        SplitCardsToPlayer(Global.numberOfCardsLargeTichuPhase);

        StartCoroutine(StartLargeTichuPhaseCoroutine()); //ī�� 8�� �����ְ� ���� Ƽ�� ����
        yield return new WaitUntil(() => phaseChangeFlag);

        SplitCardsToPlayer(Global.numberOfCardsSmallTichuPhase);

        StartCoroutine(StartExchangeCardPhaseCoroutine()); //ī�� 6�� ���� �����ְ� ��ȯ,����Ƽ�� ����
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartReceiveCardPhaseCoroutine()); //��ȯ�� ī�� Ȯ��, ����Ƽ�� ����
        yield return new WaitUntil(() => phaseChangeFlag);
        
        StartCoroutine(StartMainPlayPhaseCoroutine()); //1,2,3,4���� ���� ������ �÷���
        yield return new WaitUntil(() => phaseChangeFlag);

        //�÷��� ����� ���� ���� ���, ���÷���. �ٽ� ������ �������� �ƴϸ� ������ �������� ����.
    }
    
    IEnumerator StartMainPlayPhaseCoroutine()
    {
        phaseChangeFlag = false;

        isFirstRound = true;
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
        //���� �÷��̾� ã��
        //�÷��̾ �� ���� �����ϰ�
        //��� �н��϶����� ī�� ���� �ݺ�
        isTrickEnd = false;
        trickFinishFlag = false;
        isFirstTrick = true;

        FindStartPlayer();
        int idx = startPlayerIdx;

        while(trickFinishFlag==false)
        {
            currentPlayer = players[(idx++) % Global.numberOfPlayers];
            currentPlayer.SelectTrick();
            yield return new WaitUntil(() => currentPlayer.coroutineFinishFlag); //��ź ������ ���?
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
        int idx = 0;
        foreach (string cardName in Enum.GetNames(typeof(Global.CardType)))
        {
            if (typeNumber < Global.numberOfGeneralCardType)
            {
                for (int i = 1; i <= Global.numberOfCardsGeneral; ++i)
                {
                    var nowCard = (
                                    Instantiate(Resources.Load(Global.prefabPath + Global.GetCardName(cardName, i)),Global.hiddenCardPosition,
                                                Global.initialCardRotation,cardsParent.transform) as GameObject
                                   ).GetComponent<Card>();

                    
                    nowCard.cardName = Global.GetCardName(cardName, i); nowCard.type = (Global.CardType)Enum.Parse(typeof(Global.CardType),cardName); 
                    nowCard.value = Global.generalCardsValue[i]; nowCard.id = idNumber;
                    cards.Add(nowCard);
                    idNumber++;
                }
                typeNumber++;
            }
            else
            {
                var nowCard = (
                                Instantiate(Resources.Load(Global.prefabPath + cardName), Global.hiddenCardPosition,
                                           Global.initialCardRotation,cardsParent.transform) as GameObject
                              ).GetComponent<Card>();
                
                nowCard.cardName = cardName; nowCard.type = (Global.CardType)Enum.Parse(typeof(Global.CardType), cardName);
                nowCard.value = Global.specialCardsValue[idx]; nowCard.id = idNumber;
                cards.Add(nowCard);
                idNumber++; idx++; typeNumber++;
            }
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
        if(isFirstRound)
        {
            isFirstRound = false;
            for(int idx = 0; idx<Global.numberOfPlayers; ++idx)
            {
                if(players[idx].cards.Any(x=>x.value==Global.specialCardsValue[0])==true) //���� �ٲ� �� ������
                {
                    startPlayerIdx = idx;
                    return;
                }
            }
        }
    }

    public bool isTrickValid(Global.Trick trick)
    {
        if (trick.trickType == Global.TrickType.IsNotTrick||trick.trickType == Global.TrickType.Blank)
        {
            return false;
        }
        else if (isFirstTrick)
        {
            isFirstTrick = false;
            return true;
        }
        else
        {
            var topTrick = trickStack.Peek();
            if (trick.trickType == Global.TrickType.StraightFlushBomb)
            {
                if (topTrick.trickType == Global.TrickType.StraightFlushBomb)
                {
                    if (trick.trickLength > topTrick.trickLength) return true;
                    else if (trick.trickLength == topTrick.trickLength && trick.trickValue > topTrick.trickValue) return true;
                    else return false;
                }
                else return true;
            }
            else if (trick.trickType == Global.TrickType.FourCardBomb)
            {
                if (topTrick.trickType == Global.TrickType.StraightFlushBomb) return false;
                else if (topTrick.trickType == Global.TrickType.FourCardBomb)
                {
                    if (trick.trickValue > topTrick.trickValue) return true;
                    else return false;
                }
                else return true;
            }
            else
            {
                if (trick.trickType != topTrick.trickType) return false;
                else if (trick.trickLength == topTrick.trickLength && trick.trickValue > topTrick.trickValue) return true;
                else return false;
            } 
        }
    }
}
