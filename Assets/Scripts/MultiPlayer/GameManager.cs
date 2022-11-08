using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MultiPlayer.Define;

namespace MultiPlayer
{
    public class GameManager : MonoBehaviour
    {

        [HideInInspector]
        public GameObject cardsParent;

        public static GameManager instance;

        public string playerName=null;

        void Awake()
        {
            InitializeVariables();
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(playerName!=null) Debug.Log(playerName);
        }

        void InitializeVariables()
        {
            cardsParent = GameObject.Find(cardsParentObjectName);
        }
    }
}
