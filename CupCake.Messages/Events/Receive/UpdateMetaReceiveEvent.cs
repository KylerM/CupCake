using PlayerIOClient;

namespace CupCake.Messages.Events.Receive
{
    public class UpdateMetaReceiveEvent : ReceiveEvent
    {
        public UpdateMetaReceiveEvent(Message message)
            : base(message)
        {
            this.OwnerUsername = message.GetString(0);
            this.WorldName = message.GetString(1);
            this.Plays = message.GetInteger(2);
            this.CurrentWoots = message.GetInteger(3);
            this.TotalWoots = message.GetInteger(4);
        }

        public int CurrentWoots { get; private set; }
        public string OwnerUsername { get; private set; }
        public int Plays { get; private set; }
        public int TotalWoots { get; private set; }
        public string WorldName { get; private set; }
    }
}