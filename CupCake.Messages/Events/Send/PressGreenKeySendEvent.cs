using PlayerIOClient;

namespace CupCake.Messages.Events.Send
{
    public class PressGreenKeySendEvent : SendEvent, IEncryptedSendEvent
    {
        public string Encryption { get; set; }

        public override Message GetMessage()
        {
            return Message.Create(this.Encryption + "g");
        }
    }
}