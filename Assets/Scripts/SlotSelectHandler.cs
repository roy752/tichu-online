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
        if (GameManager.instance.currentCard != null) //ī�尡 ���õǾ� �ִٸ� currentSlot �� ������ null ���� ����.
        {
            if (card != null) PopCardFromSlot();
            PushCardToSlot(GameManager.instance.currentCard);
            GameManager.instance.currentPlayer.SortCards();
            GameManager.instance.currentSlot = null;
        }
        else //ī�尡 ���õǾ� ���� �ʴٸ�
        {
            if (card != null)
            {
                PopCardFromSlot(); //���Կ� ī�尡 ���� ��� �A��.
                GameManager.instance.currentPlayer.SortCards();
                GameManager.instance.currentSlot?.ToggleSelection();
            }
            else
            {
                ToggleBase(); //���� ���.
                if (GameManager.instance.currentSlot != null) //���� ���õ� ������ �ְ�
                {
                    if (GameManager.instance.currentSlot == this) GameManager.instance.currentSlot = null; //�װ� ���� ��� currentSlot �� null.
                    else
                    {
                        GameManager.instance.currentSlot.ToggleBase(); //�װ� ���� �ƴ� ��� �� ������ ����.
                        GameManager.instance.currentSlot = this; //���� Ȱ��ȭ�� ������ '��'.
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
        GameManager.instance.currentPlayer.AddCards(new List<Global.Card> { card }); //���Կ��� ī�带 ����.
        card.cardObject.transform.position = Global.hiddenCardPosition;
        card.isFixed = false;
        Global.Card tmpCard = card;
        card = null;
        return tmpCard;
    }
}
