using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour
{
    public List<Global.Card> cards      = new List<Global.Card>();
    public Global.PlayerReceiveCardSlot[] slot = new Global.PlayerReceiveCardSlot[Global.numberOfSlots];
    public string playerName;
    public int    playerNumber;
    public int roundScore;
    public int totalScore;

    public bool chooseFlag          = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag      = false;
    public bool smallTichuFlag      = false;

    public bool canDeclareSmallTichu = true;
      
    public void AddCards(List<Global.Card> cardList)
    {
        cards.AddRange(cardList);
    }
    public void AddCardToSlot(Global.Card card, GamePlayer cardGiver)
    {
        slot[Global.GetCardGiverIdx(cardGiver, this)].player = cardGiver;
        slot[Global.GetCardGiverIdx(cardGiver, this)].card = card;
    }
    public void RemoveCard(Global.Card card)
    {
        cards.Remove(card);
    }
    public void SortCards()
    {
        cards = cards.OrderBy(x => x.value).ToList();
    }



    public void DeclareLargeTichuCall()
    {
        chooseFlag = true;
        canDeclareSmallTichu = false;
        largeTichuFlag = true;
        UIManager.instance.DeactivateAlertPopup();
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
        UIManager.instance.RenderPlayerInfo();
        UIManager.instance.DeactivateAlertPopup();
    }
    public void ExchangeCardCall()
    {
        if (UIManager.instance.IsAllSlotSelected()) chooseFlag = true;
        else
        {
            UIManager.instance.Massage(Global.SlotSelectErrorMsg);
            return;
        }
    }
    public void ReceiveCardCall()
    {
        chooseFlag = true;
        foreach(var cardSlot in slot)
        {
            AddCards(new List<Global.Card> { cardSlot.card });
            cardSlot.card.isFixed = false;
        }
        SortCards();
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        //버퍼에 있는 카드를 AddCard() 하고, isFixed 풀고, 정렬하고, 렌더.
    }



    public void ChooseLargeTichu()
    {
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public void ExchangeCards()
    {
        StartCoroutine(ExchangeCardsCoroutine());
    }

    public void ReceiveCard()
    {
        StartCoroutine(ReceiveCardCoroutine());
    }




    public IEnumerator ChooseLargeTichuCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        SortCards();
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInLargeTichuPhase, cards);
        
        chooseFlag = false;
        
        coroutineFinishFlag = false;
        
        UIManager.instance.ActivateLargeTichu(DeclareLargeTichuCall, SkipLargeTichuCall);
        yield return new WaitUntil(()=>chooseFlag == true || UIManager.instance.IsTimeOut());
        UIManager.instance.DeactivateLargeTichu();

        coroutineFinishFlag = true;
    }

    public IEnumerator ExchangeCardsCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        SortCards();
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        
        chooseFlag = false;
        
        coroutineFinishFlag = false;

        UIManager.instance.ActivateExchangeCardsPopup(ExchangeCardCall, DeclareSmallTichuCall);
        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
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
        UIManager.instance.DeactivateReceiveCardPopup();

        coroutineFinishFlag = true;
    }
}
