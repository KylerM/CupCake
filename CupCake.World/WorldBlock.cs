using System;
using System.CodeDom;
using System.Diagnostics;
using System.Reflection.Emit;
using CupCake.Core;
using CupCake.Core.Metadata;
using CupCake.Messages;
using CupCake.Messages.Blocks;
using CupCake.Messages.Send;

namespace CupCake.World
{
    [DebuggerDisplay("Block = {Block}, Data = {DebuggerData()}")]
    public class WorldBlock : MetadataProvider
    {
        private BlockData _data;

        internal WorldBlock(MetadataPlatform metadataPlatform, Layer layer, int x, int y, Block block)
            : base(metadataPlatform)
        {
            this.Layer = layer;
            this.X = x;
            this.Y = y;
            this.SetBlock(block);
        }

        protected override object MetadataKey
        {
            get { return new Point3D(this.Layer, this.X, this.Y); }
        }

        public Layer Layer { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public virtual BlockType BlockType { get; private set; }
        public Block Block { get; private set; }
        
        [Obsolete("Use the Get<T> and Set<T> methods instead.")]
        public MetadataStore Metadata
        {
            get { return this.MetadataStore; }
        }

        public string Text
        {
            get
            {
                if (this.BlockType != BlockType.Label)
                    throw new InvalidOperationException("This property can only be accessed on Label blocks.");

                return this._data.Text;
            }
        }

        public string WorldPortalTarget
        {
            get
            {
                if (this.BlockType != BlockType.WorldPortal)
                    throw new InvalidOperationException("This property can only be accessed on WorldPortal blocks.");

                return this._data.WorldPortalTarget;
            }
        }

        public uint CoinsToCollect
        {
            get
            {
                if (this.BlockType != BlockType.CoinDoor)
                    throw new InvalidOperationException("This property can only be accessed on CoinDoor blocks.");

                return this._data.CoinsToCollect;
            }
        }

        public uint PortalId
        {
            get
            {
                if (this.BlockType != BlockType.Portal)
                    throw new InvalidOperationException("This property can only be accessed on Portal blocks.");

                return this._data.PortalId;
            }
        }

        public uint PortalTarget
        {
            get
            {
                if (this.BlockType != BlockType.Portal)
                    throw new InvalidOperationException("This property can only be accessed on Portal blocks.");

                return this._data.PortalTarget;
            }
        }

        public PortalRotation PortalRotation
        {
            get
            {
                if (this.BlockType != BlockType.Portal)
                    throw new InvalidOperationException("This property can only be accessed on Portal blocks.");

                return this._data.PortalRotation;
            }
        }

        public uint SoundId
        {
            get
            {
                if (this.BlockType != BlockType.Sound)
                    throw new InvalidOperationException("This property can only be accessed on Sound blocks.");

                return this._data.SoundId;
            }
        }

        public uint Rotation
        {
            get
            {
                if (this.BlockType != BlockType.Rotatable)
                    throw new InvalidOperationException("This property can only be accessed on Rotatable blocks.");

                return this._data.Rotation;
            }
        }

        internal void SetBlock(Block block)
        {
            this.BlockType = BlockType.Normal;
            this.Block = block;
            this._data = new BlockData();
        }

        internal void SetCoinDoor(CoinDoorBlock block, uint coinsToCollect)
        {
            this.BlockType = BlockType.CoinDoor;
            this.Block = (Block)block;

            this._data = new BlockData
            {
                CoinsToCollect = coinsToCollect
            };
        }

        internal void SetPortal(PortalBlock block, uint portalId, uint portalTarget, PortalRotation portalRotation)
        {
            this.BlockType = BlockType.Portal;
            this.Block = (Block)block;

            this._data = new BlockData
            {
                PortalId = portalId,
                PortalTarget = portalTarget,
                PortalRotation = portalRotation
            };
        }

        internal void SetSound(SoundBlock block, uint soundId)
        {
            this.BlockType = BlockType.Sound;
            this.Block = (Block)block;

            this._data = new BlockData
            {
                SoundId = soundId
            };
        }

        internal void SetRotatable(RotatableBlock block, uint rotation)
        {
            this.BlockType = BlockType.Rotatable;
            this.Block = (Block)block;

            this._data = new BlockData
            {
                Rotation = rotation
            };
        }

        internal void SetLabel(LabelBlock block, string text)
        {
            this.BlockType = BlockType.Label;
            this.Block = (Block)block;

            this._data = new BlockData
            {
                Text = text
            };
        }

        internal void SetWorldPortal(WorldPortalBlock block, string worldId)
        {
            this.BlockType = BlockType.WorldPortal;
            this.Block = (Block)block;

            this._data = new BlockData
            {
                WorldPortalTarget = worldId
            };
        }

        public bool IsSame(IBlockPlaceSendEvent other)
        {
            if (other == null)
            {
                return false;
            }

            bool result = this.Block == other.Block;

            if (other is BlockPlaceSendEvent)
            {
                return result;
            }
            var coinEvent = other as CoinDoorPlaceSendEvent;
            if (coinEvent != null)
            {
                if (this._data.CoinsToCollect != coinEvent.CoinsToCollect)
                    result = false;
                return result;
            }
            var labelEvent = other as LabelPlaceSendEvent;
            if (labelEvent != null)
            {
                if (this._data.Text != labelEvent.Text)
                    result = false;
                return result;
            }
            var worldPortalEvent = other as WorldPortalPlaceSendEvent;
            if (worldPortalEvent != null)
            {
                if (this._data.WorldPortalTarget != worldPortalEvent.WorldPortalTarget)
                    result = false;
                return result;
            }
            var rotatableEvent = other as RotatablePlaceSendEvent;
            if (rotatableEvent != null)
            {
                if (this._data.Rotation != rotatableEvent.Rotation)
                    result = false;
                return result;
            }
            var soundEvent = other as SoundPlaceSendEvent;
            if (soundEvent != null)
            {
                if (this._data.SoundId != soundEvent.SoundId)
                    result = false;
                return result;
            }
            var portalEvent = other as PortalPlaceSendEvent;
            if (portalEvent != null)
            {
                if (this._data.PortalId != portalEvent.PortalId ||
                    this._data.PortalTarget != portalEvent.PortalTarget ||
                    this._data.PortalRotation != portalEvent.PortalRotation)
                    result = false;

                return result;
            }

            throw new NotSupportedException("The given send message is not supported.");
        }

        public IBlockPlaceSendEvent ToEvent()
        {
            switch (this.BlockType)
            {
                case BlockType.Normal:
                    return new BlockPlaceSendEvent(this.Layer, this.X, this.Y, this.Block);
                case BlockType.CoinDoor:
                    return new CoinDoorPlaceSendEvent(this.Layer, this.X, this.Y, (CoinDoorBlock)this.Block, this.CoinsToCollect);
                case BlockType.Portal:
                    return new PortalPlaceSendEvent(this.Layer, this.X, this.Y, (PortalBlock)this.Block, this.PortalId, this.PortalTarget, this.PortalRotation);
                case BlockType.Sound:
                    return new SoundPlaceSendEvent(this.Layer, this.X, this.Y, (SoundBlock)this.Block, this.SoundId);
                case BlockType.Label:
                    return new LabelPlaceSendEvent(this.Layer, this.X, this.Y, (LabelBlock)this.Block, this.Text);
                case BlockType.Rotatable:
                    return new RotatablePlaceSendEvent(this.Layer, this.X, this.Y, (RotatableBlock)this.Block, this.Rotation);
                case BlockType.WorldPortal:
                    return new WorldPortalPlaceSendEvent(this.Layer, this.X, this.Y, (WorldPortalBlock)this.Block, this.WorldPortalTarget);
                default:
                    throw new NotSupportedException("The given send message is not supported.");
            }
        }

// ReSharper disable once UnusedMember.Local
        private string DebuggerData()
        {
            switch (this.BlockType)
            {
                case BlockType.Normal:
                    return String.Format("Layer = {0}", this.Layer);
                case BlockType.CoinDoor:
                    return String.Format("CoinsToCollect = {0}", this.CoinsToCollect);
                case BlockType.Rotatable:
                    return String.Format("Rotation = {0}", this.Rotation);
                case BlockType.Sound:
                    return String.Format("SoundId = {0}", this.SoundId);
                case BlockType.Label:
                    return String.Format("Text = {0}", this.Text);
                case BlockType.Portal:
                    return String.Format("Id = {0}, Target = {1}, Rotation = {2}", this.PortalId, this.PortalTarget,
                        this.PortalRotation);
                case BlockType.WorldPortal:
                    return String.Format("Target = {0}", this.WorldPortalTarget);
                default:
                    return String.Empty;
            }
        }

        public WorldBlock Clone()
        {
            return new WorldBlock(this.MetadataPlatform, this.Layer, this.X, this.Y, this.Block)
            {
                _data = this._data.Clone(),
                BlockType = this.BlockType
            };
        }

        private struct BlockData
        {
            internal string Text { get; set; }
            internal string WorldPortalTarget { get; set; }
            internal uint CoinsToCollect { get; set; }
            internal uint PortalId { get; set; }
            internal uint PortalTarget { get; set; }
            internal PortalRotation PortalRotation { get; set; }
            internal uint SoundId { get; set; }
            internal uint Rotation { get; set; }

            internal BlockData Clone()
            {
                return new BlockData
                {
                    Text = this.Text,
                    WorldPortalTarget = this.WorldPortalTarget,
                    CoinsToCollect = this.CoinsToCollect,
                    PortalId = this.PortalId,
                    PortalTarget = this.PortalTarget,
                    PortalRotation = this.PortalRotation,
                    SoundId = this.SoundId,
                    Rotation = this.Rotation
                };
            }
        }
    }
}