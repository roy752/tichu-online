using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Util;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public List<Card> cards = new List<Card>();

    [HideInInspector]
    public List<Card> cardsObjectPool = new List<Card>();

    [HideInInspector]
    public Stack<Trick> trickStack = new Stack<Trick>();

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

    [HideInInspector]
    public Card bird;

    /// <summary> 
    /// 첫번째 트릭인가? 첫번째 트릭은 아무거나 낼 수 있고 패스가 불가능.
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
    public int birdWishValue = 0;

    [HideInInspector]
    public static int currentTrickSelectPlayerIdx;

    private int splitCardIdx;

    [HideInInspector]
    public static GameManager instance;

    private void Awake()
    {
        InitializeVariables();
        InitializePlayers();
        MakeCards();

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

    /*
    void TestSetup()
    {
        //52부터 새52 개53 봉54 용55
        
        List<Card> tmp = new List<Card>();
        Trick nowTrick = new Trick(tmp);
        nowTrick.trickType = TrickType.StraightFlushBomb;
        nowTrick.trickLength = 5;
        nowTrick.trickValue  = 7;
        trickStack.Push(nowTrick);

        isBirdWishActivated = true;
        birdWishValue = 8;
        UIManager.instance.ActivateBirdWishNotice(birdWishValue);

        fo(0, 52); fo(0, 9); fo(0, 10);fo(0, 11);fo(0, 12); fo(0, 0);
        fo(1, 3);fo(1, 4);fo(1, 5);fo(1, 6); fo(1, 7);
        //fo(1, 6); fo(1, 7); fo(1, 8);fo(1, 9); fo(1, 54);
    }

    void fo(int idx, int id)
    {
        players[idx].cards.Add(cards.Find(x => x.id == id));
    }
    */
    IEnumerator StartPlay()
    {
        ResetRoundSetting();
        ShuffleCards(ref cards);

        SplitCardsToPlayer(Util.numberOfCardsLargeTichuPhase);

        StartCoroutine(StartLargeTichuPhaseCoroutine()); //카드 8장 나눠주고 라지 티츄 결정
        yield return new WaitUntil(() => phaseChangeFlag);

        SplitCardsToPlayer(Util.numberOfCardsSmallTichuPhase);

        StartCoroutine(StartExchangeCardPhaseCoroutine()); //카드 6장 마저 나눠주고 교환,스몰티츄 결정
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartReceiveCardPhaseCoroutine()); //교환한 카드 확인, 스몰티츄 결정
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartMainPlayPhaseCoroutine()); //1,2,3,4등이 나뉠 때까지 플레이
        yield return new WaitUntil(() => phaseChangeFlag);

        StartCoroutine(StartDisplayResultCoroutine());
        yield return new WaitUntil(() => phaseChangeFlag);
        //플레이 결과에 따른 점수 계산, 디스플레이. 다시 게임을 시작할지 아니면 게임이 끝났는지 결정.
    }
   
    IEnumerator StartDisplayResultCoroutine()
    {
        phaseChangeFlag = false;
        isSelectionEnabled = false;
        isMultipleSelectionEnabled = false;

        Score[] TeamScore = new Score[numberOfTeam];
        for (int i = 0; i < TeamScore.Length; ++i) { TeamScore[i].trickScore = 0; TeamScore[i].oneTwoScore = 0; TeamScore[i].tichuScore = 0; }
        
        foreach (var player in players) //티츄 선언 점수를 계산한다.
        {
            player.ResetPerTrick();
            int nowTeamIdx = player.playerNumber % numberOfTeam;

            if (player.largeTichuFlag)
            {
                if (player.ranking == 1) TeamScore[nowTeamIdx].tichuScore += largeTichuScore;
                else TeamScore[nowTeamIdx].tichuScore -= largeTichuScore;
            }
            else if (player.smallTichuFlag)
            {
                if (player.ranking == 1) TeamScore[nowTeamIdx].tichuScore += smallTichuScore;
                else TeamScore[nowTeamIdx].tichuScore -= smallTichuScore;
            }
        }

        int? oneTwoPlayerIdx = FindIfOneTwo(); // 원투를 성공했을 경우 카드 점수는 계산하지 않고 원투 점수를 추가.
        if (oneTwoPlayerIdx != null)
        {
            TeamScore[(int)oneTwoPlayerIdx].oneTwoScore = 200;
        }
        else
        {
            GamePlayer firstPlace = FindFirstPlace();
            GamePlayer lastPlace = FindLastPlace();
            if (firstPlace == null) Debug.LogError(findFirstPlaceError);
            if (lastPlace == null) Debug.LogError(findLastPlaceError);

            firstPlace.roundScore += lastPlace.roundScore; lastPlace.roundScore = 0; //꼴등은 딴 트릭을 전부 1등에게 준다.
            TeamScore[1 - lastPlace.playerNumber % 2].trickScore += lastPlace.cards.Sum(x => x.score);

            for (int idx = 0; idx < numberOfPlayers; ++idx) TeamScore[idx % numberOfTeam].trickScore += players[idx].roundScore;
        }

        for (int idx = 0; idx < numberOfTeam; ++idx) TeamScore[idx].previousScore = players[idx].totalScore;

        for(int idx = 0; idx <numberOfPlayers; ++idx)
        {
            var nowTeam = TeamScore[idx % numberOfTeam];
            players[idx].totalScore += nowTeam.trickScore + nowTeam.tichuScore + nowTeam.oneTwoScore;
            players[idx].roundScore = 0;
        }

        if (trickStack.Count > 0)
        {
            foreach (var card in trickStack.Peek().cards) { card.isFixed = false; card.transform.position = hiddenCardPosition; }
        }
        UIManager.instance.DeactivateRenderCards();
        UIManager.instance.DeactivateBirdWishNotice();

        UIManager.instance.RenderPlayerInfo();

        UIManager.instance.ActivateRoundResult(TeamScore);
        UIManager.instance.Wait(roundResultDuration);
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
    /// 트릭이 끝나면 이 플래그를 true로.
    /// </summary>
    public bool trickFinishFlag = false;
    
    IEnumerator StartTrickCoroutine()
    {
        //시작 플레이어 찾고
        //플레이어가 낼 족보 결정하고
        //모두 패스일때까지 카드 내기 반복
        isTrickEnd = false;

        ResetTrickSetting();
        while(trickFinishFlag==false)
        {
            currentPlayer = players[startPlayerIdx % numberOfPlayers];
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
        int idx = 0;
        foreach (string cardName in Enum.GetNames(typeof(CardType)))
        {
            if (typeNumber < numberOfGeneralCardType)
            {
                for (int i = 1; i <= numberOfCardsGeneral; ++i)
                {
                    var nowCard = (
                                    Instantiate(Resources.Load(prefabPath + GetCardName(cardName, i)),hiddenCardPosition,
                                                initialCardRotation,cardsParent.transform) as GameObject
                                   ).GetComponent<Card>();

                    
                    nowCard.cardName = GetCardName(cardName, i); nowCard.type = (CardType)Enum.Parse(typeof(CardType),cardName); 
                    nowCard.value = generalCardsValue[i]; nowCard.id = idNumber; nowCard.score = generalCardsScore[i];
                    cards.Add(nowCard);
                    idNumber++;
                }
                typeNumber++;
            }
            else
            {
                var nowCard = (
                                Instantiate(Resources.Load(prefabPath + cardName), hiddenCardPosition,
                                           initialCardRotation,cardsParent.transform) as GameObject
                              ).GetComponent<Card>();
                
                nowCard.cardName = cardName; nowCard.type = (CardType)Enum.Parse(typeof(CardType), cardName);
                nowCard.value = specialCardsValue[idx]; nowCard.id = idNumber; nowCard.score = specialCardsScore[idx];
                cards.Add(nowCard);
                if (nowCard.type == CardType.Phoenix) phoenix = nowCard;
                if (nowCard.type == CardType.Bird) bird = nowCard;
                idNumber++; idx++; typeNumber++;
            }
        }
    }

    void InitializePlayers()
    {
        players = new GamePlayer[numberOfPlayers];

        for(int idx = 0; idx<numberOfPlayers; ++idx)
        {
            players[idx] = GameObject.Find(playerObjectNames[idx]).GetComponent<GamePlayer>();
            players[idx].playerNumber = idx;
            players[idx].playerName   = playerNames[idx];
        }
    }

    void InitializeVariables()
    {
        cardsParent = GameObject.Find(cardsParentObjectName);
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0) && isSelectionEnabled)
        {
            GameObject hitObject = GetHitObject( Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (hitObject != null) hitObject.GetComponent<SelectionHandler>().ToggleSelection();
        }
    }

    private void FindBirdIfFirst()
    {
        if(isFirstRound)
        {
            isFirstRound = false;
            for(int idx = 0; idx<numberOfPlayers; ++idx)
            {
                if(players[idx].cards.Any(x=>x.type==CardType.Bird)==true) //새로 바꿀 수 없을까
                {
                    startPlayerIdx = idx;
                    return;
                }
            }
        }
    }

    public bool isTrickValid(Trick trick)
    {
        if (trick.trickType == TrickType.IsNotTrick||trick.trickType == TrickType.Blank)
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
            if (trick.trickType == TrickType.StraightFlushBomb)
            {
                if (topTrick.trickType == TrickType.StraightFlushBomb)
                {
                    if (trick.trickLength > topTrick.trickLength) return true;
                    else if (trick.trickLength == topTrick.trickLength && trick.trickValue > topTrick.trickValue) return true;
                    else return false;
                }
                else return true;
            }
            else if (trick.trickType == TrickType.FourCardBomb)
            {
                if (topTrick.trickType == TrickType.StraightFlushBomb) return false;
                else if (topTrick.trickType == TrickType.FourCardBomb)
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
        for (int i = 0; i < numberOfSlots; ++i)
        {
            var nowPlayer = players[(idx + i) % numberOfPlayers];
            if (nowPlayer.isTrickPassed == false) return false;
        }
        return true;
    }

    public bool IsBombAllPassed()
    {
        int idx = currentPlayer.playerNumber;
        for(int i=0; i<numberOfPlayers; ++i)
        {
            var nowPlayer = players[(idx + i) % numberOfPlayers];
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
        birdWishValue = 0;
        foreach (var player in players) player.ResetPerRound();
    }
    
    public bool IsDragonOnTop()
    {
        return trickStack.Peek().cards.Any(x => x.type == CardType.Dragon) == true;
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
        phoenix.value = specialCardsValue[2];
    }

    public Trick FindValidBirdWishFulfillTrick(List<Card> cardList)
    {
        //player는 bird wish에 해당하는 카드를 가지고 있음이 보장되어있음.
        //이 메소드는 bird wish에 해당하는 카드를 포함해 유효한 트릭을 만들어서 낼 수 있으면
        //해당 트릭을 만들어준다.
        if (isFirstTrick) //스택이 비어있다면? 즉 지금이 처음 내는 트릭이라면
        {
            return FindValidSingle(cardList, trickStack.Peek().trickValue, birdWishValue); //참새의 소원에 맞는 싱글을 낸다.
        }
        else //스택이 비어있지 않다면?
        {
            //모든 스택의 트릭 타입에 대한 경우의 수와 그에 따른 유효성을 검증한다.
            //스택의 트릭 타입은 Single,Pair,ConsecutivePair,Triple,FullHouse, Straight,FourCardBomb,StraightFlushBomb 이 가능하다.
            var topTrick = trickStack.Peek();
            Trick retTrick = null;
            if(topTrick.trickType==TrickType.Single&&(topTrick.trickValue/2)<birdWishValue)// 직전 트릭이 싱글이고 직전 트릭의 값이 참새의 소원보다 낮을 경우 낼 수 있다.
            {
                return FindValidSingle(cardList, trickStack.Peek().trickValue, birdWishValue); //참새의 소원에 맞는 싱글을 낸다.
            }
            //위 조건을 만족하지 못한다면 싱글은 낼 수 없음.
            if (topTrick.trickType == TrickType.Pair && (retTrick = FindValidPair(cardList,topTrick.trickValue, birdWishValue)) != null)
                return retTrick;
            //위 조건을 만족하지 못한다면 더블은 낼 수 없음.
            if (topTrick.trickType == TrickType.Triple && (retTrick = FindValidTriple(cardList, topTrick.trickValue, birdWishValue)) != null)
                return retTrick;
            //위 조건을 만족하지 못한다면 트리플은 낼 수 없음. consecutivePair, FullHouse, Straight, FourCardBomb, StraightFlushBomb 남음.
            if (topTrick.trickType == TrickType.FullHouse && (retTrick = FindValidFullHouse(cardList, topTrick.trickValue, birdWishValue)) != null)
                return retTrick;
            //위 조건을 만족하지 못한다면 풀하우스는 낼 수 없음. consecutivePair, Straight, FourCardBomb, StraightFlushBomb 남음.
            if (topTrick.trickType == TrickType.Straight && (retTrick = FindValidStraight(cardList, topTrick.trickValue, birdWishValue, topTrick.trickLength)) != null)
                return retTrick;
            //위 조건을 만족하지 못한다면 스트레이트는 낼 수 없음. consecutivePair, FourCardBomb, StraightFlushBomb 남음.
            if (topTrick.trickType == TrickType.ConsecutivePair && (retTrick = FindValidConsecutivePair(cardList, topTrick.trickValue, birdWishValue, topTrick.trickLength)) != null)
                return retTrick;
            //위 조건을 만족하지 못한다면 연속페어는 낼 수 없음. 이제 FourCardBomb, StraightFlushBomb 을 낼수 있는지 검사.
            if ((retTrick = FindValidFourCardBomb(topTrick, cardList, birdWishValue)) != null)
                return retTrick;
            if ((retTrick = FindValidStraightFlushBomb(topTrick, cardList, birdWishValue)) != null)
                return retTrick;

            return retTrick;
        }
    }

    public Trick FindValidSingle(List<Card>cardList, int topValue, int mustContainValue)
    {
        cardList = cardList.ToList();
        if (cardList.Any(x => x.type == CardType.Phoenix)) RestorePhoenixValue();
        while(cardList.Count>0)
        {
            List<Card> nowList = cardList.Take(1).ToList();
            var nowTrick = MakeTrick(nowList);
            if (nowTrick.trickType == TrickType.Single && nowTrick.trickValue > topValue && nowTrick.trickValue == mustContainValue*2)
                return nowTrick;
            else cardList = cardList.Skip(1).ToList();
        }
        return null;
    }

    public Trick FindValidPair(List<Card>cardList, int topValue, int mustContainValue)
    {
        cardList = cardList.ToList();
        if (topValue < mustContainValue) //포함해야할 카드 숫자가 topValue 보다 크다면,
        {
            var nowList = cardList.ToList();
            while (nowList.Count >= 2)
            {
                var nowPair = nowList.Take(2).ToList();
                if (MakeTrick(nowPair).trickType==TrickType.Pair && nowPair[0].value == mustContainValue) return MakeTrick(nowPair);
                else nowList = nowList.Skip(1).ToList();
            }

            if (cardList.Any(x => x.type == CardType.Phoenix))
            {
                phoenix.value = mustContainValue;
                SortCard(ref cardList);
                nowList = cardList.ToList();
                while (nowList.Count >= 2)
                {
                    var nowPair = nowList.Take(2).ToList();
                    if (MakeTrick(nowPair).trickType == TrickType.Pair && nowPair.Any(x => x.value == mustContainValue)) return MakeTrick(nowPair);
                    else nowList = nowList.Skip(1).ToList();
                }
                return null;
            }
            else return null;
        }
        else return null;
    }

    public Trick FindValidTriple(List<Card>cardList, int topValue, int mustContainValue)
    {
        cardList = cardList.ToList();
        if (topValue < mustContainValue)
        {
            var nowList = cardList.ToList();
            while(nowList.Count>=3)
            {
                var nowTriple = nowList.Take(3).ToList();
                if (MakeTrick(nowTriple).trickType == TrickType.Triple && nowTriple.Any(x => x.value == mustContainValue)) return MakeTrick(nowTriple);
                else nowList = nowList.Skip(1).ToList();
            }
            if (cardList.Any(x => x.type == CardType.Phoenix))
            {
                phoenix.value = mustContainValue;
                SortCard(ref cardList);
                nowList = cardList.ToList();
                while (nowList.Count >= 3)
                {
                    var nowTriple = nowList.Take(3).ToList();
                    if (MakeTrick(nowTriple).trickType == TrickType.Triple && nowTriple.Any(x => x.value == mustContainValue)) return MakeTrick(nowTriple);
                    else nowList = nowList.Skip(1).ToList();
                }
                return null;
            }
            else return null;
        }
        else return null;
    }

    public Trick FindValidFullHouse(List<Card> cardList, int topValue, int mustContainValue)
    {
        cardList = cardList.ToList();
        //아이디어: FindValidTriple, FindValidPair 사용해서 찾는다.
        int tripleValue = topValue + 1;
        while(tripleValue<=14) //유효한 값부터 A까지 트리플을 만들어본다.
        {
            var nowList = cardList.ToList();
            var validTriple = FindValidTriple(nowList, topValue, tripleValue);

            if(validTriple!=null) //유효한 트리플을 만들 수 있다면
            {
                foreach (var card in validTriple.cards) nowList.Remove(card); //유효한 트리플 카드들을 빼보고
                for (int i = 2; i <= 14; ++i)
                {
                    var validPair = FindValidPair(nowList, 0, i); //모든 숫자에 대해 페어를 찾아본다.
                    if (validPair != null) //페어도 찾을 수 있다면
                    {
                        if (validTriple.cards.Any(x => x.value == mustContainValue) ||
                            validPair.cards.Any(x => x.value == mustContainValue))//트리플이나 페어 중 mustContainValue를 포함하고 있다면
                        {
                            validPair.cards.AddRange(validTriple.cards);
                            return MakeTrick(validPair.cards);
                        }
                    }
                }
            }            
            ++tripleValue;
        }
        return null;
    }

    public Trick FindValidStraight(List<Card> cardList, int topValue, int mustContainValue, int trickLength)
    {
        cardList = cardList.ToList();
        var properList = cardList.Distinct(new CardValueComparer()).OrderBy(x=>x.value).ThenBy(x=>x.type).ToList(); //중복 제거.
        while(properList.Count>0)
        {
            var nowList = properList.Take(trickLength).ToList();
            if (nowList.Count != trickLength) break;
            var nowTrick = MakeTrick(nowList);
            if (nowTrick.trickType == TrickType.Straight && nowTrick.trickValue > topValue && nowTrick.cards.Any(x => x.value == mustContainValue)) return nowTrick;
            else properList = properList.Skip(1).ToList();
        }
        if (cardList.Any(x => x.type == CardType.Phoenix))
        {
            for(int idx = 2; idx<=14; ++idx)
            {
                phoenix.value = idx;
                properList = cardList.Distinct(new CardValueComparer()).OrderBy(x => x.value).ThenBy(x=>x.type).ToList();
                while(properList.Count>0)
                {
                    var nowList = properList.Take(trickLength).ToList();
                    if (nowList.Count != trickLength) break;
                    var nowTrick = MakeTrick(nowList);
                    if (nowTrick.trickType == TrickType.Straight && nowTrick.trickValue > topValue && nowTrick.cards.Any(x => x.value == mustContainValue)) return nowTrick;
                    else properList = properList.Skip(1).ToList();
                }
            }
            return null;
        }
        else return null;
    }

    public Trick FindValidConsecutivePair(List<Card>cardList, int topValue, int mustContainValue, int trickLength)
    {
        cardList = cardList.ToList();
        var doubleList = (from n in cardList where cardList.Count(y => y.value == n.value) == 2 select n).ToList(); //같은 값이 2개인 카드들 리스트
        var tripleList = (from n in cardList where cardList.Count(y => y.value == n.value) == 3 select n).ToList(); //같은 값이 3개인 카드들 리스트
        var quadList   = (from n in cardList where cardList.Count(y => y.value == n.value) == 4 select n).ToList(); //같은 값이 4개인 카드들 리스트
        tripleList     = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3개 중 2개씩을 pick
        quadList       = quadList.Where((z, index) => index % 2 == 0).ToList(); //4개 중 2개씩을 pick
        var properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat 하고 정렬
        SortCard(ref properList);

        while(properList.Count>0)
        {
            var nowList = properList.Take(trickLength).ToList(); //length 만큼 자름.
            if (nowList.Count != trickLength) break;
            var nowTrick = MakeTrick(nowList);
            if (nowTrick.trickType == TrickType.ConsecutivePair && nowTrick.trickValue > topValue && nowTrick.cards.Any(x => x.value == mustContainValue)) return nowTrick;
            else properList = properList.Skip(2).ToList();
        }

        if (cardList.Any(x => x.type == CardType.Phoenix))
        {
            for (int i = 2; i <= 14; ++i)
            {
                phoenix.value = i;
                doubleList = (from n in cardList where cardList.Count(y => y.value == n.value) == 2 select n).ToList(); //같은 값이 2개인 카드들 리스트
                tripleList = (from n in cardList where cardList.Count(y => y.value == n.value) == 3 select n).ToList(); //같은 값이 3개인 카드들 리스트
                quadList = (from n in cardList where cardList.Count(y => y.value == n.value) == 4 select n).ToList(); //같은 값이 4개인 카드들 리스트
                tripleList = tripleList.Where((y, index) => index % 3 != 0).ToList(); //3개 중 2개씩을 pick
                quadList = quadList.Where((z, index) => index % 2 == 0).ToList(); //4개 중 2개씩을 pick
                properList = doubleList.Concat(tripleList).Concat(quadList).ToList(); // concat 하고 정렬
                SortCard(ref properList);

                while (properList.Count > 0)
                {
                    var nowList = properList.Take(trickLength).ToList(); //length 만큼 자름.
                    if (nowList.Count != trickLength) break;
                    var nowTrick = MakeTrick(nowList);
                    if (nowTrick.trickType == TrickType.ConsecutivePair && nowTrick.trickValue > topValue && nowTrick.cards.Any(x => x.value == mustContainValue)) return nowTrick;
                    else properList = properList.Skip(2).ToList();
                }
            }
            return null;
        }
        else return null;
    }

    public Trick FindValidFourCardBomb(Trick topTrick, List<Card> cardList, int mustContainValue)
    {
        cardList = cardList.ToList();
        RestorePhoenixValue();
        var fourCardList = cardList.Where(x=>x.value==mustContainValue).ToList(); //mustContainValue로 포카드를 만들 수 있는가?
        if (fourCardList.Count != 4) return null; //포카드를 만들 수 없다면 리턴
        //포카드를 만들 수 있다면
        if (topTrick.trickType == TrickType.StraightFlushBomb) return null; // 탑이 스플 폭탄이라면 리턴
        if (topTrick.trickType == TrickType.FourCardBomb)
        {
            if (mustContainValue > topTrick.trickValue) return MakeTrick(fourCardList); //탑이 포카드이고 엎을 수 있다면 유효. 트릭 만들어서 리턴.
            else return null;
        }
        else return MakeTrick(fourCardList); //탑이 폭탄이 아니라면 모두 엎을 수 있다. 트릭 만들어서 리턴.
    }

    public Trick FindValidStraightFlushBomb(Trick topTrick, List<Card> cardList, int mustContainValue)
    {
        cardList = cardList.ToList();
        //아이디어: 카드 유형별로 분류해서 정렬, 스트레이트이고 mustContainValue 포함하고 현재 트릭을 이길 수 있는지 확인.
        var beanCardList   = cardList.Where(x => x.type == CardType.Bean).ToList();
        var flowerCardList = cardList.Where(x => x.type == CardType.Flower).ToList();
        var shuCardList    = cardList.Where(x => x.type == CardType.Shu).ToList();
        var moonCardList   = cardList.Where(x => x.type == CardType.Moon).ToList();

        SortCard(ref beanCardList);
        SortCard(ref flowerCardList);
        SortCard(ref shuCardList);
        SortCard(ref moonCardList);

        if (topTrick.trickType == TrickType.StraightFlushBomb)
        {
            while (beanCardList.Count > 0)
            {
                var nowList = beanCardList.Take(topTrick.trickLength).ToList();
                if (nowList.Count != topTrick.trickLength) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue) && nowTrick.trickValue > topTrick.trickValue)
                    return nowTrick;
                else beanCardList = beanCardList.Skip(1).ToList();
            }

            while (flowerCardList.Count > 0)
            {
                var nowList = flowerCardList.Take(topTrick.trickLength).ToList();
                if (nowList.Count != topTrick.trickLength) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue) && nowTrick.trickValue > topTrick.trickValue)
                    return nowTrick;
                else flowerCardList = flowerCardList.Skip(1).ToList();
            }

            while (shuCardList.Count > 0)
            {
                var nowList = shuCardList.Take(topTrick.trickLength).ToList();
                if (nowList.Count != topTrick.trickLength) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue) && nowTrick.trickValue > topTrick.trickValue)
                    return nowTrick;
                else shuCardList = shuCardList.Skip(1).ToList();
            }

            while (moonCardList.Count > 0)
            {
                var nowList = moonCardList.Take(topTrick.trickLength).ToList();
                if (nowList.Count != topTrick.trickLength) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue) && nowTrick.trickValue > topTrick.trickValue)
                    return nowTrick;
                else moonCardList = moonCardList.Skip(1).ToList();
            }
            return null;
        }
        else
        {
            while (beanCardList.Count > 0)
            {
                var nowList = beanCardList.Take(5).ToList();
                if (nowList.Count != 5) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue))
                    return nowTrick;
                else beanCardList = beanCardList.Skip(1).ToList();
            }

            while (flowerCardList.Count > 0)
            {
                var nowList = flowerCardList.Take(5).ToList();
                if (nowList.Count != 5) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue))
                    return nowTrick;
                else flowerCardList = flowerCardList.Skip(1).ToList();
            }

            while (shuCardList.Count > 0)
            {
                var nowList = shuCardList.Take(5).ToList();
                if (nowList.Count != 5) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue))
                    return nowTrick;
                else shuCardList = shuCardList.Skip(1).ToList();
            }

            while (moonCardList.Count > 0)
            {
                var nowList = moonCardList.Take(5).ToList();
                if (nowList.Count != 5) break;
                var nowTrick = MakeTrick(nowList);
                if (nowTrick.trickType == TrickType.StraightFlushBomb && nowTrick.cards.Any(x => x.value == mustContainValue))
                    return nowTrick;
                else moonCardList = moonCardList.Skip(1).ToList();
            }
            return null;
        }
    }

    public bool IsTrickValidAndFulfillBirdWish(Trick nowTrick)
    {
        if (isTrickValid(nowTrick) && nowTrick.cards.Any(x => x.value == birdWishValue && x.type != CardType.Phoenix)) return true;
        else return false;
    }
}
