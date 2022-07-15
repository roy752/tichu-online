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
    public Stack<Util.Trick> trickStack = new Stack<Util.Trick>();

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
    public Card phoenix;

    /// <summary> 
    /// ù��° Ʈ���ΰ�? ù��° Ʈ���� �ƹ��ų� �� �� �ְ� �н��� �Ұ���.
    /// </summary>
    [HideInInspector]
    public bool isFirstTrick;

    [HideInInspector]
    public bool isFirstRound;

    [HideInInspector]
    public int startPlayerIdx;

    [HideInInspector]
    public int passCount = 0;

    [HideInInspector]
    public bool isBirdWishActivated;

    [HideInInspector]
    public static GameManager instance;

    [HideInInspector]
    public static int currentTrickSelectPlayerIdx;

    private int splitCardIdx;

    private void Awake()
    {
        InitializeVariables();
        InitializePlayers();
        MakeCards();
        Util.ShuffleCards(ref cards);

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
        ResetRoundSetting();
        /*
        SplitCardsToPlayer(Global.numberOfCardsLargeTichuPhase);

        StartCoroutine(StartLargeTichuPhaseCoroutine()); //ī�� 8�� �����ְ� ���� Ƽ�� ����
        yield return new WaitUntil(() => phaseChangeFlag);

        SplitCardsToPlayer(Global.numberOfCardsSmallTichuPhase);

        StartCoroutine(StartExchangeCardPhaseCoroutine()); //ī�� 6�� ���� �����ְ� ��ȯ,����Ƽ�� ����
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartReceiveCardPhaseCoroutine()); //��ȯ�� ī�� Ȯ��, ����Ƽ�� ����
        yield return new WaitUntil(() => phaseChangeFlag);
        */

        SplitCardsToPlayer(Util.numberOfCardsPlay);
        foreach (var player in players) Util.SortCard(ref player.cards);

        StartCoroutine(StartMainPlayPhaseCoroutine()); //1,2,3,4���� ���� ������ �÷���
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartDisplayResultCoroutine());
        yield return new WaitUntil(() => phaseChangeFlag);
        //�÷��� ����� ���� ���� ���, ���÷���. �ٽ� ������ �������� �ƴϸ� ������ �������� ����.
    }
   
    IEnumerator StartDisplayResultCoroutine()
    {
        phaseChangeFlag = false;
        isSelectionEnabled = false;
        isMultipleSelectionEnabled = false;

        Util.Score[] TeamScore = new Util.Score[Util.numberOfTeam];
        for (int i = 0; i < TeamScore.Length; ++i) { TeamScore[i].trickScore = 0; TeamScore[i].oneTwoScore = 0; TeamScore[i].tichuScore = 0; }
        
        foreach (var player in players) //Ƽ�� ���� ������ ����Ѵ�.
        {
            player.ResetPerTrick();
            int nowTeamIdx = player.playerNumber % Util.numberOfTeam;

            if (player.largeTichuFlag)
            {
                if (player.ranking == 1) TeamScore[nowTeamIdx].tichuScore += Util.largeTichuScore;
                else TeamScore[nowTeamIdx].tichuScore -= Util.largeTichuScore;
            }
            else if (player.smallTichuFlag)
            {
                if (player.ranking == 1) TeamScore[nowTeamIdx].tichuScore += Util.smallTichuScore;
                else TeamScore[nowTeamIdx].tichuScore -= Util.smallTichuScore;
            }
        }

        int? oneTwoPlayerIdx = FindIfOneTwo(); // ������ �������� ��� ī�� ������ ������� �ʰ� ���� ������ �߰�.
        if (oneTwoPlayerIdx != null)
        {
            TeamScore[(int)oneTwoPlayerIdx].oneTwoScore = 200;
        }
        else
        {
            GamePlayer firstPlace = FindFirstPlace();
            GamePlayer lastPlace = FindLastPlace();
            if (firstPlace == null) Debug.LogError(Util.findFirstPlaceError);
            if (lastPlace == null) Debug.LogError(Util.findLastPlaceError);

            firstPlace.roundScore += lastPlace.roundScore; lastPlace.roundScore = 0; //�õ��� �� Ʈ���� ���� 1��� �ش�.
            TeamScore[1 - lastPlace.playerNumber % 2].trickScore += lastPlace.cards.Sum(x => x.score);

            for (int idx = 0; idx < Util.numberOfPlayers; ++idx) TeamScore[idx % Util.numberOfTeam].trickScore += players[idx].roundScore;
        }

        for (int idx = 0; idx < Util.numberOfTeam; ++idx) TeamScore[idx].previousScore = players[idx].totalScore;

        for(int idx = 0; idx <Util.numberOfPlayers; ++idx)
        {
            var nowTeam = TeamScore[idx % Util.numberOfTeam];
            players[idx].totalScore += nowTeam.trickScore + nowTeam.tichuScore + nowTeam.oneTwoScore;
            players[idx].roundScore = 0;
        }

        if (trickStack.Count > 0)
        {
            foreach (var card in trickStack.Peek().cards) { card.isFixed = false; card.transform.position = Util.hiddenCardPosition; }
        }

        UIManager.instance.RenderPlayerInfo();

        UIManager.instance.ActivateRoundResult(TeamScore);
        UIManager.instance.Wait(Util.roundResultDuration);
        yield return new WaitUntil(() => UIManager.instance.IsWaitFinished());
        UIManager.instance.DeactivateRoundResult();

        phaseChangeFlag = true;
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

    /// <summary>
    /// Ʈ���� ������ �� �÷��׸� true��.
    /// </summary>
    public bool trickFinishFlag = false;
    
    IEnumerator StartTrickCoroutine()
    {
        //���� �÷��̾� ã��
        //�÷��̾ �� ���� �����ϰ�
        //��� �н��϶����� ī�� ���� �ݺ�
        isTrickEnd = false;

        ResetTrickSetting();
        while(trickFinishFlag==false)
        {
            currentPlayer = players[startPlayerIdx % Util.numberOfPlayers];
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
        foreach (string cardName in Enum.GetNames(typeof(Util.CardType)))
        {
            if (typeNumber < Util.numberOfGeneralCardType)
            {
                for (int i = 1; i <= Util.numberOfCardsGeneral; ++i)
                {
                    var nowCard = (
                                    Instantiate(Resources.Load(Util.prefabPath + Util.GetCardName(cardName, i)),Util.hiddenCardPosition,
                                                Util.initialCardRotation,cardsParent.transform) as GameObject
                                   ).GetComponent<Card>();

                    
                    nowCard.cardName = Util.GetCardName(cardName, i); nowCard.type = (Util.CardType)Enum.Parse(typeof(Util.CardType),cardName); 
                    nowCard.value = Util.generalCardsValue[i]; nowCard.id = idNumber; nowCard.score = Util.generalCardsScore[i];
                    cards.Add(nowCard);
                    idNumber++;
                }
                typeNumber++;
            }
            else
            {
                var nowCard = (
                                Instantiate(Resources.Load(Util.prefabPath + cardName), Util.hiddenCardPosition,
                                           Util.initialCardRotation,cardsParent.transform) as GameObject
                              ).GetComponent<Card>();
                
                nowCard.cardName = cardName; nowCard.type = (Util.CardType)Enum.Parse(typeof(Util.CardType), cardName);
                nowCard.value = Util.specialCardsValue[idx]; nowCard.id = idNumber; nowCard.score = Util.specialCardsScore[idx];
                cards.Add(nowCard);
                if (nowCard.type == Util.CardType.Phoenix) phoenix = nowCard;
                idNumber++; idx++; typeNumber++;
            }
        }
    }

    void InitializePlayers()
    {
        players = new GamePlayer[Util.numberOfPlayers];

        for(int idx = 0; idx<Util.numberOfPlayers; ++idx)
        {
            players[idx] = GameObject.Find(Util.playerObjectNames[idx]).GetComponent<GamePlayer>();
            players[idx].playerNumber = idx;
            players[idx].playerName   = Util.playerNames[idx];
        }
    }

    void InitializeVariables()
    {
        cardsParent = GameObject.Find(Util.cardsParentObjectName);
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0) && isSelectionEnabled)
        {
            GameObject hitObject = Util.GetHitObject( Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (hitObject != null) hitObject.GetComponent<SelectionHandler>().ToggleSelection();
        }
    }

    private void FindBirdIfFirst()
    {
        if(isFirstRound)
        {
            isFirstRound = false;
            for(int idx = 0; idx<Util.numberOfPlayers; ++idx)
            {
                if(players[idx].cards.Any(x=>x.type==Util.CardType.Bird)==true) //���� �ٲ� �� ������
                {
                    startPlayerIdx = idx;
                    return;
                }
            }
        }
    }

    public bool isTrickValid(Util.Trick trick)
    {
        if (trick.trickType == Util.TrickType.IsNotTrick||trick.trickType == Util.TrickType.Blank)
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
            if (trick.trickType == Util.TrickType.StraightFlushBomb)
            {
                if (topTrick.trickType == Util.TrickType.StraightFlushBomb)
                {
                    if (trick.trickLength > topTrick.trickLength) return true;
                    else if (trick.trickLength == topTrick.trickLength && trick.trickValue > topTrick.trickValue) return true;
                    else return false;
                }
                else return true;
            }
            else if (trick.trickType == Util.TrickType.FourCardBomb)
            {
                if (topTrick.trickType == Util.TrickType.StraightFlushBomb) return false;
                else if (topTrick.trickType == Util.TrickType.FourCardBomb)
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

    public int CountFinishedPlayer()
    {
        int ret = 0;
        foreach(var player in players)
        {
            if (player.isFinished == true) ++ret; 
        }
        return ret;
    }


    public bool IsTrickAllPassed()
    {
        int idx = currentPlayer.playerNumber + 1;
        for (int i = 0; i < Util.numberOfSlots; ++i)
        {
            var nowPlayer = players[(idx + i) % Util.numberOfPlayers];
            if (nowPlayer.isTrickPassed == false) return false;
        }
        return true;
    }

    public bool IsBombAllPassed()
    {
        int idx = currentPlayer.playerNumber;
        for(int i=0; i<Util.numberOfPlayers; ++i)
        {
            var nowPlayer = players[(idx + i) % Util.numberOfPlayers];
            if (nowPlayer.isBombPassed == false) return false;
        }
        return true;
    }

    public void ResetTrickSetting()
    {
        trickFinishFlag = false;
        isFirstTrick = true;
        foreach (var player in players) player.ResetPerTrick();
        FindBirdIfFirst();
    }

    public void ResetRoundSetting()
    {
        isFirstRound = true;
        isBirdWishActivated = false;
        foreach (var player in players) player.ResetPerRound();
    }
    
    public bool IsDragonOnTop()
    {
        return trickStack.Peek().cards.Any(x => x.type == Util.CardType.Dragon) == true;
    }

    public int? FindIfOneTwo()
    {
        if (players[0].isFinished == true && players[2].isFinished == true && players[1].isFinished == false && players[3].isFinished == false)
        {
            return 0;
        }
        else if (players[1].isFinished == true && players[3].isFinished == true && players[0].isFinished == false && players[2].isFinished == false)
        {
            return 1;
        }
        else return null;
    }

    public GamePlayer FindFirstPlace()
    {
        foreach (var player in players) if (player.ranking == 1) return player;
        return null;
    }

    public GamePlayer FindLastPlace()
    {
        foreach (var player in players) if (player.isFinished == false) return player;
        return null;
    }

    public void RestorePhoenixValue()
    {
        phoenix.value = Util.specialCardsValue[2];
    }
}
