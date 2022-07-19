using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class GamePlayer : MonoBehaviour
{
    public List<Card> cards             = new List<Card>();
    public List<Card> selectCardList    = new List<Card>();
    public Util.PlayerReceiveCardSlot[] slot = new Util.PlayerReceiveCardSlot[Util.numberOfSlots];

    public string playerName;
    public int    playerNumber;
    public int    roundScore;
    public int    totalScore;

    public bool chooseFlag          = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag      = false;
    public bool smallTichuFlag      = false;
    public bool isTrickPassed       = false;
    public bool isBombPassed        = false;
    public bool isTrickSelected     = false;
    public int  ranking;

    public bool canDeclareSmallTichu = true;

    public bool isFinished = false;

    public string previousTrick = null;

    public bool hasBomb = false;

    public Util.PlayerType playerType;

    public TichuAgent agent = null;

    private void Awake()
    {
        agent = GetComponent<TichuAgent>();
        if (agent == null) playerType = Util.PlayerType.Player;
        else
        {
            if (GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.HeuristicOnly) playerType = Util.PlayerType.Heuristic;
            else                                                                               playerType = Util.PlayerType.Inference;
        }
    }

    /// <summary>
    /// 트릭이 끝날때마다 리셋할 변수들.
    /// </summary>
    public void ResetPerTrick()
    {
        
        isBombPassed  = false;
        isTrickPassed = false;
        isTrickSelected = false;
        previousTrick = null;
        isDogTrick = false;
        dragonChooseFlag = false;
        getTrickScoreFlag = false;
        dragonChoosePlayerIdx = -1;
    }
    /// <summary>
    /// 라운드가 끝날때마다(1,2,3,4등) 리셋할 변수들.
    /// </summary>
    public void ResetPerRound()
    {
        hasBomb = false;
        roundScore = 0;
        chooseFlag = false;
        coroutineFinishFlag = false;
        largeTichuFlag = false;
        smallTichuFlag = false;
        ranking = -1;
        canDeclareSmallTichu = true;
        isFinished = false;
        cards.Clear();
        selectCardList.Clear();
        ResetPerTrick();
    }
      
    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public void AddCards(List<Card> cardList)
    {
        cards.AddRange(cardList);
    }
    public void AddCardToSlot(Card card, GamePlayer cardGiver)
    {
        slot[Util.GetCardGiverIdx(cardGiver, this)].player = cardGiver;
        slot[Util.GetCardGiverIdx(cardGiver, this)].card   = card;
    }
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }

    public void AddSelection(Card card)
    {
        selectCardList.Add(card);
        cards.Remove(card);
        Util.SortCard(ref selectCardList);
    }

    public void RemoveSelection(Card card)
    {
        selectCardList.Remove(card);
        cards.Add(card);
        Util.SortCard(ref cards);
    }

    public void DisableSelection()
    {
        cards.AddRange(selectCardList);
        ClearSelection();
    }

    public void ClearSelection()
    {
        foreach (var selectedCard in selectCardList) selectedCard.ToggleBase();
        selectCardList.Clear();
        GameManager.instance.RestorePhoenixValue();
        Util.SortCard(ref cards);
    }

    public bool CalculateHandRunOut()
    {
        if (cards.Count == 0) { ranking = GameManager.instance.CountFinishedPlayer() + 1; isFinished = true; }
        return isFinished;
        //스몰 티츄, 라지 티츄 관련 로직이 있으면 좋을 듯.
    }

    public void FindNextPlayer(Util.Trick nowTrick)
    {
        int startIdx = GameManager.instance.startPlayerIdx + 1;
        if (nowTrick?.trickType == Util.TrickType.Dog) 
        {
            startIdx++;
            for (int i = 0; i < Util.numberOfPlayers; ++i)
            {
                if (GameManager.instance.players[startIdx % Util.numberOfPlayers].isFinished == false) break;
                ++startIdx;
            }
        }
        GameManager.instance.startPlayerIdx = startIdx;
    }

    public void CalculateIsRoundEnd()
    {
        if(isFinished == true&&GameManager.instance.players[(playerNumber+2)%Util.numberOfPlayers].isFinished == true)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // 원투로 끝남. 원투 관련 셋업.
        }
        else if(GameManager.instance.CountFinishedPlayer()==3)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // 1,2,3등이 나눠짐.
        }
    }

    private bool getTrickScoreFlag;
    private bool dragonChooseFlag;
    private int dragonChoosePlayerIdx;
    public IEnumerator GetTrickScore()
    {
        getTrickScoreFlag = false;
        dragonChooseFlag = false;
        if (GameManager.instance.IsDragonOnTop())
        {
            // 용으로 딴 트릭은 줄 사람을 정하는 팝업 띄우고,
            if (playerType == Util.PlayerType.Player)
            {
                UIManager.instance.ActivateDragonSelection(DragonChooseNextOpponentCall, DragonChoosePreviousOpponentCall);
                yield return new WaitUntil(() => dragonChooseFlag || UIManager.instance.IsTimeOut());
                UIManager.instance.DeactivateDragonSelection();
                if (dragonChooseFlag == false) RandomDragonChooseCall();
            }
            else
            {
                GameManager.instance.currentPhase = Util.PhaseType.DragonSelectionPhase;
                agent.RequestDecision();
                yield return new WaitUntil(() => agent.isActionEnd);
                agent.isActionEnd = false;
            }
            var trickTaker = GameManager.instance.players[dragonChoosePlayerIdx % Util.numberOfPlayers];
            foreach (var trick in GameManager.instance.trickStack)
            {
                int score = 0;
                foreach (var card in trick.cards)
                {
                    score += card.score;
                    card.transform.position = Util.hiddenCardPosition;
                }
                trickTaker.roundScore += score;
            }

            // 선택 후 그 사람에 대해 Give.
        }
        else
        {
            foreach (var trick in GameManager.instance.trickStack)
            {
                int score = 0;
                foreach (var card in trick.cards)
                {
                    score += card.score;
                    card.transform.position = Util.hiddenCardPosition;
                }
                roundScore += score;
            }
        }
        GameManager.instance.trickStack.Clear();
        getTrickScoreFlag = true;
    }

    private bool isDogTrick;
    public void ProgressIfDog(Util.Trick nowTrick)
    {
        if (nowTrick.trickType == Util.TrickType.Dog)
        {
            isDogTrick = true;
        }
        else isDogTrick = false;
    }

    private bool isBirdTrick;
    public void ProgressIfBird(Util.Trick nowTrick)
    {
        if (nowTrick.cards.Contains(GameManager.instance.bird)) isBirdTrick = true;
        else isBirdTrick = false;
    }

    public void DeclareLargeTichuCall()
    {
        chooseFlag = true;
        canDeclareSmallTichu = false;
        largeTichuFlag = true;
    }
    public void SkipLargeTichuCall()
    {
        chooseFlag = true;
        largeTichuFlag = false;
    }
    public void DeclareSmallTichuCall()
    {
        smallTichuFlag = true;
        canDeclareSmallTichu = false;
        UIManager.instance.DeactivateAlertPopup();
        UIManager.instance.RenderPlayerInfo();
    }
    public void ExchangeCardCall()
    {
        if (UIManager.instance.IsAllSlotSelected())
        {
            GameManager.instance.currentCard?.ToggleSelection();
            GameManager.instance.currentSlot?.ToggleSelection();
            chooseFlag = true;
        }
        else
        {
            UIManager.instance.Massage(Util.slotSelectErrorMsg);
            return;
        }
    }

    public void RandomExchangeCardCall()
    {
        for (int i = 0; i < Util.numberOfSlots; ++i)
        {
            if (UIManager.instance.exchangeCard.slots[i].slot.card == null)
            {
                cards[Random.Range(0, cards.Count)].ToggleSelection();
                UIManager.instance.exchangeCard.slots[i].slot.ToggleSelection();
            }
        }
        ExchangeCardCall();
    }

    public void ReceiveCardCall()
    {
        for (int idx = 0; idx < slot.Length; ++idx)// var cardSlot in slot)
        {
            AddCard(slot[idx].card);
            slot[idx].card.isFixed = false;
            slot[idx].card = null;
        }
        Util.SortCard(ref cards);
        hasBomb = GameManager.instance.IsBombExist(this);
        chooseFlag = true;
        //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        //버퍼에 있는 카드를 AddCard() 하고, isFixed 풀고, 정렬하고, 렌더.
    }

    public void SelectTrickCall()
    {
        Util.Trick nowTrick = Util.MakeTrick(selectCardList);
        if (Util.IsPlayerHaveToFulfillBirdWish(this)!=null)
        {
            if(GameManager.instance.IsTrickValidAndFulfillBirdWish(nowTrick))
            {
                previousTrick = Util.GetTrickInfo(nowTrick);
                UIManager.instance.RenderTrickCard(selectCardList);
                nowTrick.playerIdx = playerNumber;
                GameManager.instance.trickStack.Push(nowTrick); // 수정 필요.
                GameManager.instance.AddCardMarking(selectCardList);
                ClearSelection();
                canDeclareSmallTichu = false;
                CalculateHandRunOut();
                ProgressIfDog(nowTrick);
                ProgressIfBird(nowTrick);
                CalculateIsRoundEnd();
                isTrickPassed = false;
                isTrickSelected = true;
                //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
                UIManager.instance.DeactivateBirdWishNotice(); // 이 부분 추가.

                hasBomb = GameManager.instance.IsBombExist(this);
                
                chooseFlag = true;
            }
            else
            {
                UIManager.instance.Massage(Util.fulfillBirdWishErrorMsg);
                return;
            }
        }
        else
        {
            if (GameManager.instance.isTrickValid(nowTrick))
            {
                previousTrick = Util.GetTrickInfo(nowTrick);
                UIManager.instance.RenderTrickCard(selectCardList);
                nowTrick.playerIdx = playerNumber;
                GameManager.instance.AddCardMarking(selectCardList);
                GameManager.instance.trickStack.Push(nowTrick); // 수정 필요.
                ClearSelection();
                canDeclareSmallTichu = false;

                CalculateHandRunOut();
                ProgressIfDog(nowTrick);
                ProgressIfBird(nowTrick);
                CalculateIsRoundEnd();
                isTrickPassed = false;
                isTrickSelected = true;

                hasBomb = GameManager.instance.IsBombExist(this);
                //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
                chooseFlag = true;
            }
            else
            {
                UIManager.instance.Massage(Util.trickSelectErrorMsg);
                return;
            }
        }
    }

    public void SelectBombCall()
    {
        Util.Trick nowTrick = Util.MakeTrick(selectCardList);

        if(nowTrick.trickType==Util.TrickType.FourCardBomb || nowTrick.trickType==Util.TrickType.StraightFlushBomb)
        {
            isBombPassed = false;
            SelectTrickCall();
        }
        else
        {
            UIManager.instance.Massage(Util.bombSelectErrorMsg);
            return;
        }
    }

    public void PassTrickCall()
    {
        DisableSelection();
        if (isFinished == false) previousTrick = Util.trickPassInfo;
        isTrickPassed = true;
        isTrickSelected = false;
        chooseFlag = true;
    }

    public void PassBombCall()
    {
        DisableSelection();
        isBombPassed = true;
        chooseFlag = true;
    }

    public void PassOrPickRandomTrickCall()
    {
        //참새의 소원을 만족할 수 있는데 타임아웃이라면 만족하는 트릭 아무거나 낸다.
        Util.Trick birdWishFulfillTrick = null;
        if ((birdWishFulfillTrick = Util.IsPlayerHaveToFulfillBirdWish(this)) != null)
        {
            DisableSelection();
            foreach (var card in birdWishFulfillTrick.cards) card.ToggleSelection();
            SelectTrickCall();
        }
        else
        {
            if (GameManager.instance.isFirstTrick)
            {
                DisableSelection();
                cards[Random.Range(0, cards.Count)].ToggleSelection();
                SelectTrickCall();
            }
            else
            {
                PassTrickCall();
            }
        }
    }

    public void DragonChooseNextOpponentCall()
    {
        dragonChoosePlayerIdx = playerNumber + 1;
        dragonChooseFlag = true;
    }

    public void DragonChoosePreviousOpponentCall()
    {
        dragonChoosePlayerIdx = playerNumber + 3;
        dragonChooseFlag = true;
    }

    public void RandomDragonChooseCall()
    {
        var rnd = new UnityAction[] { DragonChooseNextOpponentCall, DragonChoosePreviousOpponentCall };
        rnd[Random.Range(0, 2)]();
    }

    public void BirdWishChooseCall()
    {
        chooseFlag = true;
    }



    public void ChooseLargeTichu()
    {
        Util.SortCard(ref cards);
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public void ChooseSmallTichu() //강화학습 전용 함수
    {
        StartCoroutine(ChooseSmallTichuCoroutine());
    }

    public void ExchangeCards()
    {
        Util.SortCard(ref cards);
        StartCoroutine(ExchangeCardsCoroutine());
    }

    public void ReceiveCard()
    {
        StartCoroutine(ReceiveCardCoroutine());
    }

    public void SelectTrick()
    {
        StartCoroutine(SelectTrickCoroutine());
    }



    public IEnumerator ChooseLargeTichuCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        
        coroutineFinishFlag = false;


        if(playerType == Util.PlayerType.Inference)
        {
            GameManager.instance.currentPhase = Util.PhaseType.LargeTichuSelectionPhase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
        }
        else
        {
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInLargeTichuPhase, cards);
            chooseFlag = false;
            UIManager.instance.ActivateLargeTichu(DeclareLargeTichuCall, SkipLargeTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) SkipLargeTichuCall();
            UIManager.instance.DeactivateLargeTichu();
        }

        coroutineFinishFlag = true;
    }

    public IEnumerator ChooseSmallTichuCoroutine() //강화학습 전용 코루틴
    {
        UIManager.instance.RenderPlayerInfo();

        coroutineFinishFlag = false;

        if (playerType == Util.PlayerType.Inference && canDeclareSmallTichu)
        {
            GameManager.instance.currentPhase = Util.PhaseType.SmallTichuSelectionPhase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
        }
        coroutineFinishFlag = true;
    }

    public IEnumerator ExchangeCardsCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        
        coroutineFinishFlag = false;

        if (playerType == Util.PlayerType.Inference)
        {
            GameManager.instance.currentPhase = Util.PhaseType.ExchangeSelection1Phase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
            GameManager.instance.currentPhase = Util.PhaseType.ExchangeSelection2Phase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
            GameManager.instance.currentPhase = Util.PhaseType.ExchangeSelection3Phase;
            agent.RequestDecision();
            yield return new WaitUntil(() => agent.isActionEnd);
            agent.isActionEnd = false;
        }
        else
        {
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
            chooseFlag = false;
            UIManager.instance.ActivateExchangeCardsPopup(ExchangeCardCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) RandomExchangeCardCall();
            UIManager.instance.DeactivateExchangeCardsPopup();
        }
        coroutineFinishFlag = true;
    }

    public IEnumerator ReceiveCardCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        
        chooseFlag = false;
        
        coroutineFinishFlag = false;

        if (playerType == Util.PlayerType.Player)
        {
            UIManager.instance.ActivateReceiveCardPopup(ReceiveCardCall, DeclareSmallTichuCall);
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) ReceiveCardCall();
            UIManager.instance.DeactivateReceiveCardPopup();
        }
        else
        {
            ReceiveCardCall();
        }
        coroutineFinishFlag = true;
    }

    public IEnumerator SelectTrickCoroutine()
    {
        GameManager.instance.isSelectionEnabled = true;

        //폭탄은 나부터.
        chooseFlag = false;

        coroutineFinishFlag = false;

        if(GameManager.instance.IsBombPhase()) //폭탄 페이즈
        {
            if (GameManager.instance.IsBombAllPassed()) //폭탄 확인은 다 끝났다면
            {//
                GameManager.instance.DeactivateBombPhase(); //폭탄 페이즈를 해제하고, 폭탄 패스를 모두 초기화.
                FindNextPlayer(null); //다음 플레이어를 찾는다.
            }
            else //폭탄 확인이 다 끝나지 않았다면
            {
                if (isFinished == true)
                {
                    PassBombCall();
                    FindNextPlayer(null);
                }
                else
                {
                    UIManager.instance.RenderPlayerInfo();

                    if (playerType == Util.PlayerType.Inference)
                    {
                        if (hasBomb)
                        {
                            GameManager.instance.currentPhase = Util.PhaseType.BombSelectionPhase;
                            agent.RequestAction(); //여기서 SelectBombCall() 같은 메소드를 호출해야한다.
                            yield return new WaitUntil(() => agent.isActionEnd);
                            agent.isActionEnd = false;
                        }
                        else
                        {
                            PassBombCall();
                        }
                    }
                    else
                    {
                        UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
                        UIManager.instance.ActivateBombSelection(SelectBombCall, PassBombCall, DeclareSmallTichuCall);
                        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                        if (chooseFlag == false) PassBombCall();
                        UIManager.instance.DeactivateBombSelection();
                    }
                    FindNextPlayer(null);
                }
            }
        }
        else //폭탄 페이즈가 아니다. 카드를 내는 타이밍.
        {
            if (GameManager.instance.IsTrickAllPassed()) //모두 패스를 했다면 끝. 렌더링 후 다시 내 차례.
            {
                UIManager.instance.RenderPlayerInfo();
                UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

                GameManager.instance.isSelectionEnabled = false;
                StartCoroutine(GetTrickScore());
                yield return new WaitUntil(() => getTrickScoreFlag);
                GameManager.instance.trickFinishFlag = true;
                GameManager.instance.startPlayerIdx = playerNumber;
                if (isFinished == true) FindNextPlayer(null);

                if (dragonChooseFlag)
                {
                    UIManager.instance.ShowInfo(Util.GetTrickTakeInfo(GameManager.instance.players[dragonChoosePlayerIdx % Util.numberOfPlayers].playerName));
                    dragonChooseFlag = false;
                }
                else UIManager.instance.ShowInfo(Util.GetTrickTakeInfo(playerName));

                if (playerType == Util.PlayerType.Player)
                {
                    UIManager.instance.Wait(Util.trickTakeDuration);
                    yield return new WaitUntil(() => UIManager.instance.IsWaitFinished());
                }
            }
            else
            {
                if (isFinished == true)
                {
                    previousTrick = null;
                    PassTrickCall();
                    FindNextPlayer(null);
                }
                else
                {
                    GameManager.instance.currentTrickPlayerIdx = playerNumber;

                    UIManager.instance.RenderPlayerInfo();

                    if (playerType == Util.PlayerType.Inference)
                    {
                        if (GameManager.instance.isFirstTrick) GameManager.instance.currentPhase = Util.PhaseType.FirstTrickSelectionPhase;
                        else GameManager.instance.currentPhase = Util.PhaseType.TrickSelectionPhase;

                        agent.RequestAction(); //여기서 SelectTrickCall() 같은 메소드를 호출해야한다.
                        yield return new WaitUntil(() => agent.isActionEnd);
                        agent.isActionEnd = false;
                    }
                    else
                    {
                        UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
                        UIManager.instance.ActivateTrickSelection(SelectTrickCall, PassTrickCall, DeclareSmallTichuCall, SelectBombCall);
                        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                        if (chooseFlag == false) PassOrPickRandomTrickCall();
                        UIManager.instance.DeactivateTrickSelection();
                    }

                    if (isDogTrick) //개를 낸 경우
                    {
                        FindNextPlayer(GameManager.instance.trickStack.Peek());
                        GameManager.instance.isSelectionEnabled = false;
                        isDogTrick = false;
                        if (playerType == Util.PlayerType.Player) // inference 중에는 필요 없는 부분.
                        {
                            UIManager.instance.ShowInfo(GameManager.instance.players[GameManager.instance.startPlayerIdx % Util.numberOfPlayers].playerName + Util.selectDogInfo);
                            UIManager.instance.Wait(Util.selectDogDuration);
                            yield return new WaitUntil(() => UIManager.instance.IsWaitFinished());
                        }
                        StartCoroutine(GetTrickScore()); //스택 비울 용도로.
                        yield return new WaitUntil(() => getTrickScoreFlag);
                        GameManager.instance.trickFinishFlag = true;
                    }
                    else
                    {
                        if (isBirdTrick) //새를 낸 경우
                        {
                            chooseFlag = false;
                            isBirdTrick = false;

                            if (playerType == Util.PlayerType.Inference)
                            {
                                GameManager.instance.currentPhase = Util.PhaseType.BirdWishSelectionPhase;
                                agent.RequestDecision();
                                yield return new WaitUntil(() => agent.isActionEnd);
                                agent.isActionEnd = false;
                            }
                            else
                            {
                                GameManager.instance.isSelectionEnabled = false; //카드 선택을 disable 하고
                                UIManager.instance.ActivateBirdWishSelection(BirdWishChooseCall);
                                yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                                UIManager.instance.DeactivateBirdWishSelection();
                            }
                        }
                        GameManager.instance.ActivateBombPhase();
                    }
                }
            }
        }

        coroutineFinishFlag = true;
    }
}
