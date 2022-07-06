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

    public static GameManager instance;

    public GameObject cardsParent;
    public GameObject uiParent;

    public bool phaseChangeFlag;

    private void Awake()
    {
        MakeCardNameList();
    }

    void Start()
    {
        InitializeVariables();
        InstantiateCards();
        InitializePlayers();

        ShuffleCards(ref cards);

        instance = this;

        StartCoroutine(StartPlay());
    }

    IEnumerator StartPlay()
    {
        SplitCardsToPlayer(GlobalInfo.numberOfCardsLargeTichuPhase);
        StartCoroutine(StartLargeTichuPhaseCoroutine());
        yield return new WaitUntil(()=>phaseChangeFlag);
        phaseChangeFlag = false;

        //SplitCardsToPlayer(GlobalInfo.numberOfCardsSmallTichuPhase);
    }

    IEnumerator StartLargeTichuPhaseCoroutine()
    {
        foreach (var player in players)
        {
            player.ChooseLargeTichu();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }
        phaseChangeFlag = true;
    }

    IEnumerator RenderTestCoroutine()
    {
        int cnt = GlobalInfo.numberOfPlayers;
        while(cnt-->0)
        {
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

        foreach (var item in cards)
        {
            item.cardObject = Instantiate(Resources.Load(GlobalInfo.prefabPath + item.cardName),
                                          initialPosition,
                                          initialRotation,
                                          cardsParent.transform) as GameObject;

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

    void InitializeVariables()
    {
        cardsParent = GameObject.Find(GlobalInfo.cardsParentObjectName);
        uiParent    = GameObject.Find(GlobalInfo.uiParentObjectName);
    }

    void ShuffleCards(ref List<Card> cardList)
    {
        RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        cardList = cardList.OrderBy(x => Next(random)).ToList();
    }
    
    public void RenderCards(List<Card> cardList) //중심좌표, 한 줄당 렌더링할 카드 개수, 카드 리스트
    {
        foreach (var item in cards) item.cardObject.transform.position = GlobalInfo.hiddenCardPosition;

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
