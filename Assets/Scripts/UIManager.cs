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
        largeTichu.skipObject    = uiParent.transform.Find(GlobalInfo.buttons.ltNoButtonName).gameObject;
        largeTichu.declareButton = largeTichu.declareObject.GetComponent<Button>();
        largeTichu.skipButton    = largeTichu.skipObject.GetComponent<Button>();
        ///////////////////////////////////

        //인포 창 오브젝트, 텍스트
        infoBar.infoBarObject = uiParent.transform.Find(GlobalInfo.infoBarObjectName).gameObject;
        infoBar.infoBarText = infoBar.infoBarObject.transform.Find("Text").GetComponent<TMP_Text>();

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
}
