using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class LeftReceiveEvent : ReceiveEvent, IUserReceiveEvent
    {
        public LeftReceiveEvent(Message message)
            : base(message)
        {
            this.UserId = message.GetInteger(0);
        }

        public int UserId { get; set; }
    }
}