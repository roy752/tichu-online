using FreeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MultiPlayer
{
    public enum NETWORK_EVENT:byte
    {
        connected,
        disconnected,
        end
    }
    public class FreeNetEventManager
    {
        object cs_event;

        Queue<NETWORK_EVENT> events;

        Queue<CPacket> network_message_events;

        public FreeNetEventManager()
        {
            events = new Queue<NETWORK_EVENT>();
            network_message_events = new Queue<CPacket>();
            cs_event = new object();
        }

        public void enqueue(NETWORK_EVENT eventType)
        {
            lock(cs_event)
            {
                events.Enqueue(eventType);
            }
        }
        public bool has_event()
        {
            lock(cs_event)
            {
                return events.Count > 0;
            }
        }
        public NETWORK_EVENT dequeue()
        {
            lock(cs_event)
            {
                return events.Dequeue();
            }
        }
        public void enqueue_network_message(CPacket buffer)
        {
            lock(cs_event)
            {
                network_message_events.Enqueue(buffer);
            }
        }

        public bool has_message()
        {
            lock (cs_event)
            {
                return network_message_events.Count > 0;
            }
        }

        public CPacket dequeue_network_message()
        {
            lock(cs_event)
            {
                return network_message_events.Dequeue();
            }
        }
    }
}
