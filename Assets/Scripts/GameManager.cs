using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public List<Card> cards = new List<Card>();

    [HideInInspector]
    public List<Card> cardsObjectPool = new List<Card>();

    [HideInInspector]
    public class Card
    {
        public GameObject cardObject;
        public string cardName;
        public int value;
        public int type;
        public int id;
    }

    [HideInInspector]
    public GamePlayer[] players;

    private void Awake()
    {
        MakeCardNameList();
    }

    void Start()
    {
        InstantiateCards();
        InitializePlayers();

        ShuffleCards(ref cards);

        StartPlay();
    }

    // Update is called once per frame
    void Update()
    {
        //RenderCards(cards.GetRange(0, GlobalInfo.numberOfCardsPlay).OrderBy(x => x.value).ToList());
    }

    void StartPlay()
    {
        SplitCardsToPlayer(GlobalInfo.numberOfCardsGrandTichuPhase);

        //StartCoroutine(RenderTestCoroutine());
    }

    IEnumerator RenderTestCoroutine()
    {
        int cnt = GlobalInfo.numberOfPlayers;
        while(cnt-->0)
        {
            foreach (var item in cards) item.cardObject.transform.position = GlobalInfo.hiddenCardPosition;
            RenderCards(players[cnt].cards.OrderBy(x => x.value).ToList());
            yield return new WaitForSeconds(3.0f);
        }
    }

    void SplitCardsToPlayer(int num)
    {
        int idx = 0;
        foreach(var player in players)
        {
            player.AddCards(cards.GetRange(idx, num));
            idx += num;
        }
    }

    void MakeCardNameList()
    {
        int type = 0;
        int id = 0;

        foreach (string cardName in System.Enum.GetNames(typeof(GlobalInfo.GeneralCardName)))
        {
            for (int i = 1; i <= GlobalInfo.numberOfCardsGeneral; ++i)
            {
                Card cardInstance = new Card();

                cardInstance.cardName = cardName + i.ToString("D2");
                cardInstance.type     = type;
                cardInstance.value    = i==1? GlobalInfo.aceCardsValue: i;
                cardInstance.id       = id;

                cards.Add(cardInstance);
                id++;
            }
            type++;
        }
        int idx = 0;
        foreach (string cardName in System.Enum.GetNames(typeof(GlobalInfo.SpecialCardName)))
        {
            Card cardInstance = new Card();

            cardInstance.cardName = cardName;
            cardInstance.type     = type;
            cardInstance.value    = GlobalInfo.specialCardsValue[idx];
            cardInstance.id       = id;

            cards.Add(cardInstance);
            id++;
            idx++;
            type++;
        }
    }

    void InstantiateCards()
    {
        Vector3 initialPosition = GlobalInfo.hiddenCardPosition;
        Quaternion initialRotation = Quaternion.identity;
        GameObject cardParent = GameObject.Find("Cards");

        foreach (var item in cards)
        {
            item.cardObject = Instantiate(Resources.Load(GlobalInfo.prefabPath + item.cardName),
                                          initialPosition,
                                          initialRotation,
                                          cardParent.transform) as GameObject;

            //Debug.Log("이름: " + item.cardName + " 타입: " + item.type + " 값: " + item.value);
        }
    }

    void InitializePlayers()
    {
        players = new GamePlayer[]
        {
            GameObject.Find(GlobalInfo.playerObjectNames[0]).GetComponent<GamePlayer>(),
            GameObject.Find(GlobalInfo.playerObjectNames[1]).GetComponent<GamePlayer>(),
            GameObject.Find(GlobalInfo.playerObjectNames[2]).GetComponent<GamePlayer>(),
            GameObject.Find(GlobalInfo.playerObjectNames[3]).GetComponent<GamePlayer>()
        };
    }

    void ShuffleCards(ref List<Card> cardList)
    {
        RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        cardList = cardList.OrderBy(x => Next(random)).ToList();
    }

    void RenderCards(List<Card> cardList)
    {
        float initialCardPosition = -1.6f;
        float heightFactor = -0.001f;
        float summonPositionX = initialCardPosition;
        float summonPositionY = -2.9f; //-2.9가 적당.
        float summonPositionZ = -1f;
        int idx = 0;
        float cardScaleFactor = 0.2f;
        Vector3 initialCardScale = new Vector3(cardScaleFactor, cardScaleFactor, cardScaleFactor);
        Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);

        foreach (var item in cardList)
        {
            if ((idx - 1) / 7 != idx / 7)
            {
                summonPositionY -= 0.9f;
                summonPositionX = initialCardPosition;
            }
            ++idx;
            item.cardObject.transform.position = new Vector3(summonPositionX, summonPositionY, summonPositionZ);
            item.cardObject.transform.rotation = initialCardRotation;
            item.cardObject.transform.localScale = initialCardScale;
            summonPositionZ += heightFactor;
            summonPositionX += 0.52f;
        }
    }

    static int Next(RNGCryptoServiceProvider random)
    {
        byte[] randomInt = new byte[4];
        random.GetBytes(randomInt);
        return Convert.ToInt32(randomInt[0]);
    }
}
