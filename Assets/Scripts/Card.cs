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
    public Global.CardType type;
    [HideInInspector]
    public int             id;
    [HideInInspector]
    public bool            isFixed = false;

    public override void ToggleSelection()
    {
        ToggleBase();
        if (GameManager.instance.isMultipleSelectionEnabled) //���� ������ enabled �� ���, ������ disable �Ǿ������� ����Ǿ�����.
        {
            // ��Ȳ�� value�� predict �ϴ� ���� ���� �ʿ�. ��Ȳ�� ����ī��θ� ��ü�� �� �����Ƿ� value estimation �� one&only �߿���.
            if (isSelected) GameManager.instance.currentPlayer.AddSelection(this);
            else            GameManager.instance.currentPlayer.RemoveSelection(this);
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
            UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, GameManager.instance.currentPlayer.cards);
        }
    }
}
