using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static string[] playerObjectNames = new string[] { "Player1", "Player2", "Player3", "Player4" };

    public static Vector3 hiddenCardPosition = new Vector3(-100f, -100f, -100f);

    public static int numberOfPlayers = 4;

    public static float largeTichuDuration = 20.5f;

    public static float tick = 0.1f;

    public static string uiParentObjectName = "GameUI";
    public static string cardsParentObjectName = "Cards";
    public static string infoBarObjectName = "InfoBar";

    public static float width = 3.5f;
    public static float offsetY = 0.9f;
    public static float offsetZ = 0.001f;


    public static Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);
    public static Vector3 initialPosition = new Vector3(0f, -3.9f, -1f);
    public static Vector3 initialScale = new Vector3(0.2f,0.2f, 0.2f);

    public struct ButtonNames
    {
        public string ltYesButtonName;
        public string ltNoButtonName;
    }

    public static ButtonNames buttons = new ButtonNames
                                     {                                     
                                       ltYesButtonName = "LargeTichuDeclareButton",
                                       ltNoButtonName  = "LargeTichuSkipButton"
                                     };

    public static string largeTichuInfo = "라지 티츄 여부를 결정하세요.";
    public static string exchangeCardInfo = "카드를 한장씩 나눠주세요.";

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
