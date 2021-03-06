using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : SelectionHandler
{
    [HideInInspector]
    public string          cardName;
    [HideInInspector]
    public int             value;
    [HideInInspector]
    public Util.CardType type;
    [HideInInspector]
    public int             id;
    [HideInInspector]
    public bool            isFixed = false;
    [HideInInspector]
    public int             score;

    public override void ToggleSelection()
    {
        ToggleBase();
        if (GameManager.instance.isMultipleSelectionEnabled) //다중 선택이 enabled 된 경우, 슬롯이 disable 되어있음이 보장되어있음.
        {
            // 봉황의 value를 predict 하는 로직 구현 필요. 봉황은 숫자카드로만 대체할 수 있으므로 value estimation 이 one&only 중요함.
            if (isSelected) GameManager.instance.currentPlayer.AddSelection(this);
            else            GameManager.instance.currentPlayer.RemoveSelection(this);
            // 봉황 predict 하고
            Util.EstimatePhoenix(GameManager.instance.currentPlayer.selectCardList, Util.FindPhoenix(GameManager.instance.currentPlayer.selectCardList));
            Util.SortCard(ref GameManager.instance.currentPlayer.selectCardList);
            UIManager.instance.ShowInfo(Util.GetTrickInfo(Util.MakeTrick(GameManager.instance.currentPlayer.selectCardList)));
        }
        else
        {
            if (GameManager.instance.currentSlot != null) //선택된 슬롯이 있는 경우
            {
                GameManager.instance.currentSlot.PushCardToSlot(this);
                GameManager.instance.currentSlot.ToggleBase();
                GameManager.instance.currentSlot = null;
            }
            else
            {
                if (GameManager.instance.currentCard == this)         GameManager.instance.currentCard = null;
                else {GameManager.instance.currentCard?.ToggleBase(); GameManager.instance.currentCard = this;}
            }
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, GameManager.instance.currentPlayer.cards);
        }
    }
    public void ResetByToggle()
    {
        if (isSelected) ToggleBase();
    }
}
