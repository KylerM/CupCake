using System.Collections.Generic;
using PlayerIOClient;

namespace CupCake.EE.Messages.Receive
{
    public sealed class TeleportEveryoneReceiveMessage : ReceiveMessage
    {
        public Dictionary<int, Point> Coordinates { get; private set; }
        public bool ResetCoins { get; private set; }

        public TeleportEveryoneReceiveMessage(Message message)
            : base(message)
        {
            this.Coordinates = new Dictionary<int, Point>();

            this.ResetCoins = message.GetBoolean(0);

            for (uint i = 1; i <= message.Count - 1u; i += 3)
            {
                this.Coordinates.Add(message.GetInteger(i),
                    new Point(message.GetInteger(i + 1u), message.GetInteger(i + 2u)));
            }
        }
    }
}