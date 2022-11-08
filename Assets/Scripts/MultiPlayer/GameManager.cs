using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MultiPlayer.Define;
using static Util;
using System.Linq;
using UnityEngine.SocialPlatforms;

namespace MultiPlayer
{
    public class GameManager : MonoBehaviour
    {

        [HideInInspector]
        public GameObject cardsParent;

        public static GameManager instance;

        [HideInInspector]
        public GamePlayer[] players;

        [HideInInspector]
        public List<Card> cards = new List<Card>();

        Card phoenix = null;
        Card bird = null;

        void Awake()
        {
            InitializeVariables();
            InitializePlayers();
            MakeCards();
            instance = this;
        }

        public void GameStart(List<int> cardId)
        {
            EventManager.instance.DeactivateInput();
            GameObject.Find("GameUI").SetActive(true);
            cardId.ForEach(id => players[0].AddCard(cards.Where(x => x.id == id).First()));
            for(int i=0; i<4; ++i)
            UIManager.instance.SetPlayerInfo(players[i].playerName, (byte)players[i].playerNumber);
            UIManager.instance.RenderCards(Util.initialPosition, Util.numberOfCardsForLineInSmallTichuPhase, players[0].cards);
        }

        // Start is called before the first frame update

        // Update is called once per frame

        void InitializeVariables()
        {
            cardsParent = GameObject.Find(Define.cardsParentObjectName);
        }

        void InitializePlayers()
        {
            players = new GamePlayer[Util.numberOfPlayers];

            for (int idx = 0; idx < Util.numberOfPlayers; ++idx)
            {
                players[idx] = GameObject.Find(Util.playerObjectNames[idx]).GetComponent<GamePlayer>();
            }
        }
        void MakeCards()
        {
            int typeNumber = 0;
            int idNumber = 0;
            int idx = 0;
            foreach (string cardName in Enum.GetNames(typeof(Util.CardType)))
            {
                if (typeNumber < Util.numberOfGeneralCardType)
                {
                    for (int i = 1; i <= Util.numberOfCardsGeneral; ++i)
                    {
                        var nowCard = (
                                        Instantiate(Resources.Load(Util.prefabPath + Util.GetCardName(cardName, i)), hiddenCardPosition,
                                                    initialCardRotation, cardsParent.transform) as GameObject
                                        ).GetComponent<Card>();


                        nowCard.cardName = GetCardName(cardName, i); nowCard.type = (CardType)Enum.Parse(typeof(CardType), cardName);
                        nowCard.value = generalCardsValue[i]; nowCard.id = idNumber; nowCard.score = generalCardsScore[i];
                        cards.Add(nowCard);
                        idNumber++;
                    }
                    typeNumber++;
                }
                else
                {
                    var nowCard = (
                                    Instantiate(Resources.Load(prefabPath + cardName), hiddenCardPosition,
                                                initialCardRotation, cardsParent.transform) as GameObject
                                    ).GetComponent<Card>();

                    nowCard.cardName = cardName; nowCard.type = (CardType)Enum.Parse(typeof(CardType), cardName);
                    nowCard.value = specialCardsValue[idx]; nowCard.id = idNumber; nowCard.score = specialCardsScore[idx];
                    cards.Add(nowCard);
                    if (nowCard.type == CardType.Phoenix) phoenix = nowCard;
                    if (nowCard.type == CardType.Bird) bird = nowCard;
                    idNumber++; idx++; typeNumber++;
                }
            }
        }
    }
}
