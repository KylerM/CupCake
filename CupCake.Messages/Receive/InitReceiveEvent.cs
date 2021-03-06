using PlayerIOClient;

namespace CupCake.Messages.Receive
{
    public class InitReceiveEvent : ReceiveEvent, IUserPosReceiveEvent, IMetadataReceiveMessage
    {
        public InitReceiveEvent(Message message)
            : base(message)
        {
            this.OwnerUsername = message.GetString(0);
            this.WorldName = message.GetString(1);
            this.Plays = message.GetInteger(2);
            this.CurrentWoots = message.GetInteger(3);
            this.TotalWoots = message.GetInteger(4);
            this.Encryption = message.GetString(5);
            this.UserId = message.GetInteger(6);
            this.SpawnX = message.GetInteger(7);
            this.SpawnY = message.GetInteger(8);
            this.Username = message.GetString(9);
            this.CanEdit = message.GetBoolean(10);
            this.IsOwner = message.GetBoolean(11);
            this.RoomWidth = message.GetInteger(12);
            this.RoomHeight = message.GetInteger(13);
            this.IsTutorialRoom = message.GetBoolean(14);
            this.Gravity = message.GetDouble(15);
            this.AllowPotions = message.GetBoolean(16);
        }

        public bool AllowPotions { get; set; }
        public bool CanEdit { get; set; }
        public string Encryption { get; set; }
        public double Gravity { get; set; }
        public bool IsOwner { get; set; }
        public bool IsTutorialRoom { get; set; }
        public int RoomWidth { get; set; }
        public int RoomHeight { get; set; }
        public int SpawnX { get; set; }
        public int SpawnY { get; set; }
        public string Username { get; set; }
        public int CurrentWoots { get; set; }
        public string OwnerUsername { get; set; }
        public int Plays { get; set; }
        public int TotalWoots { get; set; }
        public string WorldName { get; set; }
        public int UserId { get; set; }

        int IUserPosReceiveEvent.UserPosX
        {
            get { return this.SpawnX; }
            set { this.SpawnX = value; }
        }

        int IUserPosReceiveEvent.UserPosY
        {
            get { return this.SpawnY; }
            set { this.SpawnY = value; }
        }
    }
}