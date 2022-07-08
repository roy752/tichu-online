using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotSelectHandler : SelectionHandler
{
    public GameManager.Card card;
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
            GameManager.instance.currentSlot = null;
        }
        else //ī�尡 ���õǾ� ���� �ʴٸ�
        {
            if (card != null)
            {
                PopCardFromSlot(); //���Կ� ī�尡 ���� ��� �A��.
                if (GameManager.instance.currentSlot != null) GameManager.instance.currentSlot.ToggleSelection();
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
        GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, GameManager.instance.currentPlayer.cards);
    }

    public void PushCardToSlot(GameManager.Card inputCard)
    {
        GameManager.instance.currentPlayer.RemoveCard(inputCard);
        inputCard.cardObject.transform.position = gameObject.transform.position + GlobalInfo.frontEpsilon;
        inputCard.isFixed = true;
        card = inputCard;
        card.cardObject.GetComponent<SelectionHandler>().ToggleBase();
        GameManager.instance.currentCard = null;
    }
    public GameManager.Card PopCardFromSlot()
    {
        GameManager.instance.currentPlayer.AddCards(new List<GameManager.Card> { card }); //���Կ��� ī�带 ����.
        card.cardObject.transform.position = GlobalInfo.hiddenCardPosition;
        card.isFixed = false;
        GameManager.Card tmpCard = card;
        card = null;
        return tmpCard;
    }
}
