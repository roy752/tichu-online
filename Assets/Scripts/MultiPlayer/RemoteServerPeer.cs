using FreeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPlayer
{
    public class RemoteServerPeer : IPeer
    {
        public CUserToken token { get; private set; }
        WeakReference freenet_eventmanager;

        public RemoteServerPeer(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
        }

        public void set_event_manager(FreeNetEventManager event_manager)
        {
            freenet_eventmanager = new WeakReference(event_manager);
        }

        /// <summary>
        /// 메시지를 수신했을 때 호출된다.
        /// 파라미터로 넘어온 버퍼는 워커 스레드에서 재사용 되므로 복사한 뒤 어플리케이션으로 넘겨준다.
        /// </summary>
        /// <param name="buffer"></param>
        void IPeer.OnMessage(Const<byte[]> buffer)
        {
            // 버퍼를 복사한 뒤 CPacket클래스로 감싼 뒤 넘겨준다.
            // CPacket클래스 내부에서는 참조로만 들고 있는다.
            byte[] app_buffer = new byte[buffer.Value.Length];
            Array.Copy(buffer.Value, app_buffer, buffer.Value.Length);
            CPacket msg = new CPacket(app_buffer, this);
            (freenet_eventmanager.Target as FreeNetEventManager).enqueue_network_message(msg);
        }

        void IPeer.OnRemoved()
        {
            (freenet_eventmanager.Target as FreeNetEventManager).enqueue(NETWORK_EVENT.disconnected);
        }

        void IPeer.Send(CPacket msg)
        {
            token.send(msg);
        }

        void IPeer.Disconnect()
        {
        }

        void IPeer.ProcessPacket(CPacket msg)
        {
        }
    }
}
