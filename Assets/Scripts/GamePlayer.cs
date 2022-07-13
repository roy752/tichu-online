using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        if (nowTrick?.trickType == Global.TrickType.Dog) startIdx++;
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
        }
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
            //'나' 가 트릭을 가져갑니다.(3초)
            Debug.Log("트릭을 가져갈 차례");
            GameManager.instance.trickFinishFlag = true;
            GameManager.instance.startPlayerIdx = playerNumber;
            GameManager.instance.ClearPass();
            UIManager.instance.ShowInfo(Global.GetTrickTakeInfo(playerName));
            UIManager.instance.Wait(Global.trickTakeDuration);
            yield return new WaitUntil(()=>UIManager.instance.IsWaitFinished());
        }
        else if(GameManager.instance.IsAllPassed())
        {
            //bomb 받는 과정 활성화. 메세지 교체.
            Debug.Log("폭탄을 받을 차례");
            UIManager.instance.ActivateBombSelection(SelectBombCall, PassTrickCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) PassTrickCall();
            UIManager.instance.DeactivateBombSelection();
        }
        else
        {
            Debug.Log("일반 교환할 차례");
            UIManager.instance.ActivateTrickSelection(SelectTrickCall, PassTrickCall, DeclareSmallTichuCall);
            yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
            if (chooseFlag == false) PassTrickCall();
            UIManager.instance.DeactivateTrickSelection();
        }

        coroutineFinishFlag = true;
    }
}
