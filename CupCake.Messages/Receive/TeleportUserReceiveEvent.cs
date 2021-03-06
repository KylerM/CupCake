using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class TeleportUserReceiveEvent : ReceiveEvent, IUserPosReceiveEvent
    {
        public TeleportUserReceiveEvent(Message message)
            : base(message)
        {
            this.UserId = message.GetInteger(0);
            this.UserPosX = message.GetInteger(1);
            this.UserPosY = message.GetInteger(2);
        }

        public int BlockX
        {
            get { return this.UserPosX + 8 >> 4; }
        }

        public int BlockY
        {
            get { return this.UserPosY + 8 >> 4; }
        }

        public int UserPosX { get; set; }
        public int UserPosY { get; set; }
        public int UserId { get; set; }
    }
}