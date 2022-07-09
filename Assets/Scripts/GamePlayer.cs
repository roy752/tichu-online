using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour
{
    public List<Global.Card> cards      = new List<Global.Card>();
    public List<Global.Card> cardBuffer = new List<Global.Card>();
    public string playerName;
    public int    playerNumber;
    public int roundScore;
    public int totalScore;

    public bool chooseFlag          = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag      = false;
    public bool smallTichuFlag      = false;
      
    public void AddCards(List<Global.Card> cardList)
    {
        cards.AddRange(cardList);
    }

    public void AddCardToBuffer(Global.Card card)
    {
        cardBuffer.Add(card);
    }

    public void RemoveCard(Global.Card card)
    {
        cards.Remove(card);
    }

    public void DeclareLargeTichu()
    {
        chooseFlag = true;
        largeTichuFlag = true;
    }

    public void SkipLargeTichu()
    {
        chooseFlag = true;
        largeTichuFlag = false;
    }

    public void ChooseExchangeCard()
    {
        if (UIManager.instance.IsAllSlotSelected()) chooseFlag = true;
        else
        {
            UIManager.instance.Massage(Global.SlotSelectErrorMsg);
            return;
        }
    }

    public void ChooseLargeTichu()
    {
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public IEnumerator ChooseLargeTichuCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        SortCards();
        coroutineFinishFlag = false;
        
        chooseFlag = false;
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInLargeTichuPhase, cards);
        

        UIManager.instance.ActivateTimer(Global.largeTichuDuration);
        UIManager.instance.ActivateLargeTichu(DeclareLargeTichu, SkipLargeTichu);
        yield return new WaitUntil(()=>chooseFlag == true || UIManager.instance.IsTimeOut());
        UIManager.instance.DeactivateLargeTichu();
        UIManager.instance.DeactivateTimer();

        coroutineFinishFlag = true;
    }

    public void ExchangeCards()
    {
        StartCoroutine(ExchangeCardsCoroutine());
    }
    public IEnumerator ExchangeCardsCoroutine()
    {
        UIManager.instance.RenderPlayerInfo();
        SortCards();
        coroutineFinishFlag = false;

        chooseFlag = false;
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, cards);

        UIManager.instance.ActivateTimer(Global.exchangeCardsDuration);
        UIManager.instance.ActivateExchangeCardsPopup(ChooseExchangeCard);
        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
        UIManager.instance.DeactivateExchangeCardsPopup();
        UIManager.instance.DeactivateTimer();

        coroutineFinishFlag = true;
    }

    public void SortCards()
    {
        cards = cards.OrderBy(x => x.value).ToList();
    }
}
