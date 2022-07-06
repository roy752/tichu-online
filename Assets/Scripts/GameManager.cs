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
    /*
    IEnumerator RenderTestCoroutine()
    {
        int cnt = GlobalInfo.numberOfPlayers;
        while(cnt-->0)
        {
            RenderCards(players[cnt].cards.OrderBy(x => x.value).ToList());
            yield return new WaitForSeconds(3.0f);
        }
    }
    */
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

            item.cardObject.transform.rotation   = GlobalInfo.initialCardRotation;
            item.cardObject.transform.localScale = GlobalInfo.initialScale;
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
    
    public void RenderCards(Vector3 centerPosition, int numberOfCardsForLine, List<Card> cardList) 
    {
        foreach (var item in cards) item.cardObject.transform.position = GlobalInfo.hiddenCardPosition;

        float offsetX = GlobalInfo.width / (numberOfCardsForLine - 1);
        float offsetY = GlobalInfo.offsetY;
        float offsetZ = GlobalInfo.offsetZ;

        Vector3 initialPosition = centerPosition + new Vector3(-offsetX * ((float)(numberOfCardsForLine - 1) / 2f), offsetY*((cardList.Count-1)/numberOfCardsForLine), 0);

        Vector3 pos = Vector3.zero;

        int cnt = 0;
        foreach(var item in cardList)
        {
            if(cnt==numberOfCardsForLine)
            {
                pos.x = 0;
                pos.y -= offsetY;
                cnt = 0;
            }

            item.cardObject.transform.position = initialPosition + pos;
            pos.x += offsetX;
            pos.z -= offsetZ;
            ++cnt;
        }

    }

    static int Next(RNGCryptoServiceProvider random)
    {
        byte[] randomInt = new byte[4];
        random.GetBytes(randomInt);
        return Convert.ToInt32(randomInt[0]);
    }
}
