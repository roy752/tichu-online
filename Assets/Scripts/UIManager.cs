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
    private Global.LargeTichu        largeTichu   = new Global.LargeTichu();
    private Global.InfoBar           infoBar      = new Global.InfoBar();
    private Global.Timer             timer        = new Global.Timer();
    private Global.ExchangeCardPopup exchangeCard = new Global.ExchangeCardPopup();
    private Global.PlayerInfoUI      playerInfo   = new Global.PlayerInfoUI();


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
        ///////////////////////////////////

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
            var nowSlot = exchangeCard.exchangeCardPopupObject.transform.Find(Global.exchangeCardSlotObjectName[i]);
            exchangeCard.slots[i].slot       = nowSlot.GetComponent<SlotSelectHandler>();
            exchangeCard.slots[i].playerText = nowSlot.Find(Global.exchangeplayerObjectName).GetComponent<TMP_Text>();
        }
        /////////////////////////////////


        //플레이어 인포 오브젝트
        playerInfo.playerInfoObject = uiParent.transform.Find(Global.playerInfoObjectName).gameObject;
        playerInfo.playerInfo = new Global.PlayerInfo[Global.numberOfPlayers];

        for(int idx = 0; idx<playerInfo.playerInfo.Length; ++idx)
        {
            playerInfo.playerInfo[idx].playerInfoObject = playerInfo.playerInfoObject.transform.Find(Global.playerInfoObjectNames[idx]).gameObject;
            GameObject nowPlayerInfoObject = playerInfo.playerInfo[idx].playerInfoObject;

            playerInfo.playerInfo[idx].name                 = nowPlayerInfoObject.transform.Find(Global.playerInfoNameObjectName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].hand                 = nowPlayerInfoObject.transform.Find(Global.playerInfoHandObjectName).Find(Global.playerInfoHandName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].smallTichuIconObject = nowPlayerInfoObject.transform.Find(Global.playerInfoTichuObjectName).Find(Global.playerInfoSmallTichuName).gameObject;
            playerInfo.playerInfo[idx].largeTichuIconObject = nowPlayerInfoObject.transform.Find(Global.playerInfoTichuObjectName).Find(Global.playerInfoLargeTichuName).gameObject;
            playerInfo.playerInfo[idx].roundScore           = nowPlayerInfoObject.transform.Find(Global.playerInfoScoreObjectName).Find(Global.playerInfoRoundScoreName).GetComponent<TMP_Text>();
            playerInfo.playerInfo[idx].totalScore           = nowPlayerInfoObject.transform.Find(Global.playerInfoScoreObjectName).Find(Global.playerInfoTotalScoreName).GetComponent<TMP_Text>();
        }


    }

    public void ShowInfo(string text)
    {
        infoBar.infoBarText.text = text;
    }

    public void HideInfo()
    {
        infoBar.infoBarText.text = null;
    }

    public void ActivateLargeTichu(UnityAction DeclareCall, UnityAction SkipCall)
    {

        ShowInfo(Global.largeTichuInfo);

        largeTichu.largeTichuObject.SetActive(true);

        largeTichu.declareButton.onClick.AddListener(DeclareCall);
        largeTichu.skipButton.onClick.AddListener(SkipCall);
    }

    public void DeactivateLargeTichu()
    {

        largeTichu.declareButton.onClick.RemoveAllListeners();
        largeTichu.skipButton.onClick.RemoveAllListeners();

        largeTichu.largeTichuObject.SetActive(false);
        HideInfo();
    }

    IEnumerator timerCoroutineVariable;

    public void ActivateTimer(float duration)
    {
        timerCoroutineVariable = TimerCoroutine(duration);
        StartCoroutine(timerCoroutineVariable);
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

    public void DeactivateTimer()
    {
        StopCoroutine(timerCoroutineVariable);
        timer.timerText.text = null;
    }

    public void ActivateExchangeCardsPopup(UnityAction exchangeCall, UnityAction declareSmallTichuCall)
    {
        ShowInfo(Global.exchangeCardInfo);
        exchangeCard.exchangeCardPopupObject.SetActive(true);
        WritePlayerName();
        exchangeCard.exchangeCardButton.onClick.AddListener(exchangeCall);
        exchangeCard.smallTichuButton.onClick.AddListener(declareSmallTichuCall);
        exchangeCard.smallTichuButton.gameObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요
    }

    public void UpdateExchangeCardsSmallTichuButton()
    {
        exchangeCard.smallTichuButton.gameObject.SetActive(GameManager.instance.currentPlayer.canDeclareSmallTichu); //수정 필요
    }

    public void DeactivateExchangeCardsPopup()
    {
        exchangeCard.exchangeCardButton.onClick.RemoveAllListeners();
        if(GameManager.instance.currentCard != null)
        {
            GameManager.instance.currentCard.cardObject.GetComponent<SelectionHandler>().ToggleBase();
            GameManager.instance.currentCard = null;
        }
        if(GameManager.instance.currentSlot != null)
        {
            GameManager.instance.currentSlot.ToggleBase();
            GameManager.instance.currentSlot = null;
        }
        FlushCard();
        exchangeCard.exchangeCardPopupObject.SetActive(false);
        HideInfo();
    }

    public void WritePlayerName()
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
        for(int i=0; i<Global.numberOfSlots; ++i)
        {
            if(exchangeCard.slots[i].slot.card == null)
            {
                int idx = Random.Range(0, GameManager.instance.currentPlayer.cards.Count);
                exchangeCard.slots[i].slot.card = GameManager.instance.currentPlayer.cards[idx];
                GameManager.instance.currentPlayer.RemoveCard(exchangeCard.slots[i].slot.card);
            }
            exchangeCard.slots[i].slot.card.isFixed = false;

            exchangeCard.slots[i].player.AddCardToBuffer(exchangeCard.slots[i].slot.card);
            exchangeCard.slots[i].slot.card.cardObject.transform.position = Global.hiddenCardPosition;
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

    public void RenderCards(Vector3 centerPosition, int numberOfCardsForLine, List<Global.Card> cardList)
    {
        foreach (var item in GameManager.instance.cards) if (item.isFixed == false) item.cardObject.transform.position = Global.hiddenCardPosition;

        float offsetX = Global.width / (numberOfCardsForLine - 1);
        float offsetY = Global.offsetY;
        float offsetZ = Global.offsetZ;

        Vector3 initialPosition = centerPosition + new Vector3(-offsetX * ((float)(numberOfCardsForLine - 1) / 2f), offsetY * ((cardList.Count - 1) / numberOfCardsForLine), 0);

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

            item.cardObject.transform.position = initialPosition + pos;
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
}
