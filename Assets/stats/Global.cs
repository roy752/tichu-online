using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
public static class Global
{
    public static int numberOfGeneralCardType       = 4;
    public static int numberOfSpecialCardType       = 4;
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

    public static int[]    generalCardsValue        = new int[] { 0, 14, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
    public static int[]    specialCardsValue        = new int[]{ 1, 16, 17, 18 }; //�� �� �� �� ������
    public static string[] valueToStrTable          = new string[] {"0","1","2","3","4","5","6","7","8","9","10", "J", "Q", "K", "A" };
    public static int   aceCardsValue               = 14;
    
    public static int   invalidTrickValue           = -1;


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
    public static string   playerInfoTrickTextName   = "PlayerTrick";

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

    public static string trickSelectionObjectName           = "TrickSelection";
    public static string trickSelectionBombButtonName       = "BombButton";
    public static string trickSelectionSubmitButtonName     = "SubmitButton";
    public static string trickSelectionPassButtonName       = "PassButton";
    public static string trickSelectionSmallTichuButtonName = "SmallTichuButton";

    public static float width   = 3.5f;
    public static float offsetY = 0.75f;
    public static float offsetZ = 0.002f;
    public static float cameraPosition = -10f;


    public static float    trickCardOffset     = 0.4f;

    public static Vector3    frontEpsilon        = new Vector3(0, 0, -0.001f);
    public static Vector3    hiddenCardPosition  = new Vector3(-100f, -100f, -100f);
    public static Vector3    initialPosition     = new Vector3(0f, -2.4f, -1f);
    public static Vector3    initialScale        = new Vector3(0.2f,0.2f, 0.2f);
    public static Quaternion initialCardRotation = Quaternion.Euler(270f, 180f, 180f);

    public static Vector3 initialTrickPosition   = new Vector3(0f, 1.7f, -1f);

    public static string largeTichuInfo       = "���� Ƽ�� ���θ� �����ϼ���.";
    public static string exchangeCardInfo     = "ī�带 ���徿 �����ּ���.";
    public static string receiveCardInfo      = "ī�带 ��������.";
    public static string selectTrickInfo      = "Ʈ���� �����ϼ���.";
    public static string selectBombInfo       = "Ʈ���� �������ϴ�. ��ź�� ���� �����ϼ���.";
    public static string takeTrickInfo        = " �� Ʈ���� �������ϴ�.";

    public static string passInfo                 = "�н�";
    public static string isNotTrickInfo           = "Ʈ���� �ƴմϴ�.";
    public static string singleTrickInfo          = "�̱�";
    public static string pairTrickInfo            = "���";
    public static string consecutivePairTrickInfo = "���� ���";
    public static string tripleTrickInfo          = "Ʈ����";
    public static string fullHouseTrickInfo       = "Ǯ�Ͽ콺";
    public static string straightTrickInfo        = "��Ʈ����Ʈ";
    public static string fourCardTrickInfo        = "��ī�� ��ź";
    public static string straightFlushTrickInfo   = "��Ʈ����Ʈ �÷��� ��ź";

    public static string slotSelectErrorMsg   = "ī�带 ��� �������� �ʾҽ��ϴ�.";
    public static string trickSelectErrorMsg  = "�� Ʈ���� �� �� �����ϴ�.";

    public static string alertLargeTichuMsg   = "���� ���� Ƽ�� �����Ͻðڽ��ϱ�?";
    public static string alertSmallTichuMsg   = "���� ���� Ƽ�� �����Ͻðڽ��ϱ�?";

    public static float largeTichuDuration    = 20.5f;
    public static float exchangeCardsDuration = 30.5f;
    public static float massageDuration       = 1.5f;
    public static float shakeDuration         = 0.15f;
    public static float receiveCardDuration   = 15.5f;
    public static float selectTrickDuration   = 40.5f;
    public static float selectBombDuration    = 5.5f;
    public static float trickTakeDuration     = 3.5f;
    
    public static float tick      = 0.1f;
    public static float shakeTick = 1 / 60f;
    
    public static float shakeSpeedX   = 47f;
    public static float shakeSpeedY   = 26f;
    public static float shakeAmountX  = 0.05f;
    public static float shakeAmountY  = 0.01f;


    public static Color massageColor = new Color(1f, 0, 0, 1f);

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
        public TMP_Text   trick;
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

    public struct TrickSelection
    {
        public GameObject trickSelectionObject;
        public Button bombButton;
        public Button submitButton;
        public Button passButton;
        public Button smallTichuButton;
    }

    public class Trick
    {
        public List<Card> cards;
        public int trickLength;
        public int trickValue;
        public TrickType trickType;

        public Trick(List<Card> inputCards)
        {
            cards = new List<Card>();
            cards.AddRange(inputCards);
        }
    }
    

    public enum CardType
    {
        Bean,
        Flower,
        Shu,
        Moon,
        Bird,
        Dog,
        Phoenix,
        Dragon
    }

    public enum TrickType
    {
        Blank,
        IsNotTrick,
        Dog,
        Single,
        Pair,
        ConsecutivePair,
        Triple,
        FullHouse,
        Straight,
        FourCardBomb,
        StraightFlushBomb
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

    static public void SortCard(ref List<Card> cardList)
    {
        cardList = cardList.OrderBy(x => x.value).ToList();
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

    static public Trick MakeTrick(List<Card> cardList) //cardList �� ���ĵǾ������� ����.
    {
        int length = cardList.Count;
        Trick nowTrick = null;
        if (length == 0) nowTrick = WriteTrick(cardList, 0, invalidTrickValue, TrickType.Blank);
        else if (length == 1)
        {
            //�̱��� ��찡 ����.
            if (cardList[0].type == CardType.Phoenix)
            {
                if (cardList[0].value == invalidTrickValue) nowTrick = WriteTrick(cardList, 1, invalidTrickValue, TrickType.IsNotTrick);
                else nowTrick = WriteTrick(cardList, 1, cardList[0].value * 2 + 1, TrickType.Single); //��Ȳ�� ��� 2�迡 + 1.
            }
            else if(cardList[0].type == CardType.Dog)
            {
                nowTrick = WriteTrick(cardList, 1, cardList[0].value * 2, TrickType.Dog);
            }
            else nowTrick = WriteTrick(cardList, 1, cardList[0].value * 2, TrickType.Single);//�Ϲ� �̱��� ��� 2�踦 ���ϴ� ��ũ��.
        }
        else if (length == 2)
        {
            //����� ��찡 ����.
            if (cardList.All(x => x.value == cardList[0].value)) nowTrick = WriteTrick(cardList, 2, cardList[0].value, TrickType.Pair);
            else nowTrick = WriteTrick(cardList, 2, invalidTrickValue, TrickType.IsNotTrick);
        }
        else if (length == 3)
        {
            //Ʈ������ ��찡 ����.
            if (cardList.All(x => x.value == cardList[0].value)) nowTrick = WriteTrick(cardList, 3, cardList[0].value, TrickType.Triple);
            else nowTrick = WriteTrick(cardList, 3, invalidTrickValue, TrickType.IsNotTrick);
        }
        else if (length == 4)
        {
            //��ī�� ��ź, ���� ����� ��찡 ����.
            if (cardList.All(x => x.value == cardList[0].value)&&FindPhoenix(cardList)==null) nowTrick = WriteTrick(cardList, 4, cardList[0].value, TrickType.FourCardBomb);
            else
            {
                if (cardList[0].value == cardList[1].value && cardList[2].value == cardList[3].value && cardList[0].value + 1 == cardList[2].value)
                {
                    nowTrick = WriteTrick(cardList, 4, cardList[2].value, TrickType.ConsecutivePair);
                }
                else
                {
                    nowTrick = WriteTrick(cardList, 4, invalidTrickValue, TrickType.IsNotTrick);
                }
            }
        }
        else if (length == 5)
        {
            //Ǯ�Ͽ콺, ��Ʈ����Ʈ, ���� ��ź�� ��찡 ����.
            var frontDouble = cardList.Take(2).ToList();
            var backTriple = cardList.Skip(2).ToList();
            if(frontDouble.All(x=>x.value==frontDouble[0].value)&&backTriple.All(x=>x.value==backTriple[0].value))
            {
                nowTrick = WriteTrick(cardList, 5, backTriple[0].value, TrickType.FullHouse); // �ڰ� Ʈ����
            }
            else
            {
                var frontTriple = cardList.Take(3).ToList();
                var  backDouble = cardList.Skip(3).ToList();

                if(frontTriple.All(x=>x.value==frontTriple[0].value)&&backDouble.All(x=>x.value==backDouble[0].value))
                {
                    nowTrick = WriteTrick(cardList, 5, frontTriple[0].value, TrickType.FullHouse); //���� Ʈ����
                }
                else
                {
                    //Ǯ�Ͽ콺�� �ƴ�. ��Ʈ����Ʈ, ���� ��ź �׽�Ʈ.
                    if (IsStraight(cardList))
                    {
                        if (cardList.All(x => x.type == cardList[0].type)&&FindPhoenix(cardList)==null) nowTrick = WriteTrick(cardList, 5, cardList.Last().value, TrickType.StraightFlushBomb);
                        else nowTrick = WriteTrick(cardList, 5, cardList.Last().value, TrickType.Straight);
                    }
                    else nowTrick = WriteTrick(cardList, 5, invalidTrickValue, TrickType.IsNotTrick);
                }
            }
        }
        else
        {
            //���� ��ź, ��Ʈ����Ʈ, ���� ����� ��찡 ����.
            //���� ��ź, ��Ʈ����Ʈ
            if(IsStraight(cardList))
            {
                if (cardList.All(x => x.type == cardList[0].type)) nowTrick = WriteTrick(cardList, cardList.Count(), cardList.Last().value, TrickType.StraightFlushBomb);
                else nowTrick = WriteTrick(cardList, cardList.Count(), cardList.Last().value, TrickType.Straight);
            }
            else //�������
            {
                var oddCardList = cardList.Where((x, index) => index % 2 != 0).ToList();
                var evenCardList = cardList.Where((x, index) => index % 2 == 0).ToList();
                if (cardList.Count()%2==0&&IsStraight(oddCardList)&&IsStraight(evenCardList)) //¦�� ������ ī���̰�,¦Ȧī�� ����Ʈ�� ���� �����̶�� ���� ���.
                {
                    nowTrick = WriteTrick(cardList, cardList.Count(), cardList.Last().value, TrickType.ConsecutivePair);
                }
                else nowTrick = WriteTrick(cardList, cardList.Count(), invalidTrickValue, TrickType.IsNotTrick);
            }
        }
        return nowTrick;
    }

    static Trick WriteTrick(List<Card> cardList, int length, int value, TrickType type)
    {
        Trick nowTrick = new Trick(cardList);
        nowTrick.trickLength = length;
        nowTrick.trickType = type;
        nowTrick.trickValue = value;
        return nowTrick;
    }
    static bool IsStraight(List<Card> cardList)
    {
        int val = cardList[0].value;
        bool flag = true;
        foreach(var card in cardList)
        {
            if(card.value!=val) { flag = false; break; }
            ++val;
        }
        return flag;
    }

    static public int? FindPhoenix(List<Card> cardList)
    {
        int? ret = null;
        for(int idx = 0; idx<cardList.Count(); ++idx)
        {
            if(cardList[idx].type == CardType.Phoenix)
            {
                ret = idx;
                break;
            }
        }
        return ret;
    }

    static public void EstimatePhoenix(List<Card> cardList, int? phoenixIdx)
    {
        if (phoenixIdx == null) return;
        else
        {
            if (cardList.Count == 1) 
            {
                if (GameManager.instance.isFirstTrick) { cardList[(int)phoenixIdx].value = 1; return; }
                else
                {
                    if (GameManager.instance.trickStack.Peek().trickType==TrickType.Single&&GameManager.instance.trickStack.Peek().cards[0].value != specialCardsValue[3])
                    {
                        cardList[(int)phoenixIdx].value = GameManager.instance.trickStack.Peek().cards[0].value;
                        return;
                    }
                    else
                    {
                        cardList[(int)phoenixIdx].value = invalidTrickValue;
                        return;
                    }
                }
            }
            int retVal = 2;
            Card phoenix = cardList[(int)phoenixIdx];
            phoenix.value = 2;
            Trick retTrick = MakeTrick(cardList);
            for(int val = 3; val<=14; ++val)
            {
                phoenix.value = val;
                SortCard(ref cardList);
                Trick nowTrick = MakeTrick(cardList);
                if (retTrick.trickType == TrickType.FourCardBomb || retTrick.trickType == TrickType.StraightFlushBomb) continue;
                if ((int)retTrick.trickType < (int)nowTrick.trickType) retVal = val;
                else if (retTrick.trickType == nowTrick.trickType && retTrick.trickValue < nowTrick.trickValue) retVal = val;
                else continue;
            }
            phoenix.value = retVal;
        }
    }

    static public string GetTrickInfo(Trick trick)
    {
        string retInfo = null;
        switch (trick.trickType)
        {
            case TrickType.Single:
                switch (trick.cards[0].type)
                {
                    case CardType.Phoenix: retInfo = valueToStrTable[trick.cards[0].value] + ".5 " + singleTrickInfo; break;
                    case CardType.Bird: retInfo = valueToStrTable[trick.cards[0].value] + " " + singleTrickInfo + "(��)"; break;
                    case CardType.Dragon: retInfo = "��"; break;
                    case CardType.Dog: retInfo = "��"; break;
                    default: retInfo = valueToStrTable[trick.trickValue / 2] + " " + singleTrickInfo; break;
                }
                break;
            case TrickType.Blank:             retInfo = selectTrickInfo; break;
            case TrickType.IsNotTrick:        retInfo = isNotTrickInfo; break;
            case TrickType.Pair:              retInfo = valueToStrTable[trick.trickValue] + " " + pairTrickInfo; break;
            case TrickType.Triple:            retInfo = valueToStrTable[trick.trickValue] + " " + tripleTrickInfo; break;
            case TrickType.ConsecutivePair:   retInfo = valueToStrTable[trick.trickValue] + " " + consecutivePairTrickInfo; break;
            case TrickType.Straight:          retInfo = valueToStrTable[trick.trickValue] + " " + straightTrickInfo; break;
            case TrickType.FullHouse:         retInfo = valueToStrTable[trick.trickValue] + " " + fullHouseTrickInfo; break;
            case TrickType.FourCardBomb:      retInfo = valueToStrTable[trick.trickValue] + " " + fourCardTrickInfo; break;
            case TrickType.StraightFlushBomb: retInfo = valueToStrTable[trick.trickValue] + " " + straightFlushTrickInfo; break;
        }
        return retInfo;
    }
    static public string GetTrickTakeInfo(string playerName)
    {
        return playerName + takeTrickInfo;
    }
}
