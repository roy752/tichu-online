using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour
{
    public List<Global.Card> cards             = new List<Global.Card>();
    public List<Global.Card> selectCardList    = new List<Global.Card>();
    public Global.PlayerReceiveCardSlot[] slot = new Global.PlayerReceiveCardSlot[Global.numberOfSlots];

    public string playerName;
    public int    playerNumber;
    public int    roundScore;
    public int    totalScore;

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
        slot[Global.GetCardGiverIdx(cardGiver, this)].card   = card;
    }
    public void RemoveCard(Global.Card card)
    {
        cards.Remove(card);
    }

    public void SortCards()
    {
        cards = cards.OrderBy(x => x.value).ToList();
    }

    public void AddSelection(Global.Card card)
    {
        selectCardList.Add(card);
        selectCardList = selectCardList.OrderBy(x => x.value).ToList();
    }

    public void RemoveSelection(Global.Card card)
    {
        selectCardList.Remove(card);
    }

    public Global.Card GetCard(GameObject cardObject)
    {
        foreach (var item in cards)
        {
            if (item.cardObject == cardObject)
            {
                return item;
            }
        }
        return null;
    }

    public void DisableSelection()
    {
        foreach (var card in cards) if (card.cardObject.GetComponent<SelectionHandler>().isSelected) card.cardObject.GetComponent<SelectionHandler>().ToggleSelection();
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
            GameManager.instance.currentCard?.cardObject.GetComponent<SelectionHandler>().ToggleSelection();
            GameManager.instance.currentSlot?.ToggleSelection();
            chooseFlag = true;
        }
        else
        {
            UIManager.instance.Massage(Global.SlotSelectErrorMsg);
            return;
        }
    }

    public void RandomExchangeCardCall()
    {
        for (int i = 0; i < Global.numberOfSlots; ++i)
        {
            if (UIManager.instance.exchangeCard.slots[i].slot.card == null)
            {
                cards[Random.Range(0, cards.Count)].cardObject.GetComponent<SelectionHandler>().ToggleSelection();
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
            AddCards(new List<Global.Card> { slot[idx].card });
            slot[idx].card.isFixed = false;
            slot[idx].card = null;
        }
        SortCards();
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        //���ۿ� �ִ� ī�带 AddCard() �ϰ�, isFixed Ǯ��, �����ϰ�, ����.
    }

    public void SelectTrickCall()
    {
        //������ �´��� Ȯ���ϰ�, ������ ī�带 �����ؼ� �����ϴ� �߰� ���� �ʿ���.

        GameManager.instance.trickStack.Push(new Global.Trick(selectCardList));

        foreach (var card in selectCardList)
        {
            RemoveCard(card);
        }


        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);
        chooseFlag = true;
    }

    public void PassTrickCall()
    {
        DisableSelection();
        chooseFlag = true;
    }



    public void ChooseLargeTichu()
    {
        SortCards();
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public void ExchangeCards()
    {
        SortCards();
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

        UIManager.instance.ActivateTrickSelection(SelectTrickCall,PassTrickCall,DeclareSmallTichuCall);
        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
        if (chooseFlag == false) PassTrickCall();
        UIManager.instance.DeactivateTrickSelection();


        coroutineFinishFlag = true;
    }
}
