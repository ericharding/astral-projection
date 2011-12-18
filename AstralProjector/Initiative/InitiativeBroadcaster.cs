using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Threading;

namespace Astral.Projector.Initiative
{
    class InitiativeBroadcaster
    {
        UdpClient _client;
        DispatcherTimer _timer;
        long _version = 0;
        InitiativeManager _manager;
        int _reliability_counter = 0;

        public InitiativeBroadcaster(InitiativeManager manager)
        {
            _manager = manager;
            manager.EventsUpdated += new Action<InitiativeManager>(manager_EventsUpdated);

            connect();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1.0);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        private void connect()
        {
            if (_client != null)
            {
                _client.Close();
            }

            int count = 0;
            while (count < 5)
            {
                count++;
                try
                {
                    _client = new UdpClient();
                    _client.Connect(new IPEndPoint(IPAddress.Broadcast, 38727));
                    return;
                }
                catch
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

            _client = new UdpClient();
            _client.Connect(new IPEndPoint(IPAddress.Broadcast, 38727));
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            BroadcastNow(_manager);
        }

        void manager_EventsUpdated(InitiativeManager sender)
        {
            _version++;
            BroadcastNow(sender);
        }

        private void BroadcastNow(InitiativeManager sender)
        {

            _reliability_counter++;
            if (_reliability_counter % 60 == 0)
            {
                // recreate the socket every 60seconds whether we need to or not.
                connect();
            }

            StringBuilder initiativeState = new StringBuilder();

            initiativeState.Append("Initiative ");
            initiativeState.AppendLine(_version.ToString());
            foreach (var evnt in sender.Events)
            {
                initiativeState.AppendLine(Serialize(evnt));
            }
            initiativeState.AppendLine("End");
            initiativeState.AppendLine();

            byte[] messageBytes = ASCIIEncoding.UTF8.GetBytes(initiativeState.ToString());
            try
            {
                _client.Send(messageBytes, messageBytes.Length);
            }
            catch
            {
                /* network go bye bye? */
                connect();
            }
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
