using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour
{
    public List<GlobalInfo.Card> cards = new List<GlobalInfo.Card>();
    public List<GlobalInfo.Card> cardBuffer = new List<GlobalInfo.Card>();
    public string playerName;
    public int    playerNumber;

    public bool chooseFlag = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag = false;
    public bool smallTichuFlag = false;

    public void AddCards(List<GlobalInfo.Card> cardList)
    {
        cards.AddRange(cardList);
    }

    public void AddCardToBuffer(GlobalInfo.Card card)
    {
        cardBuffer.Add(card);
    }

    public void RemoveCard(GlobalInfo.Card card)
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
            UIManager.instance.Massage(GlobalInfo.SlotSelectErrorMsg);
            return;
        }
    }

    public void ChooseLargeTichu()
    {
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public IEnumerator ChooseLargeTichuCoroutine()
    {
        coroutineFinishFlag = false;
        
        chooseFlag = false;
        GameManager.instance.RenderCards(GlobalInfo.initialPosition, 4, cards);
        

        UIManager.instance.ActivateTimer(GlobalInfo.largeTichuDuration);
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
        coroutineFinishFlag = false;

        chooseFlag = false;
        GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, cards);

        UIManager.instance.ActivateTimer(GlobalInfo.exchangeCardsDuration);
        UIManager.instance.ActivateExchangeCardsPopup(ChooseExchangeCard);
        yield return new WaitUntil(() => chooseFlag == true || UIManager.instance.IsTimeOut());
        UIManager.instance.DeactivateExchangeCardsPopup();
        UIManager.instance.DeactivateTimer();

        coroutineFinishFlag = true;
    }
}
