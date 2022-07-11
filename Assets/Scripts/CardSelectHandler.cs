
public class CardSelectHandler : SelectionHandler
{
    public override void ToggleSelection()
    {
        if (GameManager.instance.isMultipleSelectionEnabled) //���� ������ enabled �� ���, ������ disable �Ǿ������� ����Ǿ�����.
        {
            ToggleBase();
            Global.Card card = GameManager.instance.currentPlayer.GetCard(gameObject);
            if (isSelected) GameManager.instance.currentPlayer.AddSelection(card);
            else            GameManager.instance.currentPlayer.RemoveSelection(card);
        }
        else
        {
            ToggleBase();
            if (GameManager.instance.currentSlot != null) //���õ� ������ �ִ� ���
            {
                GameManager.instance.currentCard = GameManager.instance.currentPlayer.GetCard(gameObject);
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
                        GameManager.instance.currentCard = GameManager.instance.currentPlayer.GetCard(gameObject);
                    }
                }
                else GameManager.instance.currentCard = GameManager.instance.currentPlayer.GetCard(gameObject);
            }
        }
        UIManager.instance.RenderCards(Global.initialPosition, Global.numberOfCardsForLineInSmallTichuPhase, GameManager.instance.currentPlayer.cards);
    }
}
