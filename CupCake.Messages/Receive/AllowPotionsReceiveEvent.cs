using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class AllowPotionsReceiveEvent : ReceiveEvent
    {
        public AllowPotionsReceiveEvent(Message message)
            : base(message)
        {
            this.Allowed = message.GetBoolean(0);
        }

        public bool Allowed { get; set; }
    }
}