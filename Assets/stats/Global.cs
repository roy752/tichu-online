using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
public static class Global
{
    public static int numberOfCardsGeneral          = 13;
    public static int numberOfCardsSpecial          = 4;
    public static int numberOfCardsLargeTichuPhase  = 8;
    public static int numberOfCardsSmallTichuPhase  = 6;
    public static int numberOfCardsPlay             = 14;
    public static int numberOfPlayers               = 4;
    public static int numberOfSlots                 = 3;

    public static int numberOfCardsForLineInLargeTichuPhase = 4;
    public static int numberOfCardsForLineInSmallTichuPhase = 5;
    public static int numberOfCardsForLineInPlayPhase       = 14;

    public static int[] generalCardsValue           = new int[] { 0, 14, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
    public static int[] specialCardsValue           = new int[]{ 1, 15, 16, 17 }; //새 개 봉 용 순서로
    public static int   aceCardsValue               = 14;


    public static string prefabPath                 = "Prefab/Cards/";


    public static string uiParentObjectName         = "GameUI";
    public static string cardsParentObjectName      = "Cards";

    public static string[] playerObjectNames        = new string[] { "Player0", "Player1", "Player2", "Player3" };
    public static string[] playerNames              = new string[] { "Alice", "Bob", "Charles", "Dan" };
    
    public static string timerObjectName            = "Timer";
    public static string edgeObjectName             = "EdgePanel";
    
    public static string infoBarObjectName            = "InfoBar";
    public static string infoBarTextObjectName        = "InfoContent";

    public static string largeTichuButtonObjectName   = "LargeTichuButton";
    public static string largeTichuDeclareButtonName  = "LargeTichuDeclareButton";
    public static string largeTichuSkipButtonName     = "LargeTichuSkipButton";

    public static string[] exchangeCardSlotObjectName   = new string[] { "CardExchangeSlot0", "CardExchangeSlot1", "CardExchangeSlot2" };
    public static string   exchangeCardObjectName       = "CardExchangePopup";
    public static string   exchangeCardButtonObjectName = "CardExchangeButton";
    public static string   exchangeplayerObjectName     = "ExchangePlayerName";
    public static string   exchangeCardSmallTichuButtonObjectName = "SmallTichuButton";

    public static string   playerInfoObjectName      = "PlayerInfo";
    public static string[] playerInfoObjectNames     = new string[] { "PlayerInfo0", "PlayerInfo1", "PlayerInfo2", "PlayerInfo3" };
    public static string   playerInfoNameObjectName  = "PlayerName";
    public static string   playerInfoHandObjectName  = "PlayerHand";
    public static string   playerInfoHandName        = "PlayerHandNumber";
    public static string   playerInfoTichuObjectName = "PlayerTichu";
    public static string   playerInfoLargeTichuName  = "LargeTichu";
    public static string   playerInfoSmallTichuName  = "SmallTichu";
    public static string   playerInfoScoreObjectName = "PlayerScore";
    public static string   playerInfoRoundScoreName  = "RoundScore";
    public static string   playerInfoTotalScoreName  = "TotalScore";

    public static string alertPopupObjectName         = "AlertPopup";
    public static string alertTextObjectName          = "AlertContent";
    public static string alertConfirmButtonObjectName = "ConfirmButton";
    public static string alertCancelButtonObjectName  = "CancelButton";

    public static string   cardReceivePopupObjectName            = "CardReceivePopup";
    public static string[] cardReceiveSlotObjectNames            = new string[] { "CardReceiveSlot0", "CardReceiveSlot1", "CardReceiveSlot2" };
    public static string   cardReceiveButtonObjectName           = "CardReceiveButton";
    public static string   cardReceiveSmallTichuButtonObjectName = "SmallTichuButton";
    public static string   cardReceivePlayerInfoObjectName       = "CardGiverInfo";
    public static string   cardReceivePlayerNameObjectName       = "PlayerName";


    public static float width = 3.5f;
    public static float offsetY = 0.75f;
    public static float offsetZ = 0.002f;
    public static float cameraPosition = -10f;


    public static Vector3    frontEpsilon        = new Vector3(0, 0, -0.001f);
    public static Vector3    hiddenCardPosition  = new Vector3(-100f, -100f, -100f);
    public static Vector3    initialPosition     = new Vector3(0f, -3.9f, -1f);
    public static Vector3    initialScale        = new Vector3(0.2f,0.2f, 0.2f);
    public static Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);

