using System.Collections.Generic;

public class SlotSelectHandler : SelectionHandler
{
    public Card card;
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
                ToggleBase();
                if (GameManager.instance.currentSlot == this)          GameManager.instance.currentSlot = null; 
                else { GameManager.instance.currentSlot?.ToggleBase(); GameManager.instance.currentSlot = this;}
            }
        }
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, GameManager.instance.currentPlayer.cards);
    }

    public void PushCardToSlot(Card inputCard)
    {
        GameManager.instance.currentPlayer.RemoveCard(inputCard);
        inputCard.transform.position = gameObject.transform.position + Global.frontEpsilon;
        inputCard.isFixed = true;
        card = inputCard;
        card.ToggleBase();
        GameManager.instance.currentCard = null;
    }
    public Card PopCardFromSlot()
    {
        GameManager.instance.currentPlayer.AddCard(card); //슬롯에서 카드를 뺀다.
        card.transform.position = Global.hiddenCardPosition;
        card.isFixed = false;
        Card tmpCard = card;
        card = null;
        return tmpCard;
    }
}
