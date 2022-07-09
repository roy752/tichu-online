using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
public static class GlobalInfo
{
    public static int numberOfCardsGeneral = 13;
    public static int numberOfCardsSpecial = 4;
    public static int numberOfCardsLargeTichuPhase = 8;
    public static int numberOfCardsSmallTichuPhase = 6;
    public static int numberOfCardsPlay = 14;

    public static int[] specialCardsValue = new int[]{ 1, 15, 15, 16 }; //새 개 봉 용 순서로
    public static int aceCardsValue = 14;

    public static string prefabPath = "Prefab/Cards/";

    public static string[] playerObjectNames = new string[] { "Player0", "Player1", "Player2", "Player3" };

    public static Vector3 hiddenCardPosition = new Vector3(-100f, -100f, -100f);

    public static int numberOfPlayers = 4;

    public static float largeTichuDuration = 20.5f;
    public static float exchangeCardsDuration = 30.5f;

    public static float tick = 0.1f;

    public static string uiParentObjectName = "GameUI";
    public static string cardsParentObjectName = "Cards";
    public static string infoBarObjectName = "InfoBar";
    public static string exchangeCardObjectName = "CardExchangePopup";
    public static string exchangeCardButtonObjectName = "CardExchangeButton";
    public static string exchangeCardSlotObjectName = "CardExchangeSlot";
    public static string exchangeplayerObjectName = "ExchangePlayerName";
    public static string edgeObjectName = "EdgePanel";
    public static string timerObjectName = "Timer";
    public static string infoBarTextObjectName = "InfoContent";

    public static float width = 3.5f;
    public static float offsetY = 0.75f;
    public static float offsetZ = 0.002f;

    public static float cameraPosition = -10f;

    public static Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);
    public static Vector3 initialPosition = new Vector3(0f, -3.9f, -1f);
    public static Vector3 initialScale = new Vector3(0.2f,0.2f, 0.2f);
    public static Vector3 frontEpsilon = new Vector3(0, 0, -0.001f);

    public static string largeTichuButtonObjectName = "LargeTichuButton";
    public static string largeTichuDeclareButtonName = "LargeTichuDeclareButton";
    public static string largeTichuSkipButtonName = "LargeTichuSkipButton";

    public static string largeTichuInfo = "라지 티츄 여부를 결정하세요.";
    public static string exchangeCardInfo = "카드를 한장씩 나눠주세요.";
    public static string SlotSelectErrorMsg = "카드를 모두 나눠주지 않았습니다.";

    public static float massageDuration = 1.5f;
    public static float shakeTick = 1 / 60f;
    public static float shakeSpeedX = 600f;
    public static float shakeSpeedY = 457f;
    public static float shakeAmountX = 0.05f;
    public static float shakeAmountY = 0.01f;
    public static float shakeDuration = 0.15f;
    public static Color massageColor = new Color(1f, 0, 0, 1f);



    public class Card
    {
        public GameObject cardObject;
        public string cardName;
        public int value;
        public int type;
        public int id;
        public bool isFixed;
    }


    public struct LargeTichu
    {
        public GameObject largeTichuObject;
        public Button declareButton;
        public Button skipButton;
    }

    public struct InfoBar
    {
        public GameObject infoBarObject;
        public TMP_Text infoBarText;
    }

    public struct Timer
    {
        public GameObject timerObject;
        public TMP_Text timerText;
    }

    public struct ExchangeCardSlot
    {
        public TMP_Text playerText;
        public SlotSelectHandler slot;
        public GamePlayer player;
    }

    public struct ExchangeCardPopup
    {
        public GameObject exchangeCardPopupObject;
        public Button exchangeCardButton;
        public ExchangeCardSlot[] slots;
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
}
