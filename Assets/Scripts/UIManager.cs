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
    private Util.LargeTichu        largeTichu     = new Util.LargeTichu();
    private Util.InfoBar           infoBar        = new Util.InfoBar();
    private Util.Timer             timer          = new Util.Timer();
    public  Util.ExchangeCardPopup exchangeCard   = new Util.ExchangeCardPopup();
    private Util.PlayerInfoUI      playerInfo     = new Util.PlayerInfoUI();
    private Util.AlertPopup        alertPopup     = new Util.AlertPopup();
    private Util.CardReceivePopup  receiveCard    = new Util.CardReceivePopup();
    private Util.TrickSelection    selectTrick    = new Util.TrickSelection();
    private Util.DragonSelection   selectDragon   = new Util.DragonSelection();
    private Util.RoundResult       roundResult    = new Util.RoundResult();

    private bool isMassaging = false;
    private string originalMsg;

    private float timerDuration;

    private void Start()
    {
        InitializeVariables();
        instance = this;
    }

    private void InitializeVariables()
    {
        uiParent = GameObject.Find(Util.uiParentObjectName);

        //라지티츄 오브젝트
        largeTichu.largeTichuObject = uiParent.transform.Find(Util.largeTichuButtonObjectName).gameObject;
        largeTichu.declareButton    = largeTichu.largeTichuObject.transform.Find(Util.largeTichuDeclareButtonName).GetComponent<Button>();
        largeTichu.skipButton       = largeTichu.largeTichuObject.transform.Find(Util.largeTichuSkipButtonName).GetComponent<Button>();
        //////////////////////////////////


        //인포 창 오브젝트
        infoBar.infoBarObject = uiParent.transform.Find(Util.infoBarObjectName).gameObject;
        infoBar.infoBarText   = infoBar.infoBarObject.transform.Find(Util.infoBarTextObjectName).GetComponent<TMP_Text>();
        //////////////////////////////////
        

        //타이머 오브젝트
        timer.timerObject = uiParent.transform.Find(Util.timerObjectName).gameObject;
        timer.timerText   = timer.timerObject.GetComponent<TMP_Text>();
        /////////////////////////////////
        

        //카드 교환 팝업 오브젝트
        exchangeCard.exchangeCardPopupObject = uiParent.transform.Find(Util.exchangeCardObjectName).gameObject;
        exchangeCard.exchangeCardButton      = exchangeCard.exchangeCardPopupObject.transform.Find(Util.exchangeCardButtonObjectName).GetComponent<Button>();
        exchangeCard.smallTichuButton        = exchangeCard.exchangeCardPopupObject.transform.Find(Util.exchangeCardSmallTichuButtonObjectName).GetComponent<Button>();
        exchangeCard.slots = new Util.ExchangeCardSlot[Util.numberOfSlots];
        for (int i = 0; i < Util.numberOfSlots; ++i)
        {
            var nowSlotObject = exchangeCard.exchangeCardPopupObject.transform.Find(Util.exchangeCardSlotObjectName[i]);
            exchangeCard.slots[i].slot       = nowSlotObject.GetComponent<SlotSelectHandler>();
            exchangeCard.slots[i].playerText = nowSlotObject.Find(Util.exchangeplayerObjectName).GetComponent<TMP_Text>();
        }
        /////////////////////////////////


        //플레이어 인포 오브젝트
        playerInfo.playerInfoObject = uiParent.transform.Find(Util.playerInfoObjectName).gameObject;
        playerInfo.playerInfo       = new Util.PlayerInfo[Util.numberOfPlayers];

        for(int idx = 0; idx<playerInfo.playerInfo.Length; ++idx)
        {
            playerInfo.playerInfo[idx].playerInfoObject = playerInfo.playerInfoObject.transform.Find(Util.playerInfoObjectNames[idx]).gameObject;

            var nowInfoObject = playerInfo.playerInfo[idx].playerInfoObject.transform;

            playerInfo.playerInfo[idx].name                 = nowInfoObject.Find(Util.playerInfoNameObjectName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].hand                 = nowInfoObject.Find(Util.playerInfoHandObjectName).Find(Util.playerInfoHandName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].smallTichuIconObject = nowInfoObject.Find(Util.playerInfoTichuObjectName).Find(Util.playerInfoSmallTichuName).gameObject;
            playerInfo.playerInfo[idx].largeTichuIconObject = nowInfoObject.Find(Util.playerInfoTichuObjectName).Find(Util.playerInfoLargeTichuName).gameObject;
            playerInfo.playerInfo[idx].roundScore           = nowInfoObject.Find(Util.playerInfoScoreObjectName).Find(Util.playerInfoRoundScoreName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].totalScore           = nowInfoObject.Find(Util.playerInfoScoreObjectName).Find(Util.playerInfoTotalScoreName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].trick                = nowInfoObject.Find(Util.playerInfoTrickTextName).GetComponent<TMP_Text>();
        }
        ///////////////////////////////


        //확인창 팝업(alert popup) 오브젝트
        alertPopup.alertPopupObject = uiParent.transform.Find(Util.alertPopupObjectName).gameObject;

        var nowAlertObject          = alertPopup.alertPopupObject.transform;
        
        alertPopup.alertText          = nowAlertObject.Find(Util.alertTextObjectName).GetComponent<TMP_Text>();
        alertPopup.alertConfirmButton = nowAlertObject.Find(Util.alertConfirmButtonObjectName).GetComponent<Button>();
        alertPopup.alertCancelButton  = nowAlertObject.Find(Util.alertCancelButtonObjectName).GetComponent<Button>();
        //////////////////////


        //카드 받는 팝업(card receive popup) 오브젝트
        receiveCard.cardReceiveObject = uiParent.transform.Find(Util.cardReceivePopupObjectName).gameObject;

        var nowCardReceiveObject = receiveCard.cardReceiveObject.transform;
        
        receiveCard.cardReceiveSlots = new Util.CardReceiveSlot[Util.numberOfSlots];

        receiveCard.cardReceiveButton = nowCardReceiveObject.Find(Util.cardReceiveButtonObjectName).GetComponent<Button>();
        receiveCard.smallTichuButton = nowCardReceiveObject.Find(Util.cardReceiveSmallTichuButtonObjectName).GetComponent<Button>();

        for(int idx = 0; idx<Util.numberOfSlots; ++idx)
        {
            receiveCard.cardReceiveSlots[idx].slotObject     = nowCardReceiveObject.Find(Util.cardReceiveSlotObjectNames[idx]).gameObject;
            receiveCard.cardReceiveSlots[idx].InfoObject     = receiveCard.cardReceiveSlots[idx].slotObject.transform.Find(Util.cardReceivePlayerInfoObjectName).gameObject;
            receiveCard.cardReceiveSlots[idx].playerNameText = receiveCard.cardReceiveSlots[idx].InfoObject.transform.Find(Util.cardReceivePlayerNameObjectName).GetComponent<TMP_Text>();
        }
        /////////////////////////////////////////////////


        //트릭 선택 관련 버튼(trick selection) 오브젝트
        selectTrick.trickSelectionObject = uiParent.transform.Find(Util.trickSelectionObjectName).gameObject;

        var nowTrickSelectionObject     = selectTrick.trickSelectionObject.transform;

        selectTrick.bombButton       = nowTrickSelectionObject.Find(Util.trickSelectionBombButtonName).GetComponent<Button>();
        selectTrick.submitButton     = nowTrickSelectionObject.Find(Util.trickSelectionSubmitButtonName).GetComponent<Button>();
        selectTrick.passButton       = nowTrickSelectionObject.Find(Util.trickSelectionPassButtonName).GetComponent<Button>();
        selectTrick.smallTichuButton = nowTrickSelectionObject.Find(Util.trickSelectionSmallTichuButtonName).GetComponent<Button>();
        ////////////////////////


        //용 선택 관련 버튼(dragon selection) 오브젝트
        selectDragon.dragonSelectionObject = uiParent.transform.Find(Util.dragonSelectionPopupObjectName).gameObject;

        var nowDragonSelectionObject = selectDragon.dragonSelectionObject.transform;

        selectDragon.previousOpponentButton = nowDragonSelectionObject.Find(Util.dragonSelectionOpponentButtonNames[0]).GetComponent<Button>();
        selectDragon.previousOpponentName   = selectDragon.previousOpponentButton.transform.Find(Util.dragonSelectionOpponentTextNames[0]).GetComponent<TMP_Text>();
        selectDragon.nextOpponentButton     = nowDragonSelectionObject.Find(Util.dragonSelectionOpponentButtonNames[1]).GetComponent<Button>();
        selectDragon.nextOpponentName       = selectDragon.nextOpponentButton.transform.Find(Util.dragonSelectionOpponentTextNames[1]).GetComponent<TMP_Text>();
        /////////////////////////////


        //라운드 결과 관련(round result) 오브젝트
        roundResult.roundResultObject = uiParent.transform.Find(Util.roundResultPopupObjectName).gameObject;
        roundResult.team = new Util.RoundResultTeam[Util.numberOfTeam];
        for(int idx = 0; idx<Util.numberOfTeam; ++idx)
        {
            roundResult.team[idx].roundResultTeamObject = roundResult.roundResultObject.transform.Find(Util.roundResultTeamObjectNames[idx]).gameObject;
            
            var nowObject = roundResult.team[idx].roundResultTeamObject.transform;

            roundResult.team[idx].teamName        = nowObject.Find(Util.roundResultTeamNameTextName).GetComponent<TMP_Text>();
            roundResult.team[idx].trickScore      = nowObject.Find(Util.roundResultTrickScoreTextName).GetComponent<TMP_Text>();
            roundResult.team[idx].tichuScore      = nowObject.Find(Util.roundResultTichuScoreTextName).GetComponent<TMP_Text>();
            roundResult.team[idx].oneTwoScore     = nowObject.Find(Util.roundResultOneTwoScoreTextName).GetComponent<TMP_Text>();
            roundResult.team[idx].roundTotalScore = nowObject.Find(Util.roundResultRoundTotalScoreTextName).GetComponent<TMP_Text>();
            roundResult.team[idx].presentScore    = nowObject.Find(Util.roundResultPresentScoreTextName).GetComponent<TMP_Text>();
        }

    }

    public void ActivateLargeTichu(UnityAction DeclareCall, UnityAction SkipCall)
    {

        ShowInfo(Util.largeTichuInfo);
        ActivateTimer(Util.largeTichuDuration);

        largeTichu.largeTichuObject.SetActive(true);

        largeTichu.declareButton.onClick.AddListener(() => ActivateAlertPopup(Util.alertLargeTichuMsg, DeclareCall)); // 람다식, 델리게이트 알아볼 것.
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
        ShowInfo(Util.exchangeCardInfo);
        ActivateTimer(Util.exchangeCardsDuration);
        exchangeCard.exchangeCardPopupObject.SetActive(true);
        WritePlayerNameToSlot();
        exchangeCard.exchangeCardButton.onClick.AddListener(exchangeCall);
        exchangeCard.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Util.alertSmallTichuMsg,
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
        ShowInfo(Util.receiveCardInfo);
        ActivateTimer(Util.receiveCardDuration);
        receiveCard.cardReceiveObject.SetActive(true);
        receiveCard.cardReceiveButton.onClick.AddListener(receiveCall);
        receiveCard.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Util.alertSmallTichuMsg, 
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
        ShowInfo(Util.selectTrickInfo);
        ActivateTimer(Util.selectTrickDuration);

        //리스너 삽입.
        selectTrick.submitButton.onClick.AddListener(SelectTrickCall);
        selectTrick.passButton.onClick.AddListener(PassTrickCall);

        selectTrick.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Util.alertSmallTichuMsg,
                                                                                () => { SmallTichuCall(); UpdateSmallTichuButton(selectTrick.smallTichuButton.gameObject); }
                                                                              )
                                                        );

        selectTrick.trickSelectionObject.SetActive(true);
        selectTrick.passButton.gameObject.SetActive(!GameManager.instance.isFirstTrick);
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

    public void ActivateDragonSelection(UnityAction SelectNextOpponentCall, UnityAction SelectPreviousOpponentCall)
    {
        ShowInfo(Util.selectDragonInfo);
        ActivateTimer(Util.selectDragonDuration);
        
        selectDragon.dragonSelectionObject.SetActive(true);

        selectDragon.previousOpponentName.text = GameManager.instance.players[(GameManager.instance.currentPlayer.playerNumber + 3) % Util.numberOfPlayers].playerName;
        selectDragon.nextOpponentName.text = GameManager.instance.players[(GameManager.instance.currentPlayer.playerNumber + 1) % Util.numberOfPlayers].playerName;

        selectDragon.previousOpponentButton.onClick.AddListener(SelectPreviousOpponentCall);
        selectDragon.nextOpponentButton.onClick.AddListener(SelectNextOpponentCall);

    }

    public void DeactivateDragonSelection()
    {
        selectDragon.nextOpponentButton.onClick.RemoveAllListeners();
        selectDragon.previousOpponentButton.onClick.RemoveAllListeners();

        selectDragon.dragonSelectionObject.SetActive(false);
        DeactivateTimer();
        HideInfo();
    }

    public void ActivateBombSelection(UnityAction SelectBombCall, UnityAction PassCall, UnityAction SmallTichuCall)
    {
        ShowInfo(Util.selectBombInfo);
        ActivateTimer(Util.selectBombDuration);

        //리스너 삽입.
        selectTrick.submitButton.onClick.AddListener(SelectBombCall);
        selectTrick.passButton.onClick.AddListener(PassCall);
        selectTrick.smallTichuButton.onClick.AddListener(
                                                      () => ActivateAlertPopup(
                                                                                Util.alertSmallTichuMsg,
                                                                                () => { SmallTichuCall(); UpdateSmallTichuButton(selectTrick.smallTichuButton.gameObject); }
                                                                              )
                                                        );

        selectTrick.trickSelectionObject.SetActive(true);
        selectTrick.smallTichuButton.gameObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요. 버튼을 enabled = false 로 하고 흐리게 만들어야함.
    }

    public void DeactivateBombSelection()
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

    public void ActivateRoundResult(Util.Score[] teamScore)
    {
        ShowInfo(Util.roundResultInfo);

        roundResult.roundResultObject.SetActive(true);


        for(int idx = 0;idx<Util.numberOfTeam; ++idx)
        {
            roundResult.team[idx].teamName.text = Util.GetTeamName(GameManager.instance.players[idx], GameManager.instance.players[idx + 2]);
            roundResult.team[idx].trickScore.text = teamScore[idx].trickScore.ToString();
            roundResult.team[idx].tichuScore.text = teamScore[idx].tichuScore.ToString();
            roundResult.team[idx].oneTwoScore.text = teamScore[idx].oneTwoScore.ToString();
            roundResult.team[idx].roundTotalScore.text = (teamScore[idx].trickScore + teamScore[idx].tichuScore + teamScore[idx].oneTwoScore).ToString();
            roundResult.team[idx].presentScore.text = (teamScore[idx].previousScore + teamScore[idx].trickScore + teamScore[idx].tichuScore + teamScore[idx].oneTwoScore).ToString();
        }
    }

    public void DeactivateRoundResult()
    {

        roundResult.roundResultObject.SetActive(false);

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
        if (isMassaging) originalMsg = text;
        else infoBar.infoBarText.text = text;
    }

    public void ForceShowInfo(string text)
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
            timerDuration -= Util.tick;
            yield return new WaitForSeconds(Util.tick);
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
        for(int idx = GameManager.instance.currentPlayer.playerNumber + 1; idx < GameManager.instance.currentPlayer.playerNumber + 1 + Util.numberOfSlots; ++idx)
        {
            int nowSlotIdx = idx - (GameManager.instance.currentPlayer.playerNumber + 1);
            int nowPlayerIdx = idx % Util.numberOfPlayers;
            exchangeCard.slots[nowSlotIdx].player = GameManager.instance.players[nowPlayerIdx];
            exchangeCard.slots[nowSlotIdx].playerText.text = exchangeCard.slots[nowSlotIdx].player.playerName;
        }
    }

    public void FlushCard()
    {
        for (int i = 0; i < Util.numberOfSlots; ++i)
        {
            exchangeCard.slots[i].slot.card.isFixed = false;
            exchangeCard.slots[i].player.AddCardToSlot(exchangeCard.slots[i].slot.card, GameManager.instance.currentPlayer);
            exchangeCard.slots[i].slot.card.transform.position = Util.hiddenCardPosition;
        }
    }

    public void Massage(string msg)
    {
        if(isMassaging==false) StartCoroutine(MassageCoroutine(msg));
    }

    private IEnumerator MassageCoroutine(string msg)
    {
        StartCoroutine(ShakeCoroutine(infoBar.infoBarText.gameObject, Util.shakeDuration));
        isMassaging = true;
        originalMsg = infoBar.infoBarText.text;
        Color originalColor = infoBar.infoBarText.color;
        ForceShowInfo(msg);
        infoBar.infoBarText.color = Util.massageColor;
        yield return new WaitForSeconds(Util.massageDuration);
        ForceShowInfo(originalMsg);
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
            duration -= Util.shakeTick;
            shakeObject.transform.position = new Vector3(
                                                        startPosX + Mathf.Sin(Time.time * Util.shakeSpeedX) * Util.shakeAmountX,
                                                        startPosY + Mathf.Sin(Time.time * Util.shakeSpeedY) * Util.shakeAmountY,
                                                        shakeObject.transform.position.z
                                                        );
            yield return new WaitForSeconds(Util.shakeTick);
        }
        shakeObject.transform.position = new Vector3(startPosX, startPosY, shakeObject.transform.position.z);
    }

    public bool IsTimeOut()
    {
        return timerDuration < 0;
    }

    public void RenderCards(Vector3 centerPosition, int numberOfCardsForLine, List<Card> cardList)
    {
        foreach (var item in GameManager.instance.cards) if (item.isFixed == false) item.transform.position = Util.hiddenCardPosition;

        float offsetX = Util.width / (numberOfCardsForLine - 1);
        float offsetY = Util.offsetY;
        float offsetZ = Util.offsetZ;

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
        for(int idx = GameManager.instance.currentPlayer.playerNumber; idx <GameManager.instance.currentPlayer.playerNumber + Util.numberOfPlayers; ++idx)
        {
            var nowPlayerIdx         = idx % Util.numberOfPlayers;
            var nowPlayer            = GameManager.instance.players[nowPlayerIdx];
            var nowPlayerInfo        = playerInfo.playerInfo[idx - GameManager.instance.currentPlayer.playerNumber];

            nowPlayerInfo.name.text       = nowPlayer.playerName;
            nowPlayerInfo.hand.text       = nowPlayer.cards.Count.ToString();
            nowPlayerInfo.roundScore.text = nowPlayer.roundScore.ToString();
            nowPlayerInfo.totalScore.text = nowPlayer.totalScore.ToString();
            nowPlayerInfo.trick.text      = nowPlayer.previousTrick;
            nowPlayerInfo.largeTichuIconObject.SetActive(nowPlayer.largeTichuFlag);
            nowPlayerInfo.smallTichuIconObject.SetActive(nowPlayer.smallTichuFlag);

        }
    }

    public void RenderReceivedCard()
    {
        for (int idx = 0; idx < Util.numberOfSlots; ++idx)
        {
            var nowCard = GameManager.instance.currentPlayer.slot[idx].card;

            nowCard.isFixed = true;
            nowCard.transform.position = receiveCard.cardReceiveSlots[idx].slotObject.transform.position + Util.frontEpsilon;

            var nowPlayer = GameManager.instance.currentPlayer.slot[idx].player;
            receiveCard.cardReceiveSlots[idx].playerNameText.text = nowPlayer.playerName;
        }
    }

    public void RenderTrickCard(List<Card> cardList)
    {
        if (GameManager.instance.trickStack.Count > 0)
        {
            foreach (var card in GameManager.instance.trickStack.Peek().cards) card.isFixed = false;
        }
        if (cardList != null)
        {
            Vector3 nowPosition = Util.initialTrickPosition + new Vector3((-(cardList.Count - 1) / 2)*Util.trickCardOffset,0,0);
            foreach (var card in cardList)
            {
                card.transform.position = nowPosition;
                card.isFixed = true;
                nowPosition += new Vector3(Util.trickCardOffset, 0, -Util.offsetZ);
            }
        }
    }

    private bool isWaitFinished;
    public void Wait(float duration)
    {
        StartCoroutine(WaitCoroutine(duration));
    }

    public IEnumerator WaitCoroutine(float duration)
    {
        isWaitFinished = false;
        while (duration > 0)
        {
            duration -= Util.tick;
            yield return new WaitForSeconds(Util.tick);
        }
        isWaitFinished = true;
    }

    public bool IsWaitFinished()
    {
        return isWaitFinished;
    }
}
