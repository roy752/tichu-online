using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

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
    public int  ranking;

    public bool canDeclareSmallTichu = true;

    public bool isFinished = false;

    public string previousTrick = null;

    public TichuAgent agent = null;

    private void Awake()
    {
        agent = GetComponent<TichuAgent>();
    }

    /// <summary>
    /// Ʈ���� ���������� ������ ������.
    /// </summary>
    public void ResetPerTrick()
    {
        isBombPassed  = false;
        isTrickPassed = false;
        previousTrick = null;
        isDogTrick = false;
        dragonChooseFlag = false;
        getTrickScoreFlag = false;
        dragonChoosePlayerIdx = -1;
    }
    /// <summary>
    /// ���尡 ����������(1,2,3,4��) ������ ������.
    /// </summary>
    public void ResetPerRound()
    {
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
        //���� Ƽ��, ���� Ƽ�� ���� ������ ������ ���� ��.
    }

    public void FindNextPlayer(Util.Trick nowTrick)
    {
        int startIdx = GameManager.instance.startPlayerIdx + 1;
        if (nowTrick?.trickType == Util.TrickType.Dog) 
        {
            startIdx++;
            for (int i = 0; i < Util.numberOfPlayers; ++i)
            {
                if (GameManager.instance.players[startIdx % Util.numberOfPlayers] != GameManager.instance.currentPlayer &&
                    GameManager.instance.players[startIdx % Util.numberOfPlayers].isFinished == false) break;
                ++startIdx;
            }
            GameManager.instance.trickFinishFlag = true;
        }
        GameManager.instance.startPlayerIdx = startIdx;
    }

    public void CalculateIsRoundEnd()
    {
        if(isFinished == true&&GameManager.instance.players[(playerNumber+2)%Util.numberOfPlayers].isFinished == true)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // ������ ����. ���� ���� �¾�.
        }
        else if(GameManager.instance.CountFinishedPlayer()==3)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // 1,2,3���� ������.
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
            // ������ �� Ʈ���� �� ����� ���ϴ� �˾� ����,
            UIManager.instance.ActivateDragonSelection(DragonChooseNextOpponentCall,DragonChoosePreviousOpponentCall);
            yield return new WaitUntil(() => dragonChooseFlag || UIManager.instance.IsTimeOut());
            UIManager.instance.DeactivateDragonSelection();
            if (dragonChooseFlag == false) RandomDragonChooseCall();

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

            // ���� �� �� ����� ���� Give.
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
        chooseFlag = true;
        for (int idx = 0; idx < slot.Length; ++idx)// var cardSlot in slot)
        {
            AddCard(slot[idx].card);
            slot[idx].card.isFixed = false;
            slot[idx].card = null;
        }
        Util.SortCard(ref cards);
        //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        //���ۿ� �ִ� ī�带 AddCard() �ϰ�, isFixed Ǯ��, �����ϰ�, ����.
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
                GameManager.instance.trickStack.Push(nowTrick); // ���� �ʿ�.
                ClearSelection();
                canDeclareSmallTichu = false;
                CalculateHandRunOut();
                FindNextPlayer(nowTrick);
                ProgressIfDog(nowTrick);
                ProgressIfBird(nowTrick);
                CalculateIsRoundEnd();
                isTrickPassed = false;
                //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
                UIManager.instance.DeactivateBirdWishNotice(); // �� �κ� �߰�.
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
                GameManager.instance.trickStack.Push(nowTrick); // ���� �ʿ�.
                ClearSelection();
                canDeclareSmallTichu = false;

                CalculateHandRunOut();
                FindNextPlayer(nowTrick);
                ProgressIfDog(nowTrick);
                ProgressIfBird(nowTrick);
                CalculateIsRoundEnd();
                isTrickPassed = false;
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
        FindNextPlayer(null);
        if (isFinished == false) previousTrick = Util.trickPassInfo;
        isTrickPassed = true;
        chooseFlag = true;
    }

    public void PassBombCall()
    {
        isBombPassed = true;
        PassTrickCall();

    }

    public void PassOrPickRandomTrickCall()
    {
        //������ �ҿ��� ������ �� �ִµ� Ÿ�Ӿƿ��̶�� �����ϴ� Ʈ�� �ƹ��ų� ����.
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
        UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInLargeTichuPhase, cards);
        
        chooseFlag = false;
        
        coroutineFinishFlag = false;
        
        UIManager.instance.ActivateLargeTichu(DeclareLargeTichuCall, SkipLargeTichuCall);
        yield return new WaitUntil(()=>chooseFlag == true || UIManager.instance.IsTimeOut());
        if (chooseFlag == false) SkipLargeTichuCall();
        UIManager.instance.DeactivateLargeTichu();


        coroutineFinishFlag = true;
    }

    public IEnumerator ExchangeCardsCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
        
        chooseFlag = false;
        
        coroutineFinishFlag = false;

        UIManager.instance.ActivateExchangeCardsPopup(ExchangeCardCall, DeclareSmallTichuCall);
        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
        if (chooseFlag == false) RandomExchangeCardCall();
        UIManager.instance.DeactivateExchangeCardsPopup();

        coroutineFinishFlag = true;
    }

    public IEnumerator ReceiveCardCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);
        
        chooseFlag = false;
        
        coroutineFinishFlag = false;

        UIManager.instance.ActivateReceiveCardPopup(ReceiveCardCall, DeclareSmallTichuCall);
        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
        if (chooseFlag == false) ReceiveCardCall();
        UIManager.instance.DeactivateReceiveCardPopup();

        coroutineFinishFlag = true;
    }

    public IEnumerator SelectTrickCoroutine()
    {
        GameManager.instance.isSelectionEnabled = true;

        chooseFlag = false;

        coroutineFinishFlag = false;
        if (GameManager.instance.IsBombAllPassed())
        {

            UIManager.instance.RenderPlayerInfo();
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

            GameManager.instance.isSelectionEnabled = false;
            StartCoroutine(GetTrickScore());
            yield return new WaitUntil(() => getTrickScoreFlag);
            GameManager.instance.trickFinishFlag = true;
            GameManager.instance.startPlayerIdx = playerNumber;
            if(isFinished==true) FindNextPlayer(null);

            if (dragonChooseFlag)
            {
                UIManager.instance.ShowInfo(Util.GetTrickTakeInfo(GameManager.instance.players[dragonChoosePlayerIdx % Util.numberOfPlayers].playerName));
                dragonChooseFlag = false;
            }
            else UIManager.instance.ShowInfo(Util.GetTrickTakeInfo(playerName));
            
            UIManager.instance.Wait(Util.trickTakeDuration);
            yield return new WaitUntil(()=>UIManager.instance.IsWaitFinished());
        }
        else if(GameManager.instance.IsTrickAllPassed())
        {
            if (isFinished == true)
            {
                PassBombCall();
            }
            else
            {
                UIManager.instance.RenderPlayerInfo();
                UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

                UIManager.instance.ActivateBombSelection(SelectBombCall, PassBombCall, DeclareSmallTichuCall);
                yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                if (chooseFlag == false) PassBombCall();
                UIManager.instance.DeactivateBombSelection();
            }
        }
        else
        {
            if (isFinished == true)
            {
                previousTrick = null;
                PassTrickCall();
            }
            else
            {
                UIManager.instance.RenderPlayerInfo();
                UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, cards);

                UIManager.instance.ActivateTrickSelection(SelectTrickCall, PassTrickCall, DeclareSmallTichuCall, SelectBombCall);
                yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                if (chooseFlag == false) PassOrPickRandomTrickCall();
                UIManager.instance.DeactivateTrickSelection();

                if (isDogTrick)
                {
                    GameManager.instance.isSelectionEnabled = false;
                    isDogTrick = false;
                    UIManager.instance.ShowInfo(GameManager.instance.players[GameManager.instance.startPlayerIdx % Util.numberOfPlayers].playerName + Util.selectDogInfo);
                    UIManager.instance.Wait(Util.selectDogDuration);
                    yield return new WaitUntil(() => UIManager.instance.IsWaitFinished());
                    StartCoroutine(GetTrickScore());
                    yield return new WaitUntil(() => getTrickScoreFlag);
                }
                if(isBirdTrick) //���� �� ���
                {
                    chooseFlag = false;
                    GameManager.instance.isSelectionEnabled = false; //ī�� ������ disable �ϰ�
                    isBirdTrick = false;
                    UIManager.instance.ActivateBirdWishSelection(BirdWishChooseCall);
                    yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
                    UIManager.instance.DeactivateBirdWishSelection();
                }
            }
        }

        coroutineFinishFlag = true;
    }
}
