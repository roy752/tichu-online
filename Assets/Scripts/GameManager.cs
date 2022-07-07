using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Security.Cryptography;
using UnityEngine.EventSystems;

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

    [HideInInspector]
    public GamePlayer currentPlayer;
    [HideInInspector]
    public Card currentCard;

    public static GameManager instance;

    [HideInInspector]
    public GameObject cardsParent;

    [HideInInspector]
    public bool phaseChangeFlag;

    [HideInInspector]
    public bool isMultipleSelectionAllowed;

    private int splitCardIdx = 0;

    private void Awake()
    {
        MakeCardNameList();
        InitializeVariables();
        InstantiateCards();
        InitializePlayers();
        ShuffleCards(ref cards);

        instance = this;
    }

    void Start()
    {
        StartCoroutine(StartPlay());
    }

    private void Update()
    {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            Vector3 touchPos = Input.GetTouch(0).position;//Camera.main.ScreenToWorldPoint(Input.mousePosition); //Input.GetTouch(0);
            Ray ray = new Ray(new Vector3(touchPos.x, touchPos.y, -10f), Vector3.forward);
            RaycastHit hitInformation;
            Physics.Raycast(ray,out hitInformation);
            if (hitInformation.collider != null)
            {
                GameObject touchedObject = hitInformation.transform.gameObject;

                CardSelectHandler selectHandler = touchedObject.GetComponent<CardSelectHandler>();
                if (selectHandler != null)
                {
                    if (isMultipleSelectionAllowed)
                    {

                    }
                    else
                    {
                        selectHandler.ToggleSelection();
                        if (currentCard != null)
                        {
                            if (currentCard.cardObject == touchedObject) currentCard = null;
                            else
                            {
                                currentCard.cardObject.GetComponent<CardSelectHandler>().ToggleSelection();
                                SetCurrentCard(touchedObject);
                            }
                        }
                        else SetCurrentCard(touchedObject);
                    }
                }
            }
            else Debug.Log("what");
        }
    }

    IEnumerator StartPlay()
    {
        SplitCardsToPlayer(GlobalInfo.numberOfCardsLargeTichuPhase);
        StartCoroutine(StartLargeTichuPhaseCoroutine());
        yield return new WaitUntil(()=>phaseChangeFlag);
        phaseChangeFlag = false;

        SplitCardsToPlayer(GlobalInfo.numberOfCardsSmallTichuPhase);
        //StartCoroutine(StartExchangeCardPhaseCoroutine());
        //yield return new WaitUntil(() => phaseChangeFlag);
        //phaseChangeFlag = false;
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
    
    IEnumerator StartExchangeCardPhaseCoroutine()
    {
        foreach(var player in players)
        {
            currentPlayer = player;
            player.ExchangeCards();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }
    }

    void SplitCardsToPlayer(int num)
    {
        foreach(var player in players)
        {
            player.AddCards(cards.GetRange(splitCardIdx, num));
            splitCardIdx += num;
        }
    }

    void MakeCardNameList()
    {
        int type = 0;
        int id = 0;

        foreach (string cardName in Enum.GetNames(typeof(GlobalInfo.GeneralCardName)))
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
        foreach (string cardName in Enum.GetNames(typeof(GlobalInfo.SpecialCardName)))
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
            item.cardObject.name = item.cardName;
            item.cardObject.transform.rotation   = GlobalInfo.initialCardRotation;
            item.cardObject.transform.localScale = GlobalInfo.initialScale;

            GameObject edgeObject = Instantiate(Resources.Load("Prefab/etc/EdgePanel"),item.cardObject.transform) as GameObject;
            edgeObject.name = GlobalInfo.cardEdgeObjectName;
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
        int num = 0;
        foreach (var player in players)
        {
            
            player.playerNumber = num;
            player.playerName = "Player" + num.ToString();
        }
    }

    void InitializeVariables()
    {
        cardsParent = GameObject.Find(GlobalInfo.cardsParentObjectName);
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

    public void SetCurrentCard(GameObject inputObject)
    {
        foreach(var item in cards)
        {
            if(item.cardObject==inputObject)
            {
                currentCard = item;
                break;
            }
        }
    }
}
