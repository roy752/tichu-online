using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FreeNet;
using System.Net.Sockets;

namespace Assets.Scripts.MultiPlayer
{
    public class NetworkManager: MonoBehaviour
    {
        FreeNetUnityService gameServer;

        private void Awake()
        {
            gameServer = gameObject.AddComponent<FreeNetUnityService>();
            gameServer.appcallback_on_status_changed += on_status_changed;
            gameServer.appcallback_on_message += on_message;
        }

        private void Start()
        {
            connect();
        }
        private void connect()
        {
            gameServer.connect("127.0.0.1", 7979);
        }

        void on_status_changed(NETWORK_EVENT status)
        {
            switch(status)
            {
                case NETWORK_EVENT.connected:
                    {
                        CPacket msg = CPacket.create((short)Procotol.CHAT_MSG_REQ);
                        msg.push("Hello!");
                        gameServer.send(msg);
                    }
                    break;
                case NETWORK_EVENT.disconnected:
                    Debug.Log("disconnected");
                    break;
            }
        }
    }
}
