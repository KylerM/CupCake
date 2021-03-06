using CupCake.Messages.Blocks;
using PlayerIOClient;

namespace CupCake.Messages.Send
{
    public class PortalPlaceSendEvent : SendEvent, IBlockPlaceSendEvent
    {
        public PortalPlaceSendEvent(Layer layer, int x, int y, PortalBlock block, uint portalId, uint portalTarget,
            PortalRotation portalRotation)
        {
            this.Block = block;
            this.X = x;
            this.Y = y;
            this.Layer = BlockUtils.CorrectLayer((Block)block, layer);

            this.PortalId = portalId;
            this.PortalTarget = portalTarget;
            this.PortalRotation = portalRotation;
        }

        public PortalBlock Block { get; set; }
        public PortalRotation PortalRotation { get; set; }
        public uint PortalId { get; set; }
        public uint PortalTarget { get; set; }

        Block IBlockPlaceSendEvent.Block
        {
            get { return (Block)this.Block; }
            set { this.Block = (PortalBlock)value; }
        }

        public Layer Layer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public string Encryption { get; set; }

        public override Message GetMessage()
        {
            return Message.Create(this.Encryption, (int)this.Layer, this.X, this.Y, (int)this.Block,
                (uint)this.PortalRotation, this.PortalId, this.PortalTarget);
        }
    }
}