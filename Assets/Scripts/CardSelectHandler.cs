using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardSelectHandler : SelectionHandler
{
    public override void ToggleSelection()
    {
        base.ToggleSelection();
        if (GameManager.instance.isMultipleSelectionAllowed)
        {

        }
        else
        {
            if (GameManager.instance.currentSlot != null)
            {
                SlotSelectHandler tmpSlot = GameManager.instance.currentSlot;
                GameManager.instance.currentSlot.ToggleSelection();
                tmpSlot.card = GameManager.instance.currentCard;
                gameObject.transform.position = tmpSlot.gameObject.transform.position;
                GameManager.instance.SetCurrentCard(gameObject);
                GameManager.instance.currentPlayer.RemoveCard(GameManager.instance.currentCard);
                GameManager.instance.currentCard = null;
            }
            else
            {
                if (GameManager.instance.currentCard != null)
                {
                    if (GameManager.instance.currentCard.cardObject == gameObject) GameManager.instance.currentCard = null;
                    else
                    {
                        GameManager.instance.currentCard.cardObject.GetComponent<CardSelectHandler>().ToggleSelection();
                        GameManager.instance.SetCurrentCard(gameObject);
                    }
                }
                else GameManager.instance.SetCurrentCard(gameObject);
            }
        }
    }
}
