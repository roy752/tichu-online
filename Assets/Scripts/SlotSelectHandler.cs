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
        if (GameManager.instance.currentCard != null) //ī�尡 ���õǾ� �ִٸ� currentSlot �� ������ null ���� ����.
        {
            if (card != null) PopCardFromSlot();
            PushCardToSlot(GameManager.instance.currentCard);
            Util.SortCard(ref GameManager.instance.currentPlayer.cards);
            GameManager.instance.currentSlot = null;
        }
        else //ī�尡 ���õǾ� ���� �ʴٸ�
        {
            if (card != null)
            {
                PopCardFromSlot(); //���Կ� ī�尡 ���� ��� �A��.
                Util.SortCard(ref GameManager.instance.currentPlayer.cards);
                GameManager.instance.currentSlot?.ToggleSelection();
            }
            else
            {
                ToggleBase();
                if (GameManager.instance.currentSlot == this)          GameManager.instance.currentSlot = null; 
                else { GameManager.instance.currentSlot?.ToggleBase(); GameManager.instance.currentSlot = this;}
            }
        }
        UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, GameManager.instance.currentPlayer.cards);
    }

    public void PushCardToSlot(Card inputCard)
    {
        GameManager.instance.currentPlayer.RemoveCard(inputCard);
        inputCard.transform.position = gameObject.transform.position + Util.frontEpsilon;
        inputCard.isFixed = true;
        card = inputCard;
        card.ToggleBase();
        GameManager.instance.currentCard = null;
    }
    public Card PopCardFromSlot()
    {
        GameManager.instance.currentPlayer.AddCard(card); //���Կ��� ī�带 ����.
        card.transform.position = Util.hiddenCardPosition;
        card.isFixed = false;
        Card tmpCard = card;
        card = null;
        return tmpCard;
    }
}
