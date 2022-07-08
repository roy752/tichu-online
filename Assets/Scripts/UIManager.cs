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
        public GameObject declareObject;
        public GameObject skipObject;
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
    }

    private struct ExchangeCardPopup
    {
        public GameObject exchangeCardPopupObject;
        public ExchangeCardSlot[] slots; 
    }

    private ExchangeCardPopup exchangeCardObject = new ExchangeCardPopup();

    private void Start()
    {
        InitializeVariables();
        instance = this;
    }

    private void InitializeVariables()
    {
        uiParent = GameObject.Find(GlobalInfo.uiParentObjectName);

        //라지티츄 오브젝트, 버튼
        largeTichu.declareObject = uiParent.transform.Find(GlobalInfo.buttons.ltYesButtonName).gameObject;
        largeTichu.skipObject = uiParent.transform.Find(GlobalInfo.buttons.ltNoButtonName).gameObject;
        largeTichu.declareButton = largeTichu.declareObject.GetComponent<Button>();
        largeTichu.skipButton = largeTichu.skipObject.GetComponent<Button>();
        ///////////////////////////////////

        //인포 창 오브젝트, 텍스트
        infoBar.infoBarObject = uiParent.transform.Find(GlobalInfo.infoBarObjectName).gameObject;
        infoBar.infoBarText = infoBar.infoBarObject.transform.Find("Text").GetComponent<TMP_Text>();

        //타이머 오브젝트
        timer.timerObject = uiParent.transform.Find("Timer").gameObject;
        timer.timerText = timer.timerObject.GetComponent<TMP_Text>();

        //카드 교환 팝업 오브젝트
        exchangeCardObject.exchangeCardPopupObject = uiParent.transform.Find(GlobalInfo.exchangeCardObjectName).gameObject;
        exchangeCardObject.slots = new ExchangeCardSlot[3];
        for (int i = 0; i < 3; ++i)
        {
            var nowSlot = exchangeCardObject.exchangeCardPopupObject.transform.Find(GlobalInfo.exchangeCardSlotObjectName + i.ToString());
            exchangeCardObject.slots[i].slot = nowSlot.GetComponent<SlotSelectHandler>();
            exchangeCardObject.slots[i].playerText = nowSlot.Find(GlobalInfo.exchangeplayerObjectName).GetComponent<TMP_Text>();
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

        ShowInfo(GlobalInfo.largeTichuInfo);

        largeTichu.declareObject.SetActive(true);
        largeTichu.skipObject.SetActive(true);

        largeTichu.declareButton.onClick.AddListener(DeclareCall);
        largeTichu.skipButton.onClick.AddListener(SkipCall);
    }

    public void DeactivateLargeTichu()
    {

        largeTichu.declareButton.onClick.RemoveAllListeners();
        largeTichu.skipButton.onClick.RemoveAllListeners();

        largeTichu.declareObject.SetActive(false);
        largeTichu.skipObject.SetActive(false);

        HideInfo();
    }

    public void ActivateTimer()
    {
        timer.timerObject.SetActive(true);
    }

    public void ShowTimer(string text)
    {
        timer.timerText.text = text;
    }

    public void DeactivateTimer()
    {
        timer.timerText.text = null;
        timer.timerObject.SetActive(false);
    }

    public void ActivateExchangeCardsPopup()
    {
        ShowInfo(GlobalInfo.exchangeCardInfo);
        exchangeCardObject.exchangeCardPopupObject.SetActive(true);

        //버튼에 보내기 call 할당
    }
    public void DeactivateExchangeCardsPopup()
    {
        //버튼에 call 삭제
        exchangeCardObject.exchangeCardPopupObject.SetActive(false);
        HideInfo();
    }
}
