using System.Collections.Generic;

public class SlotSelectHandler : SelectionHandler
{
    public Global.Card card;
    public override void OnEnable()
    {
        base.OnEnable();
        card = null;
    }
    public override void ToggleSelection()
    {
        if (GameManager.instance.currentCard != null) //카드가 선택되어 있다면 currentSlot 은 언제나 null 임이 보장.
        {
            if (card != null) PopCardFromSlot();
            PushCardToSlot(GameManager.instance.currentCard);
            GameManager.instance.currentPlayer.SortCards();
            GameManager.instance.currentSlot = null;
        }
        else //카드가 선택되어 있지 않다면
        {
            if (card != null)
            {
                PopCardFromSlot(); //슬롯에 카드가 있을 경우 뺸다.
                GameManager.instance.currentPlayer.SortCards();
                GameManager.instance.currentSlot?.ToggleSelection();
            }
            else
            {
                ToggleBase(); //슬롯 토글.
                if (GameManager.instance.currentSlot != null) //현재 선택된 슬롯이 있고
                {
                    if (GameManager.instance.currentSlot == this) GameManager.instance.currentSlot = null; //그게 나인 경우 currentSlot 은 null.
                    else
                    {
                        GameManager.instance.currentSlot.ToggleBase(); //그게 내가 아닌 경우 그 슬롯은 끈다.
                        GameManager.instance.currentSlot = this; //현재 활성화된 슬롯은 '나'.
                    }
                }
                else GameManager.instance.currentSlot = this;
            }
        }
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, GameManager.instance.currentPlayer.cards);
    }

    public void PushCardToSlot(Global.Card inputCard)
    {
        GameManager.instance.currentPlayer.RemoveCard(inputCard);
        inputCard.cardObject.transform.position = gameObject.transform.position + Global.frontEpsilon;
        inputCard.isFixed = true;
        card = inputCard;
        card.cardObject.GetComponent<SelectionHandler>().ToggleBase();
        GameManager.instance.currentCard = null;
    }
    public Global.Card PopCardFromSlot()
    {
        GameManager.instance.currentPlayer.AddCards(new List<Global.Card> { card }); //슬롯에서 카드를 뺀다.
        card.cardObject.transform.position = Global.hiddenCardPosition;
        card.isFixed = false;
        Global.Card tmpCard = card;
        card = null;
        return tmpCard;
    }
}
