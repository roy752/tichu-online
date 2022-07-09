using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UIManager : MonoBehaviour
{

    [HideInInspector]
    public GameObject uiParent;

    [HideInInspector]
    public static UIManager instance;

    private struct LargeTichu
    {
        public GameObject largeTichuObject;
        public Button declareButton;
        public Button skipButton;
    }

    private LargeTichu largeTichu = new LargeTichu();

    private struct InfoBar
    {
        public GameObject infoBarObject;
        public TMP_Text infoBarText;
    }

    private InfoBar infoBar = new InfoBar();

    private struct Timer
    {
        public GameObject timerObject;
        public TMP_Text timerText;
    }

    private Timer timer = new Timer();

    private struct ExchangeCardSlot
    {
        public TMP_Text playerText;
        public SlotSelectHandler slot;
        public GamePlayer player;
    }

    private struct ExchangeCardPopup
    {
        public GameObject exchangeCardPopupObject;
        public Button exchangeCardButton;
        public ExchangeCardSlot[] slots; 
    }

    private ExchangeCardPopup exchangeCardObject = new ExchangeCardPopup();

    private bool isMassaging = false;

    private void Start()
    {
        InitializeVariables();
        instance = this;
    }

    private void InitializeVariables()
    {
        uiParent = GameObject.Find(GlobalInfo.uiParentObjectName);

        //라지티츄 오브젝트, 버튼
        largeTichu.largeTichuObject = uiParent.transform.Find(GlobalInfo.largeTichuButtonObjectName).gameObject;
        largeTichu.declareButton    = largeTichu.largeTichuObject.transform.Find(GlobalInfo.largeTichuDeclareButtonName).GetComponent<Button>();
        largeTichu.skipButton       = largeTichu.largeTichuObject.transform.Find(GlobalInfo.largeTichuSkipButtonName).GetComponent<Button>();
        ///////////////////////////////////

        //인포 창 오브젝트, 텍스트
        infoBar.infoBarObject = uiParent.transform.Find(GlobalInfo.infoBarObjectName).gameObject;
        infoBar.infoBarText = infoBar.infoBarObject.transform.Find("Text").GetComponent<TMP_Text>();
        //////////////////////////////////
        

        //타이머 오브젝트
        timer.timerObject = uiParent.transform.Find("Timer").gameObject;
        timer.timerText = timer.timerObject.GetComponent<TMP_Text>();
        /////////////////////////////////
        

        //카드 교환 팝업 오브젝트
        exchangeCardObject.exchangeCardPopupObject = uiParent.transform.Find(GlobalInfo.exchangeCardObjectName).gameObject;
        exchangeCardObject.exchangeCardButton = exchangeCardObject.exchangeCardPopupObject.transform.Find(GlobalInfo.exchangeCardButtonObjectName).GetComponent<Button>();
        exchangeCardObject.slots = new ExchangeCardSlot[3];
        for (int i = 0; i < 3; ++i)
        {
            var nowSlot = exchangeCardObject.exchangeCardPopupObject.transform.Find(GlobalInfo.exchangeCardSlotObjectName + i.ToString());
            exchangeCardObject.slots[i].slot = nowSlot.GetComponent<SlotSelectHandler>();
            exchangeCardObject.slots[i].playerText = nowSlot.Find(GlobalInfo.exchangeplayerObjectName).GetComponent<TMP_Text>();
        }
        /////////////////////////////////
        

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

        ShowInfo(GlobalInfo.largeTichuInfo);

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
        while (inputDuration >= 0)
        {
            ShowTimer(((int)(inputDuration)).ToString());
            inputDuration -= GlobalInfo.tick;
            yield return new WaitForSeconds(GlobalInfo.tick);
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

    public void ActivateExchangeCardsPopup(UnityAction exchangeCall)
    {
        ShowInfo(GlobalInfo.exchangeCardInfo);
        exchangeCardObject.exchangeCardPopupObject.SetActive(true);
        WritePlayerName();
        exchangeCardObject.exchangeCardButton.onClick.AddListener(exchangeCall);
        //버튼에 보내기 call 할당
    }


    public void DeactivateExchangeCardsPopup()
    {
        //버튼에 call 삭제
        exchangeCardObject.exchangeCardButton.onClick.RemoveAllListeners();
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
        exchangeCardObject.exchangeCardPopupObject.SetActive(false);
        HideInfo();
    }

    public void WritePlayerName()
    {
        int cnt = 0;
        for(int i = GameManager.instance.currentPlayer.playerNumber + 1; cnt<3; ++i)
        {
            exchangeCardObject.slots[cnt].player = GameManager.instance.players[i % 4];
            exchangeCardObject.slots[cnt].playerText.text = exchangeCardObject.slots[cnt].player.playerName;
            ++cnt;
        }
    }

    public void FlushCard()
    {
        for(int i=0; i<3; ++i)
        {
            if(exchangeCardObject.slots[i].slot.card == null)
            {
                int idx = Random.Range(0, GameManager.instance.currentPlayer.cards.Count);
                exchangeCardObject.slots[i].slot.card = GameManager.instance.currentPlayer.cards[idx];
                GameManager.instance.currentPlayer.RemoveCard(exchangeCardObject.slots[i].slot.card);
            }
            exchangeCardObject.slots[i].slot.card.isFixed = false;

            exchangeCardObject.slots[i].player.AddCardToBuffer(exchangeCardObject.slots[i].slot.card);
            exchangeCardObject.slots[i].slot.card.cardObject.transform.position = GlobalInfo.hiddenCardPosition;
        }
    }

    public void Massage(string msg)
    {
        if(isMassaging==false) StartCoroutine(MassageCoroutine(msg));
    }

    private IEnumerator MassageCoroutine(string msg)
    {
        StartCoroutine(ShakeCoroutine(infoBar.infoBarText.gameObject, GlobalInfo.shakeDuration));
        isMassaging = true;
        string originalMsg = infoBar.infoBarText.text;
        Color originalColor = infoBar.infoBarText.color;
        ShowInfo(msg);
        infoBar.infoBarText.color = GlobalInfo.massageColor;
        yield return new WaitForSeconds(GlobalInfo.massageDuration);
        ShowInfo(originalMsg);
        infoBar.infoBarText.color = originalColor;
        isMassaging = false;
    }

    public bool IsAllSlotSelected()
    {
        foreach (var slot in exchangeCardObject.slots) if (slot.slot.card == null) return false;
        return true;
    }

    private IEnumerator ShakeCoroutine(GameObject shakeObject, float duration)
    {
        float startPosX = shakeObject.transform.position.x;
        float startPosY = shakeObject.transform.position.y;
        while(duration>0)
        {
            duration -= GlobalInfo.shakeTick;
            shakeObject.transform.position = new Vector3(
                                                        startPosX + Mathf.Sin(Time.time * GlobalInfo.shakeSpeedX) * GlobalInfo.shakeAmountX,
                                                        startPosY + Mathf.Sin(Time.time * GlobalInfo.shakeSpeedY) * GlobalInfo.shakeAmountY,
                                                        shakeObject.transform.position.z
                                                        );
            yield return new WaitForSeconds(GlobalInfo.shakeTick);
        }
        shakeObject.transform.position = new Vector3(startPosX, startPosY, shakeObject.transform.position.z);
    }
}
