using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GamePlayer : MonoBehaviour
{
    public List<GameManager.Card> cards = new List<GameManager.Card>();
    public List<GameManager.Card> cardBuffer = new List<GameManager.Card>();
    public string playerName;
    public int playerNumber;

    public bool chooseFlag = false;
    public bool coroutineFinishFlag = false;
    public bool largeTichuFlag = false;
    float       duration;

    public void AddCards(List<GameManager.Card> cardList)
    {
        cards.AddRange(cardList);
    }

    public void AddCardToBuffer(GameManager.Card card)
    {
        cardBuffer.Add(card);
    }

    public void RemoveCard(GameManager.Card card)
    {
        cards.Remove(card);
    }

    public IEnumerator TimerCoroutine(float inputDuration)
    {
        duration = inputDuration;
        while(duration>=0)
        {
            UIManager.instance.ShowTimer(((int)(duration)).ToString());
            duration -= GlobalInfo.tick;
            yield return new WaitForSeconds(GlobalInfo.tick);
        }
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
        chooseFlag = true;
    }

    public void ChooseLargeTichu()
    {
        StartCoroutine(ChooseLargeTichuCoroutine());
    }

    public IEnumerator ChooseLargeTichuCoroutine()
    {
        IEnumerator t = TimerCoroutine(GlobalInfo.largeTichuDuration);

        coroutineFinishFlag = false;
        
        chooseFlag = false;
        GameManager.instance.RenderCards(GlobalInfo.initialPosition, 4, cards);
        

        UIManager.instance.ActivateTimer();
        StartCoroutine(t);


        UIManager.instance.ActivateLargeTichu(DeclareLargeTichu, SkipLargeTichu);
        yield return new WaitUntil(()=>chooseFlag == true || duration<0);
        UIManager.instance.DeactivateLargeTichu();
        
        
        StopCoroutine(t);
        UIManager.instance.DeactivateTimer();


        coroutineFinishFlag = true;
    }

    public void ExchangeCards()
    {
        StartCoroutine(ExchangeCardsCoroutine());
    }
    public IEnumerator ExchangeCardsCoroutine()
    {
        IEnumerator t = TimerCoroutine(GlobalInfo.exchangeCardsDuration);

        coroutineFinishFlag = false;

        chooseFlag = false;
        GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, cards);

        UIManager.instance.ActivateTimer();
        StartCoroutine(t);

        UIManager.instance.ActivateExchangeCardsPopup(ChooseExchangeCard);
        yield return new WaitUntil(() => chooseFlag == true || duration < 0);
        UIManager.instance.DeactivateExchangeCardsPopup();

        StopCoroutine(t);
        UIManager.instance.DeactivateTimer();

        coroutineFinishFlag = true;
    }
}
