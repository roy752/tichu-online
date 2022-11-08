using FreeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MultiPlayer
{
    public class FreeNetUnityService:MonoBehaviour
    {
        FreeNetEventManager eventManager;

        IPeer gameServer;

        CNetworkService service;

        public delegate void StatusChangeHandler(NETWORK_EVENT status);
        public StatusChangeHandler appcallback_on_status_changed;

        public delegate void MessageHandler(CPacket message);
        public MessageHandler appcallback_on_message;

        private void Awake()
        {
            CPacketBufferManager.initialize(10);
            eventManager = new FreeNetEventManager();
        }

        public void connect(string host, int port)
        {
            if(service != null)
            {
                Debug.LogError("Already connected.");
                return;
            }
            service = new CNetworkService();

            CConnector connector = new CConnector(service);

            connector.connected_callback += on_connected_gameserver;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            connector.connect(endPoint);
            
        }
        public bool is_connected()
        {
            return gameServer != null;
        }

        public void on_connected_gameserver(CUserToken server_token)
        {
            gameServer = new RemoteServerPeer(server_token);
            ((RemoteServerPeer)gameServer).set_event_manager(eventManager);

            //유니티 애플리케이션으로 이벤트를 넘겨주기 위해서 매니저에 큐잉시켜 준다.
            eventManager.enqueue(NETWORK_EVENT.connected);
        }

        // Update is called once per frame
        void Update()
        {
            // 수신된 메시지에 대한 콜백.
            if (eventManager.has_message())
            {
                CPacket msg = eventManager.dequeue_network_message();
                appcallback_on_message?.Invoke(msg);

                //네트워크 발생 이벤트에 대한 콜백.
                if (eventManager.has_event())
                {
                    NETWORK_EVENT status = eventManager.dequeue();
                    appcallback_on_status_changed?.Invoke(status);
                }
            }
        }
        public void send(CPacket msg)
        {
            try
            {
                gameServer.Send(msg);
                CPacket.destroy(msg);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 정상적인 종료시에는 OnApplicationQuit매소드에서 disconnect를 호출해 줘야 유니티가 hang되지 않는다.
        /// </summary>
        void OnApplicationQuit()
        {
            if (gameServer != null)
            {
                ((RemoteServerPeer)gameServer).token.disconnect();
            }
        }
    }
}
