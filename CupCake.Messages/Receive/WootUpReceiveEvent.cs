using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class WootUpReceiveEvent : ReceiveEvent, IUserReceiveEvent
    {
        public WootUpReceiveEvent(Message message)
            : base(message)
        {
            this.UserId = message.GetInteger(0);
        }

        public int UserId { get; set; }
    }
}