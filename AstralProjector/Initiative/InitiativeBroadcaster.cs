using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Astral.Projector.Initiative
{
    class InitiativeBroadcaster
    {
        UdpClient _client;

        public InitiativeBroadcaster(InitiativeManager manager)
        {
            manager.EventsUpdated += new Action<InitiativeManager>(manager_EventsUpdated);

            _client = new UdpClient();
            _client.Connect(new IPEndPoint(IPAddress.Broadcast, 38727));
        }

        void manager_EventsUpdated(InitiativeManager sender)
        {
            StringBuilder initiativeState = new StringBuilder();

            initiativeState.AppendLine("Initiative");
            foreach (var evnt in sender.Events)
            {
                initiativeState.AppendLine(Serialize(evnt));
            }
            initiativeState.AppendLine("End");
            initiativeState.AppendLine();

            byte[] messageBytes = ASCIIEncoding.UTF8.GetBytes(initiativeState.ToString());
            _client.Send(messageBytes, messageBytes.Length);
        }

        private string Serialize(Event e)
        {
            if (e is Actor)
            {
                Actor dude = (Actor)e;
                int healthPct = (int)Math.Round(100 * ((double)dude.CurrentHealth / (double)dude.MaxHealth));
                StringBuilder sb = new StringBuilder(1024);
                sb.Append(e.Name);
                sb.Append(";HP=");
                sb.Append(healthPct);
                sb.Append(";IsCasting=");
                sb.Append(dude.IsCasting ? "1" : "0");
                sb.Append(";IsDead=");
                sb.Append(dude.IsDead ? "1" : "0");
                return sb.ToString();
            }
            else
            {
                return e.Name;
            }
        }
    }
}
