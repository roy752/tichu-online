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

        TMP_InputField comp;

        private void Start()
        {
            var obj = GameObject.Find("Input");
            var obj2 = obj.transform.Find("PlayerNameInput");
            obj2.Find("SubmitButton").GetComponent<Button>().onClick.AddListener(InputName);
            var txt = obj2.Find("PlayerNameInput").gameObject;
            comp = txt.GetComponent<TMP_InputField>();
            nowAction = InputName;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return)) nowAction();
        }

        public void InputName()
        {
            if (comp.text.Length > 0) MultiPlayer.GameManager.instance.playerName = comp.text;
            Debug.Log("냈다");
        }
    }
}
