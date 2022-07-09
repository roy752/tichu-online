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
        public string     cardName;
        public int        value;
        public int        type;
        public int        id;
        public bool       isFixed;
    }

    [HideInInspector]
    public GamePlayer[] players;

    [HideInInspector]
    public GamePlayer currentPlayer;
    [HideInInspector]
    public Card currentCard;
    [HideInInspector]
    public SlotSelectHandler currentSlot;

    public static GameManager instance;

    [HideInInspector]
    public GameObject cardsParent;

    [HideInInspector]
    public bool phaseChangeFlag;

    [HideInInspector]
    public bool isMultipleSelectionAllowed;

    [HideInInspector]
    public bool isSelectionEnabled;

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
        HandleSelection();
    }

    IEnumerator StartPlay()
    {
        SplitCardsToPlayer(GlobalInfo.numberOfCardsLargeTichuPhase);
        StartCoroutine(StartLargeTichuPhaseCoroutine());
        yield return new WaitUntil(()=>phaseChangeFlag);

        SplitCardsToPlayer(GlobalInfo.numberOfCardsSmallTichuPhase);
        StartCoroutine(StartExchangeCardPhaseCoroutine());
        yield return new WaitUntil(() => phaseChangeFlag);
    }

    IEnumerator StartLargeTichuPhaseCoroutine()
    {
        phaseChangeFlag = false;

        foreach (var player in players)
        {
            currentPlayer = player;
            player.ChooseLargeTichu();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }
        phaseChangeFlag = true;
    }
    
    IEnumerator StartExchangeCardPhaseCoroutine()
    {
        phaseChangeFlag = false;

        isSelectionEnabled = true;
        foreach(var player in players)
        {
            currentPlayer = player;
            player.ExchangeCards();
            yield return new WaitUntil(() => player.coroutineFinishFlag);
        }
        phaseChangeFlag = true;
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
            player.playerName = GlobalInfo.playerObjectNames[num];
            num++;
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
        foreach (var item in cards) if(item.isFixed==false) item.cardObject.transform.position = GlobalInfo.hiddenCardPosition;

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
    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0) && isSelectionEnabled)
        {
            GameObject hitObject = GetHitObject( Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (hitObject != null) hitObject.GetComponent<SelectionHandler>().ToggleSelection();
        }
    }

    private GameObject GetHitObject(Vector3 inputPosition)
    {
        Ray ray = new Ray(new Vector3(inputPosition.x, inputPosition.y, GlobalInfo.cameraPosition), Vector3.forward);
        RaycastHit hitInformation;
        Physics.Raycast(ray, out hitInformation);
        if (hitInformation.collider != null) return hitInformation.transform.gameObject;
        else return null;
    }
}
