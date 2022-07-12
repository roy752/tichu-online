using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UIManager : MonoBehaviour
{
    [HideInInspector]
    public static UIManager instance;


    private GameObject               uiParent;
    private Global.LargeTichu        largeTichu     = new Global.LargeTichu();
    private Global.InfoBar           infoBar        = new Global.InfoBar();
    private Global.Timer             timer          = new Global.Timer();
    public  Global.ExchangeCardPopup exchangeCard   = new Global.ExchangeCardPopup();
    private Global.PlayerInfoUI      playerInfo     = new Global.PlayerInfoUI();
    private Global.AlertPopup        alertPopup     = new Global.AlertPopup();
    private Global.CardReceivePopup  receiveCard    = new Global.CardReceivePopup();
    private Global.TrickSelection    selectTrick    = new Global.TrickSelection();

    private bool isMassaging = false;

    private float timerDuration;

    private void Start()
    {
        InitializeVariables();
        instance = this;
    }

    private void InitializeVariables()
    {
        uiParent = GameObject.Find(Global.uiParentObjectName);

        //라지티츄 오브젝트
        largeTichu.largeTichuObject = uiParent.transform.Find(Global.largeTichuButtonObjectName).gameObject;
        largeTichu.declareButton    = largeTichu.largeTichuObject.transform.Find(Global.largeTichuDeclareButtonName).GetComponent<Button>();
        largeTichu.skipButton       = largeTichu.largeTichuObject.transform.Find(Global.largeTichuSkipButtonName).GetComponent<Button>();
        //////////////////////////////////


        //인포 창 오브젝트
        infoBar.infoBarObject = uiParent.transform.Find(Global.infoBarObjectName).gameObject;
        infoBar.infoBarText   = infoBar.infoBarObject.transform.Find(Global.infoBarTextObjectName).GetComponent<TMP_Text>();
        //////////////////////////////////
        

        //타이머 오브젝트
        timer.timerObject = uiParent.transform.Find(Global.timerObjectName).gameObject;
        timer.timerText   = timer.timerObject.GetComponent<TMP_Text>();
        /////////////////////////////////
        

        //카드 교환 팝업 오브젝트
        exchangeCard.exchangeCardPopupObject = uiParent.transform.Find(Global.exchangeCardObjectName).gameObject;
        exchangeCard.exchangeCardButton      = exchangeCard.exchangeCardPopupObject.transform.Find(Global.exchangeCardButtonObjectName).GetComponent<Button>();
        exchangeCard.smallTichuButton        = exchangeCard.exchangeCardPopupObject.transform.Find(Global.exchangeCardSmallTichuButtonObjectName).GetComponent<Button>();
        exchangeCard.slots = new Global.ExchangeCardSlot[Global.numberOfSlots];
        for (int i = 0; i < Global.numberOfSlots; ++i)
        {
            var nowSlotObject = exchangeCard.exchangeCardPopupObject.transform.Find(Global.exchangeCardSlotObjectName[i]);
            exchangeCard.slots[i].slot       = nowSlotObject.GetComponent<SlotSelectHandler>();
            exchangeCard.slots[i].playerText = nowSlotObject.Find(Global.exchangeplayerObjectName).GetComponent<TMP_Text>();
        }
        /////////////////////////////////


        //플레이어 인포 오브젝트
        playerInfo.playerInfoObject = uiParent.transform.Find(Global.playerInfoObjectName).gameObject;
        playerInfo.playerInfo       = new Global.PlayerInfo[Global.numberOfPlayers];

        for(int idx = 0; idx<playerInfo.playerInfo.Length; ++idx)
        {
            playerInfo.playerInfo[idx].playerInfoObject = playerInfo.playerInfoObject.transform.Find(Global.playerInfoObjectNames[idx]).gameObject;

            var nowInfoObject = playerInfo.playerInfo[idx].playerInfoObject.transform;

            playerInfo.playerInfo[idx].name                 = nowInfoObject.Find(Global.playerInfoNameObjectName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].hand                 = nowInfoObject.Find(Global.playerInfoHandObjectName).Find(Global.playerInfoHandName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].smallTichuIconObject = nowInfoObject.Find(Global.playerInfoTichuObjectName).Find(Global.playerInfoSmallTichuName).gameObject;
            playerInfo.playerInfo[idx].largeTichuIconObject = nowInfoObject.Find(Global.playerInfoTichuObjectName).Find(Global.playerInfoLargeTichuName).gameObject;
            playerInfo.playerInfo[idx].roundScore           = nowInfoObject.Find(Global.playerInfoScoreObjectName).Find(Global.playerInfoRoundScoreName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].totalScore           = nowInfoObject.Find(Global.playerInfoScoreObjectName).Find(Global.playerInfoTotalScoreName).GetComponent<TMP_Text>();
        }
        ///////////////////////////////


        //확인창 팝업(alert popup) 오브젝트
        alertPopup.alertPopupObject = uiParent.transform.Find(Global.alertPopupObjectName).gameObject;

        var nowAlertObject          = alertPopup.alertPopupObject.transform;
        
        alertPopup.alertText          = nowAlertObject.Find(Global.alertTextObjectName).GetComponent<TMP_Text>();
        alertPopup.alertConfirmButton = nowAlertObject.Find(Global.alertConfirmButtonObjectName).GetComponent<Button>();
        alertPopup.alertCancelButton  = nowAlertObject.Find(Global.alertCancelButtonObjectName).GetComponent<Button>();
        //////////////////////


        //카드 받는 팝업(card receive popup) 오브젝트
        receiveCard.cardReceiveObject = uiParent.transform.Find(Global.cardReceivePopupObjectName).gameObject;

        var nowCardReceiveObject = receiveCard.cardReceiveObject.transform;
        
        receiveCard.cardReceiveSlots = new Global.CardReceiveSlot[Global.numberOfSlots];

        receiveCard.cardReceiveButton = nowCardReceiveObject.Find(Global.cardReceiveButtonObjectName).GetComponent<Button>();
        receiveCard.smallTichuButton = nowCardReceiveObject.Find(Global.cardReceiveSmallTichuButtonObjectName).GetComponent<Button>();

        for(int idx = 0; idx<Global.numberOfSlots; ++idx)
        {
            receiveCard.cardReceiveSlots[idx].slotObject     = nowCardReceiveObject.Find(Global.cardReceiveSlotObjectNames[idx]).gameObject;
            receiveCard.cardReceiveSlots[idx].InfoObject     = receiveCard.cardReceiveSlots[idx].slotObject.transform.Find(Global.cardReceivePlayerInfoObjectName).gameObject;
            receiveCard.cardReceiveSlots[idx].playerNameText = receiveCard.cardReceiveSlots[idx].InfoObject.transform.Find(Global.cardReceivePlayerNameObjectName).GetComponent<TMP_Text>();
        }
        /////////////////////////////////////////////////


        //트릭 선택 관련 버튼(trick selection) 오브젝트
        selectTrick.trickSelectionObject = uiParent.transform.Find(Global.trickSelectionObjectName).gameObject;

        var nowTrickSelectionObject     = selectTrick.trickSelectionObject.transform;

        selectTrick.bombButton       = nowTrickSelectionObject.Find(Global.trickSelectionBombButtonName).GetComponent<Button>();
        selectTrick.submitButton     = nowTrickSelectionObject.Find(Global.trickSelectionSubmitButtonName).GetComponent<Button>();
        selectTrick.passButton       = nowTrickSelectionObject.Find(Global.trickSelectionPassButtonName).GetComponent<Button>();
        selectTrick.smallTichuButton = nowTrickSelectionObject.Find(Global.trickSelectionSmallTichuButtonName).GetComponent<Button>();
        ////////////////////////


    }

    public void ActivateLargeTichu(UnityAction DeclareCall, UnityAction SkipCall)
    {

        ShowInfo(Global.largeTichuInfo);
        ActivateTimer(Global.largeTichuDuration);

        largeTichu.largeTichuObject.SetActive(true);

        largeTichu.declareButton.onClick.AddListener(() => ActivateAlertPopup(Global.alertLargeTichuMsg, DeclareCall)); // 람다식, 델리게이트 알아볼 것.
        largeTichu.skipButton.onClick.AddListener(SkipCall);
        
        // btn.onClick.AddListener(() => { Function(param); OtherFunction(param); }); //이런 코드도 동작함.
        // largeTichu.declareButton.onClick.AddListener(delegate{ ActivateAlertPopup(Global.alertLargeTichuMsg, DeclareCall); }); //이런 코드도 동작함.
    }

    public void DeactivateLargeTichu()
    {
        DeactivateAlertPopup();
        DeactivateTimer();

        largeTichu.declareButton.onClick.RemoveAllListeners();
        largeTichu.skipButton.onClick.RemoveAllListeners();

        largeTichu.largeTichuObject.SetActive(false);
        HideInfo();
    }

    public void ActivateExchangeCardsPopup(UnityAction exchangeCall, UnityAction declareCall)
    {
        ShowInfo(Global.exchangeCardInfo);
        ActivateTimer(Global.exchangeCardsDuration);
        exchangeCard.exchangeCardPopupObject.SetActive(true);
        WritePlayerNameToSlot();
        exchangeCard.exchangeCardButton.onClick.AddListener(exchangeCall);
        exchangeCard.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Global.alertSmallTichuMsg,
                                                                                () => { declareCall(); UpdateSmallTichuButton(exchangeCard.smallTichuButton.gameObject); }
                                                                              )
                                                        );
        exchangeCard.smallTichuButton.gameObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요. 버튼을 enabled = false 로 하고 흐리게 만들어야함.
    }


    public void DeactivateExchangeCardsPopup()
    {
        exchangeCard.exchangeCardButton.onClick.RemoveAllListeners();
        exchangeCard.smallTichuButton.onClick.RemoveAllListeners();
        DeactivateAlertPopup();
        DeactivateTimer();

        FlushCard();
        exchangeCard.exchangeCardPopupObject.SetActive(false);
        HideInfo();
    }

    public void ActivateReceiveCardPopup(UnityAction receiveCall, UnityAction declareCall)
    {
        ShowInfo(Global.receiveCardInfo);
        ActivateTimer(Global.receiveCardDuration);
        receiveCard.cardReceiveObject.SetActive(true);
        receiveCard.cardReceiveButton.onClick.AddListener(receiveCall);
        receiveCard.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Global.alertSmallTichuMsg, 
                                                                                ()=> { declareCall(); UpdateSmallTichuButton(receiveCard.smallTichuButton.gameObject); }
                                                                              )
                                                        );

        RenderReceivedCard();
        
        receiveCard.smallTichuButton.gameObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요. 버튼을 enabled = false 로 하고 흐리게 만들어야함.
    }

    public void DeactivateReceiveCardPopup()
    {
        receiveCard.cardReceiveButton.onClick.RemoveAllListeners();
        receiveCard.smallTichuButton.onClick.RemoveAllListeners();

        DeactivateAlertPopup();
        DeactivateTimer();

        receiveCard.cardReceiveObject.SetActive(false);
        HideInfo();
    }

    public void ActivateTrickSelection(UnityAction SelectTrickCall, UnityAction PassTrickCall, UnityAction SmallTichuCall)
    {
        ShowInfo(Global.selectTrickInfo);
        ActivateTimer(Global.selectTrickDuration);

        //리스너 삽입.
        selectTrick.submitButton.onClick.AddListener(SelectTrickCall);
        selectTrick.passButton.onClick.AddListener(PassTrickCall);

        selectTrick.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Global.alertSmallTichuMsg,
                                                                                () => { SmallTichuCall(); UpdateSmallTichuButton(selectTrick.smallTichuButton.gameObject); }
                                                                              )
                                                        );

        selectTrick.trickSelectionObject.SetActive(true);
        selectTrick.smallTichuButton.gameObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요. 버튼을 enabled = false 로 하고 흐리게 만들어야함.
    }

    public void DeactivateTrickSelection()
    {
        selectTrick.submitButton.onClick.RemoveAllListeners();
        selectTrick.passButton.onClick.RemoveAllListeners();
        selectTrick.bombButton.onClick.RemoveAllListeners();
        selectTrick.smallTichuButton.onClick.RemoveAllListeners();

        DeactivateAlertPopup();
        DeactivateTimer();

        selectTrick.trickSelectionObject.SetActive(false);
        HideInfo();
    }

    public void ActivateAlertPopup(string alertText, UnityAction confirmCall)
    {
        alertPopup.alertPopupObject.SetActive(true);
        alertPopup.alertText.text = alertText;

        alertPopup.alertConfirmButton.onClick.AddListener(confirmCall);
        alertPopup.alertCancelButton.onClick.AddListener(DeactivateAlertPopup);
    }

    public void DeactivateAlertPopup()
    {
        alertPopup.alertText.text = null;

        alertPopup.alertConfirmButton.onClick.RemoveAllListeners();
        alertPopup.alertCancelButton.onClick.RemoveAllListeners();
        alertPopup.alertPopupObject.SetActive(false);
    }

    IEnumerator timerCoroutineVariable;

    public void ActivateTimer(float duration)
    {
        timerCoroutineVariable = TimerCoroutine(duration);
        StartCoroutine(timerCoroutineVariable);
    }
    public void DeactivateTimer()
    {
        StopCoroutine(timerCoroutineVariable);
        timer.timerText.text = null;
    }


    public void ShowInfo(string text)
    {
        infoBar.infoBarText.text = text;
    }

    public void HideInfo()
    {
        infoBar.infoBarText.text = null;
    }

    public IEnumerator TimerCoroutine(float inputDuration)
    {
        timerDuration = inputDuration;
        while (timerDuration >= 0)
        {
            ShowTimer(((int)(timerDuration)).ToString());
            timerDuration -= Global.tick;
            yield return new WaitForSeconds(Global.tick);
        }
    }

    public void ShowTimer(string text)
    {
        timer.timerText.text = text;
    }

    public void UpdateSmallTichuButton(GameObject buttonObject)
    {
        buttonObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요. 버튼을 enabled = false 로 하고 흐리게 만들어야함.
    }

    public void WritePlayerNameToSlot()
    {
        for(int idx = GameManager.instance.currentPlayer.playerNumber + 1; idx < GameManager.instance.currentPlayer.playerNumber + 1 + Global.numberOfSlots; ++idx)
        {
            int nowSlotIdx = idx - (GameManager.instance.currentPlayer.playerNumber + 1);
            int nowPlayerIdx = idx % Global.numberOfPlayers;
            exchangeCard.slots[nowSlotIdx].player = GameManager.instance.players[nowPlayerIdx];
            exchangeCard.slots[nowSlotIdx].playerText.text = exchangeCard.slots[nowSlotIdx].player.playerName;
        }
    }

    public void FlushCard()
    {
        for (int i = 0; i < Global.numberOfSlots; ++i)
        {
            exchangeCard.slots[i].slot.card.isFixed = false;
            exchangeCard.slots[i].player.AddCardToSlot(exchangeCard.slots[i].slot.card, GameManager.instance.currentPlayer);
            exchangeCard.slots[i].slot.card.transform.position = Global.hiddenCardPosition;
        }
    }

    public void Massage(string msg)
    {
        if(isMassaging==false) StartCoroutine(MassageCoroutine(msg));
    }

    private IEnumerator MassageCoroutine(string msg)
    {
        StartCoroutine(ShakeCoroutine(infoBar.infoBarText.gameObject, Global.shakeDuration));
        isMassaging = true;
        string originalMsg = infoBar.infoBarText.text;
        Color originalColor = infoBar.infoBarText.color;
        ShowInfo(msg);
        infoBar.infoBarText.color = Global.massageColor;
        yield return new WaitForSeconds(Global.massageDuration);
        ShowInfo(originalMsg);
        infoBar.infoBarText.color = originalColor;
        isMassaging = false;
    }

    public bool IsAllSlotSelected()
    {
        foreach (var nowSlot in exchangeCard.slots) if (nowSlot.slot.card == null) return false;
        return true;
    }

    private IEnumerator ShakeCoroutine(GameObject shakeObject, float duration)
    {
        float startPosX = shakeObject.transform.position.x;
        float startPosY = shakeObject.transform.position.y;
        while(duration>0)
        {
            duration -= Global.shakeTick;
            shakeObject.transform.position = new Vector3(
                                                        startPosX + Mathf.Sin(Time.time * Global.shakeSpeedX) * Global.shakeAmountX,
                                                        startPosY + Mathf.Sin(Time.time * Global.shakeSpeedY) * Global.shakeAmountY,
                                                        shakeObject.transform.position.z
                                                        );
            yield return new WaitForSeconds(Global.shakeTick);
        }
        shakeObject.transform.position = new Vector3(startPosX, startPosY, shakeObject.transform.position.z);
    }

    public bool IsTimeOut()
    {
        return timerDuration < 0;
    }

    public void RenderCards(Vector3 centerPosition, int numberOfCardsForLine, List<Card> cardList)
    {
        foreach (var item in GameManager.instance.cards) if (item.isFixed == false) item.transform.position = Global.hiddenCardPosition;

        float offsetX = Global.width / (numberOfCardsForLine - 1);
        float offsetY = Global.offsetY;
        float offsetZ = Global.offsetZ;

        Vector3 initialPosition = centerPosition + new Vector3(-offsetX * ((float)(numberOfCardsForLine - 1) / 2f), 0, 0);
        Vector3 pos = Vector3.zero;

        int cnt = 0;
        foreach (var item in cardList)
        {
            if (cnt == numberOfCardsForLine)
            {
                pos.x = 0;
                pos.y -= offsetY;
                cnt = 0;
            }

            item.transform.position = initialPosition + pos;
            pos.x += offsetX;
            pos.z -= offsetZ;
            ++cnt;
        }
    }

    public void RenderPlayerInfo()
    {
        for(int idx = GameManager.instance.currentPlayer.playerNumber; idx <GameManager.instance.currentPlayer.playerNumber + Global.numberOfPlayers; ++idx)
        {
            var nowPlayerIdx         = idx % Global.numberOfPlayers;
            var nowPlayer            = GameManager.instance.players[nowPlayerIdx];
            var nowPlayerInfo        = playerInfo.playerInfo[idx - GameManager.instance.currentPlayer.playerNumber];

            nowPlayerInfo.name.text       = nowPlayer.playerName;
            nowPlayerInfo.hand.text       = nowPlayer.cards.Count.ToString();
            nowPlayerInfo.roundScore.text = nowPlayer.roundScore.ToString();
            nowPlayerInfo.totalScore.text = nowPlayer.totalScore.ToString();
            nowPlayerInfo.largeTichuIconObject.SetActive(nowPlayer.largeTichuFlag);
            nowPlayerInfo.smallTichuIconObject.SetActive(nowPlayer.smallTichuFlag);
        }
    }

    public void RenderReceivedCard()
    {
        for (int idx = 0; idx < Global.numberOfSlots; ++idx)
        {
            var nowCard = GameManager.instance.currentPlayer.slot[idx].card;

            nowCard.isFixed = true;
            nowCard.transform.position = receiveCard.cardReceiveSlots[idx].slotObject.transform.position + Global.frontEpsilon;

            var nowPlayer = GameManager.instance.currentPlayer.slot[idx].player;
            receiveCard.cardReceiveSlots[idx].playerNameText.text = nowPlayer.playerName;
        }
    }

    public void DisplayTrickInfo(Global.Trick trick)
    {
        switch(trick.trickType)
        {
            case Global.TrickType.Blank:             ShowInfo(Global.selectTrickInfo);          break;
            case Global.TrickType.IsNotTrick:        ShowInfo(Global.isNotTrickInfo);           break;
            case Global.TrickType.Single:            ShowInfo(Global.singleTrickInfo);          break;
            case Global.TrickType.Pair:              ShowInfo(Global.pairTrickInfo);            break;
            case Global.TrickType.Triple:            ShowInfo(Global.tripleTrickInfo);          break;
            case Global.TrickType.ConsecutivePair:   ShowInfo(Global.consecutivePairTrickInfo); break;
            case Global.TrickType.Straight:          ShowInfo(Global.straightTrickInfo);        break;
            case Global.TrickType.FullHouse:         ShowInfo(Global.fullHouseTrickInfo);       break;
            case Global.TrickType.FourCardBomb:      ShowInfo(Global.fourCardTrickInfo);        break;
            case Global.TrickType.StraightFlushBomb: ShowInfo(Global.straightFlushTrickInfo);   break;
        }
    }
}
