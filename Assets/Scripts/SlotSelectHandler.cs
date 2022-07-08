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
        if(GameManager.instance.currentCard != null) //ī�尡 ���õǾ� �ִٸ� currentSlot �� ������ null ���� ����.
        {
            GameManager.instance.currentPlayer.RemoveCard(GameManager.instance.currentCard);
            GameManager.instance.currentCard.cardObject.transform.position = gameObject.transform.position + GlobalInfo.frontEpsilon;

            if (card != null)
            {
                GameManager.instance.currentPlayer.AddCards(new List<GameManager.Card> { card });
                GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, GameManager.instance.currentPlayer.cards); //�ٽ� ī�� ������?
            }
            card = GameManager.instance.currentCard;
            GameManager.instance.currentCard = null;
        }
        else //ī�尡 ���õǾ� ���� �ʴٸ�
        {
            if(card != null) // ���Կ� ī�尡 ���� ���
            {
                GameManager.instance.currentPlayer.AddCards(new List<GameManager.Card> { card }); //ī�带 ����.
                GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, GameManager.instance.currentPlayer.cards); //�ٽ� ī�� ������?
            }
            else // ���Կ� ī�尡 ���� ���
            {
                base.ToggleSelection(); //���� ���.
                if (GameManager.instance.currentSlot != null) //���� ���õ� ������ 
                {
                    if (GameManager.instance.currentSlot == this) GameManager.instance.currentSlot = null; //�ְ� �װ� ���� ��� currentSlot �� null.
                    else
                    {
                        GameManager.instance.currentSlot.ToggleSelection(); //�ְ� �װ� ���� �ƴ� ��� �� ������ ����.
                        GameManager.instance.currentSlot = this; //���� Ȱ��ȭ�� ������ '��'.
                    }
                }
            }
        }
    }
}
