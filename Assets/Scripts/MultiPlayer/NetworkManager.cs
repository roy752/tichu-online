using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FreeNet;
using System.Net.Sockets;

namespace MultiPlayer
{
    public class NetworkManager: MonoBehaviour
    {
        FreeNetUnityService gameServer;

        public static NetworkManager instance;

        private void Awake()
        {
            gameServer = gameObject.AddComponent<FreeNetUnityService>();
            gameServer.appcallback_on_status_changed += on_status_changed;
            gameServer.appcallback_on_message += on_message;
            instance = this;
            DontDestroyOnLoad(instance);
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

                    }
                    break;
                case NETWORK_EVENT.disconnected:
                    Debug.Log("disconnected");
                    break;
            }
        }

        void on_message(CPacket msg)
        {
            Protocol protocolId = (Protocol)msg.pop_protocol_id();
            switch(protocolId)
            {
                case Protocol.WAIT:
                    byte cnt = msg.pop_byte();
                    EventManager.instance.UpdateWaitMatch(cnt);
                    break;
                case Protocol.START_LOADING:
                    CPacket mssg = CPacket.create((short)Protocol.LOADING_COMPLETED);
                    send(mssg);
                    break;
                case Protocol.GAME_START:
                    byte id = msg.pop_byte();
                    for(int i=0; i<4; ++i)
                    {
                        byte pid = msg.pop_byte();
                        GameManager.instance.players[i].playerNumber = Util.GetRelativePlayerIdx(pid, id);
                        string playerName = msg.pop_string();
                        GameManager.instance.players[i].playerName = playerName;
                    }
                    List<int> ids = new List<int>();
                    for (int i = 0; i < 8; ++i) ids.Add(msg.pop_int32());
                    GameManager.instance.GameStart(ids);
                    break;
                default: Debug.Log("뭐야 이거" + protocolId.ToString()); break;
            }
        }

        public void send(CPacket msg)
        {
            gameServer.send(msg);
        }
    }
}
