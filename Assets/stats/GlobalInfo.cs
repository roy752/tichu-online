using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalInfo
{
    public static int numberOfCardsGeneral = 13;
    public static int numberOfCardsSpecial = 4;
    public static int numberOfCardsInitial = 8;
    public static int numberOfCardsPlay = 14;

    public static int[] specialCardsValue = new int[]{ 1, 15, 15, 16 }; //새 개 봉 용 순서로
    public static int aceCardsValue = 14;

    public static string prefabPath = "Prefab/Cards/";

    public static Vector3 hiddenCardPosition = new Vector3(-100f, -100f, -100f);

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
