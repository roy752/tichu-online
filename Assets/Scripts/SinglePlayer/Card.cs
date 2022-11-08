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
    public Util.CardType   type;
    [HideInInspector]
    public int             id;
    [HideInInspector]
    public bool            isFixed = false;
    [HideInInspector]
    public int             score;

    public override void ToggleSelection()
    {
        ToggleBase();
        if (GameManager.instance.isMultipleSelectionEnabled) //���� ������ enabled �� ���, ������ disable �Ǿ������� ����Ǿ�����.
        {
            // ��Ȳ�� value�� predict �ϴ� ���� ���� �ʿ�. ��Ȳ�� ����ī��θ� ��ü�� �� �����Ƿ� value estimation �� one&only �߿���.
            if (isSelected) GameManager.instance.currentPlayer.AddSelection(this);
            else            GameManager.instance.currentPlayer.RemoveSelection(this);
            // ��Ȳ predict �ϰ�
            Util.EstimatePhoenix(GameManager.instance.currentPlayer.selectCardList, Util.FindPhoenix(GameManager.instance.currentPlayer.selectCardList));
            Util.SortCard(ref GameManager.instance.currentPlayer.selectCardList);
            UIManager.instance.ShowInfo(Util.GetTrickInfo(Util.MakeTrick(GameManager.instance.currentPlayer.selectCardList)));
        }
        else
        {
            if (GameManager.instance.currentSlot != null) //���õ� ������ �ִ� ���
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
