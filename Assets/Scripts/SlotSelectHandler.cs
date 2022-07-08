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
        if(GameManager.instance.currentCard != null) //카드가 선택되어 있다면 currentSlot 은 언제나 null 임이 보장.
        {
            GameManager.instance.currentPlayer.RemoveCard(GameManager.instance.currentCard);
            GameManager.instance.currentCard.cardObject.transform.position = gameObject.transform.position + GlobalInfo.frontEpsilon;

            if (card != null)
            {
                GameManager.instance.currentPlayer.AddCards(new List<GameManager.Card> { card });
                GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, GameManager.instance.currentPlayer.cards); //다시 카드 렌더링?
            }
            card = GameManager.instance.currentCard;
            GameManager.instance.currentCard = null;
        }
        else //카드가 선택되어 있지 않다면
        {
            if(card != null) // 슬롯에 카드가 있을 경우
            {
                GameManager.instance.currentPlayer.AddCards(new List<GameManager.Card> { card }); //카드를 뺀다.
                GameManager.instance.RenderCards(GlobalInfo.initialPosition, 5, GameManager.instance.currentPlayer.cards); //다시 카드 렌더링?
            }
            else // 슬롯에 카드가 없을 경우
            {
                base.ToggleSelection(); //슬롯 토글.
                if (GameManager.instance.currentSlot != null) //현재 선택된 슬롯이 
                {
                    if (GameManager.instance.currentSlot == this) GameManager.instance.currentSlot = null; //있고 그게 나인 경우 currentSlot 은 null.
                    else
                    {
                        GameManager.instance.currentSlot.ToggleSelection(); //있고 그게 내가 아닌 경우 그 슬롯은 끈다.
                        GameManager.instance.currentSlot = this; //현재 활성화된 슬롯은 '나'.
                    }
                }
            }
        }
    }
}
