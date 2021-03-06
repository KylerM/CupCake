using CupCake.Messages.User;
using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class FaceReceiveEvent : ReceiveEvent, IUserReceiveEvent
    {
        public FaceReceiveEvent(Message message)
            : base(message)
        {
            this.UserId = message.GetInteger(0);
            this.Face = (Smiley)message.GetInteger(1);
        }

        public Smiley Face { get; set; }
        public int UserId { get; set; }
    }
}