    public static string largeTichuInfo     = "라지 티츄 여부를 결정하세요.";
    public static string exchangeCardInfo   = "카드를 한장씩 나눠주세요.";
    public static string receiveCardInfo    = "카드를 받으세요.";
    
    public static string SlotSelectErrorMsg = "카드를 모두 나눠주지 않았습니다.";

    public static string alertLargeTichuMsg = "정말 라지 티츄를 선언하시겠습니까?";
    public static string alertSmallTichuMsg = "정말 스몰 티츄를 선언하시겠습니까?";

    public static float largeTichuDuration    = 20.5f;
    public static float exchangeCardsDuration = 30.5f;
    public static float massageDuration       = 1.5f;
    public static float shakeDuration         = 0.15f;
    public static float receiveCardDuration   = 15.5f;
    
    public static float tick      = 0.1f;
    public static float shakeTick = 1 / 60f;
    
    public static float shakeSpeedX   = 47f;
    public static float shakeSpeedY   = 26f;
    public static float shakeAmountX  = 0.05f;
    public static float shakeAmountY  = 0.01f;


    public static Color massageColor = new Color(1f, 0, 0, 1f);


    public class Card
    {
        public GameObject cardObject;
        public string     cardName;
        public int        value;
        public int        type;
        public int        id;
        public bool       isFixed;
    }

    public struct LargeTichu
    {
        public GameObject largeTichuObject;
        public Button     declareButton;
        public Button     skipButton;
    }

    public struct InfoBar
    {
        public GameObject infoBarObject;
        public TMP_Text   infoBarText;
    }

    public struct Timer
    {
        public GameObject timerObject;
        public TMP_Text   timerText;
    }

    public struct ExchangeCardSlot
    {
        public TMP_Text          playerText;
        public SlotSelectHandler slot;
        public GamePlayer        player;
    }

    public struct ExchangeCardPopup
    {
        public GameObject         exchangeCardPopupObject;
        public Button             exchangeCardButton;
        public Button             smallTichuButton;
        public ExchangeCardSlot[] slots;
    }

    public struct PlayerInfo
    {
        public GameObject playerInfoObject;
        public TMP_Text   name;
        public TMP_Text   hand;
        public GameObject largeTichuIconObject;
        public GameObject smallTichuIconObject;
        public TMP_Text   roundScore;
        public TMP_Text   totalScore;
    }

    public struct PlayerInfoUI
    {
        public GameObject   playerInfoObject;
        public PlayerInfo[] playerInfo; 
    }

    public struct AlertPopup
    {
        public GameObject alertPopupObject;
        public TMP_Text   alertText;
        public Button     alertConfirmButton;
        public Button     alertCancelButton;
    }

    public struct CardReceiveSlot
    {
        public GameObject slotObject;
        public GameObject InfoObject;
        public TMP_Text   playerNameText;
    }

    public struct CardReceivePopup
    {
        public GameObject        cardReceiveObject;
        public CardReceiveSlot[] cardReceiveSlots;
        public Button            cardReceiveButton;
        public Button            smallTichuButton;
    }

    public struct PlayerReceiveCardSlot
    {
        public GamePlayer player;
        public Card card;
    }

    public enum GeneralCardName
    {
        Bean,
        Flower,
        Shu,
        Moon
    }
    public enum SpecialCardName
    {
        Bird = 4,
        Dog,
        Phoenix,
        Dragon
    }

    static public int Next(RNGCryptoServiceProvider random)
    {
        byte[] randomInt = new byte[4];
        random.GetBytes(randomInt);
        return Convert.ToInt32(randomInt[0]);
    }
    static public GameObject GetHitObject(Vector3 inputPosition)
    {
        Ray ray = new Ray(new Vector3(inputPosition.x, inputPosition.y, Global.cameraPosition), Vector3.forward);
        RaycastHit hitInformation;
        Physics.Raycast(ray, out hitInformation);
        if (hitInformation.collider != null) return hitInformation.transform.gameObject;
        else return null;
    }

    static public void ShuffleCards(ref List<Card> cardList)
    {
        RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        cardList = cardList.OrderBy(x => Next(random)).ToList();
    }

    static public string GetCardName(string cardTypeName, int cardNumber)
    {
        return cardTypeName + cardNumber.ToString("D2");
    }
    static public int GetCardGiverIdx(GamePlayer giver, GamePlayer receiver)
    {
        int giverIdx = giver.playerNumber;
        int receiverIdx = receiver.playerNumber;
        if (giverIdx < receiverIdx) giverIdx += numberOfPlayers;
        return giverIdx - receiverIdx - 1;
    }
}
