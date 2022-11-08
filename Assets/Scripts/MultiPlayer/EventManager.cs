using FreeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiPlayer
{
    public class EventManager:MonoBehaviour
    {
        UnityAction nowAction;

        GameObject eventParentObject;
        TMP_InputField inputBar;
        TMP_Text notify;

        int connectCnt = 0;
        public static EventManager instance;


        private void Awake()
        {
            eventParentObject = GameObject.Find("Input");
            inputBar = eventParentObject.transform.Find("InputBar").GetComponent<TMP_InputField>();
            notify = eventParentObject.transform.Find("Notify").GetComponent<TMP_Text>();
            instance = this;
        }

        private void Start()
        {
            ActivateInputName();
        }
        
        public void ActivateInputName()
        {
            var inputName = eventParentObject.transform.Find("PlayerNameInput");
            inputName.gameObject.SetActive(true);
            inputName.Find("SubmitButton").GetComponent<Button>().onClick.AddListener(InputName);
            notify.text = "플레이어 이름을 입력해주세요.";
            nowAction = InputName;
        }

        public void DeactivateInputName()
        {
            var inputName = eventParentObject.transform.Find("PlayerNameInput");
            inputName.gameObject.SetActive(false);
            inputBar.text = null;
            notify.text = null;
        }

        public void ActivateMatch()
        {
            var inputMatch = eventParentObject.transform.Find("MatchInput");
            inputMatch.gameObject.SetActive(true);
            inputMatch.Find("SubmitButton").GetComponent<Button>().onClick.AddListener(InputMatch);
            notify.text = "매칭을 시작하세요.";
            nowAction = InputMatch;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return)) nowAction();
        }

        public void InputName()
        {
            if (inputBar.text.Length > 0) GameManager.instance.players[0].playerName = inputBar.text;
            DeactivateInputName();
            ActivateMatch();
        }

        public void InputMatch()
        {
            CPacket msg = CPacket.create((short)Protocol.ENQUEUE_MATCHING);
            msg.push(GameManager.instance.players[0].playerName);
            NetworkManager.instance.send(msg);//.instance.send(msg);
            DeactivateInputMatch();
            ActivateWaitMatch();
        }

        public void ActivateWaitMatch()
        {
            notify.text = "상대를 찾는 중입니다...";
            inputBar.text = connectCnt.ToString();
        }

        public void UpdateWaitMatch(int cnt)
        {
            connectCnt = cnt;
            inputBar.text = connectCnt.ToString();
        }

        public void DeactivateInputMatch()
        {
            var inputMatch = eventParentObject.transform.Find("MatchInput");
            inputMatch.gameObject.SetActive(false);
            inputBar.text = null;
            notify.text = null;
        }

        public void DeactivateInput()
        {
            eventParentObject.SetActive(false);
        }
    }
}
