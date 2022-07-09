using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardSelectHandler : SelectionHandler
{
    public override void ToggleSelection()
    {
        if (GameManager.instance.isMultipleSelectionAllowed)
        {

        }
        else
        {
            ToggleBase();
            if (GameManager.instance.currentSlot != null) //선택된 슬롯이 있는 경우
            {
                GameManager.instance.SetCurrentCard(gameObject);
                GameManager.instance.currentSlot.PushCardToSlot(GameManager.instance.currentCard);
                GameManager.instance.currentSlot.ToggleBase();
                GameManager.instance.currentSlot = null;
            }
            else
            {
                if (GameManager.instance.currentCard != null)
                {
                    if (GameManager.instance.currentCard.cardObject == gameObject) GameManager.instance.currentCard = null;
                    else
                    {
                        GameManager.instance.currentCard.cardObject.GetComponent<CardSelectHandler>().ToggleBase();
                        GameManager.instance.SetCurrentCard(gameObject);
                    }
                }
                else GameManager.instance.SetCurrentCard(gameObject);
            }
        }
        UIManager.instance.RenderCards(Global.initialPosition, 5, GameManager.instance.currentPlayer.cards);
    }
}
