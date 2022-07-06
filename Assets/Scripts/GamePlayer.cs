using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    public List<GameManager.Card> cards = new List<GameManager.Card>();

    public void AddCards(List<GameManager.Card> cardList)
    {
        cards.AddRange(cardList);
        Debug.Log(cards.Count);
    }
}
