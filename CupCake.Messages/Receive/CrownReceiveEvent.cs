using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class CrownReceiveEvent : ReceiveEvent, IUserEvent
    {
        public CrownReceiveEvent(Message message)
            : base(message)
        {
            this.UserId = message.GetInteger(0);
        }

        public int UserId { get; set; }
    }
}