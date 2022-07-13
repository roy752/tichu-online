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
    public Global.PlayerReceiveCardSlot[] slot = new Global.PlayerReceiveCardSlot[Global.numberOfSlots];

    public string playerName;
    public int    playerNumber;
    public int    roundScore;
    public int    totalScore;
    public int    smallTichuScore;
    public int    largeTichuScore;

    public bool chooseFlag          = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag      = false;
    public bool smallTichuFlag      = false;
    public int  ranking;

    public bool canDeclareSmallTichu = true;

    public bool isFinished = false;

    public string previousTrick = null;

      
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
        slot[Global.GetCardGiverIdx(cardGiver, this)].player = cardGiver;
        slot[Global.GetCardGiverIdx(cardGiver, this)].card   = card;
    }
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }

    public void AddSelection(Card card)
    {
        selectCardList.Add(card);
        cards.Remove(card);
        Global.SortCard(ref selectCardList);
    }

    public void RemoveSelection(Card card)
    {
        selectCardList.Remove(card);
        cards.Add(card);
        Global.SortCard(ref cards);
    }

    public void DisableSelection()
    {
        cards.AddRange(selectCardList);
        ClearSelection();
        Global.SortCard(ref cards);
    }

    public void ClearSelection()
    {
        foreach (var selectedCard in selectCardList) selectedCard.ToggleBase();
        selectCardList.Clear();
    }

    public bool CalculateHandRunOut()
    {
        if (cards.Count == 0) { ranking = GameManager.instance.CountFinishedPlayer() + 1; isFinished = true; }
        return isFinished;
        //스몰 티츄, 라지 티츄 관련 로직이 있으면 좋을 듯.
    }

    public void FindNextPlayer(Global.Trick nowTrick)
    {
        int startIdx = GameManager.instance.startPlayerIdx + 1;
        if (nowTrick?.trickType == Global.TrickType.Dog) { startIdx++; GameManager.instance.trickFinishFlag = true; }
        for(int i = 0; i<Global.numberOfPlayers; ++i)
        {
            if (GameManager.instance.players[startIdx%Global.numberOfPlayers] != GameManager.instance.currentPlayer && 
                GameManager.instance.players[startIdx%Global.numberOfPlayers].isFinished == false) break;
            ++startIdx;
        }
        GameManager.instance.startPlayerIdx = startIdx;
    }

    public void CalculateIsRoundEnd()
    {
        if(isFinished == true&&GameManager.instance.players[(playerNumber+2)%2].isFinished == true)
        {
            GameManager.instance.isRoundEnd = true;
            GameManager.instance.trickFinishFlag = true;
            // 원투로 끝남. 원투 관련 셋업.
        }
        else if(isFinished == true && GameManager.instance.CountFinishedPlayer()==2)
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
            UIManager.instance.ActivateDragonSelection(DragonChooseNextOpponentCall,DragonChoosePreviousOpponentCall);
            yield return new WaitUntil(() => dragonChooseFlag || UIManager.instance.IsTimeOut());
            UIManager.instance.DeactivateDragonSelection();
            if (dragonChooseFlag == false) RandomDragonChooseCall();

            var trickTaker = GameManager.instance.players[dragonChoosePlayerIdx % Global.numberOfPlayers];
            foreach (var trick in GameManager.instance.trickStack)
            {
                int score = 0;
                foreach (var card in trick.cards)
                {
                    score += card.score;
                    card.transform.position = Global.hiddenCardPosition;
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
                    card.transform.position = Global.hiddenCardPosition;
                }
                roundScore += score;
            }
        }
        GameManager.instance.trickStack.Clear();
        getTrickScoreFlag = true;
    }

    private bool isDogTrick;
    public void ProgressIfDog(Global.Trick nowTrick)
    {
        if (nowTrick.trickType == Global.TrickType.Dog)
        {
            isDogTrick = true;
        }
        else isDogTrick = false;
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
            UIManager.instance.Massage(Global.slotSelectErrorMsg);
            return;
        }
    }

    public void RandomExchangeCardCall()
    {
        for (int i = 0; i < Global.numberOfSlots; ++i)
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
        Global.SortCard(ref cards);
        //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        //버퍼에 있는 카드를 AddCard() 하고, isFixed 풀고, 정렬하고, 렌더.
    }

    public void SelectTrickCall()
    {
        Global.Trick nowTrick = Global.MakeTrick(selectCardList);

        if (GameManager.instance.isTrickValid(nowTrick))
        {
            previousTrick = Global.GetTrickInfo(nowTrick);
            UIManager.instance.RenderTrickCard(selectCardList);
            GameManager.instance.trickStack.Push(nowTrick); // 수정 필요.
            ClearSelection();
            GameManager.instance.ClearPass();
            canDeclareSmallTichu = false;

            CalculateHandRunOut();
            FindNextPlayer(nowTrick);
            ProgressIfDog(nowTrick);
            CalculateIsRoundEnd();

            //UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
            chooseFlag = true;
        }
        else
        {
            UIManager.instance.Massage(Global.trickSelectErrorMsg);
            return;
        }
    }

    public void SelectBombCall()
    {
        Global.Trick nowTrick = Global.MakeTrick(selectCardList);

        if(nowTrick.trickType==Global.TrickType.FourCardBomb || nowTrick.trickType==Global.TrickType.StraightFlushBomb)
        {
            SelectTrickCall();
        }
        else
        {
            UIManager.instance.Massage(Global.trickSelectErrorMsg);
            return;
        }
    }

    public void PassTrickCall()
    {
        DisableSelection();
        GameManager.instance.AddPass();
        previousTrick = Global.passInfo;
        FindNextPlayer(null);
        chooseFlag = true;
    }

    public void PassOrRandomSingleTrickCall()
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



    public void ChooseLargeTichu()
    {
        Global.SortCard(ref cards);
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public void ExchangeCards()
    {
        Global.SortCard(ref cards);
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
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInLargeTichuPhase, cards);
        
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
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        
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
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        
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
        UIManager.instance.RenderPlayerInfo();
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);

        chooseFlag = false;

        coroutineFinishFlag = false;
        if(GameManager.instance.IsAllDone())
        {
            StartCoroutine(GetTrickScore());
            yield return new WaitUntil(() => getTrickScoreFlag);
            GameManager.instance.trickFinishFlag = true;
            GameManager.instance.startPlayerIdx = playerNumber;
            GameManager.instance.ClearPass();

            if (dragonChooseFlag)
            {
                UIManager.instance.ShowInfo(Global.GetTrickTakeInfo(GameManager.instance.players[dragonChoosePlayerIdx % Global.numberOfPlayers].playerName));
                dragonChooseFlag = false;
            }
            else UIManager.instance.ShowInfo(Global.GetTrickTakeInfo(playerName));
            
            UIManager.instance.Wait(Global.trickTakeDuration);
            yield return new WaitUntil(()=>UIManager.instance.IsWaitFinished());
        }
        else if(GameManager.instance.IsAllPassed())
        {
            UIManager.instance.ActivateBombSelection(SelectBombCall, PassTrickCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) PassTrickCall();
            UIManager.instance.DeactivateBombSelection();
        }
        else
        {
            UIManager.instance.ActivateTrickSelection(SelectTrickCall, PassTrickCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) PassOrRandomSingleTrickCall();
            UIManager.instance.DeactivateTrickSelection();

            if(isDogTrick)
            {
                isDogTrick = false;
                UIManager.instance.ShowInfo(GameManager.instance.players[GameManager.instance.startPlayerIdx % Global.numberOfPlayers].playerName + Global.selectDogInfo);
                UIManager.instance.Wait(Global.selectDogDuration);
                yield return new WaitUntil(() => UIManager.instance.IsWaitFinished());
                StartCoroutine(GetTrickScore());
                yield return new WaitUntil(() => getTrickScoreFlag);
            }

        }

        coroutineFinishFlag = true;
    }
}
