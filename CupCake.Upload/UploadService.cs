﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CupCake.Core;
using CupCake.Core.Events;
using CupCake.Core.Log;
using CupCake.Messages.Blocks;
using CupCake.Messages.Receive;
using CupCake.Messages.Send;
using CupCake.Messages.User;
using CupCake.Room;
using CupCake.World;

namespace CupCake.Upload
{
    public sealed class UploadService : CupCakeService
    {
        private readonly Queue<UploadRequestEvent> _checkQueue = new Queue<UploadRequestEvent>();
        private bool[,,] _uploaded;
        private DequeWorker _workThread;
        private WorldService _world;

        public int Count
        {
            get { return this._workThread.Count; }
        }

        public int LagCheckCount
        {
            get { return this._checkQueue.Count; }
        }

        protected override void Enable()
        {
            this.ServiceLoader.EnableComplete += this.ServiceLoader_EnableComplete;

            this._workThread = new DequeWorker();

            this.Events.Bind<UploadRequestEvent>(this.OnUploadRequest, EventPriority.Lowest);

            this.Events.Bind<PlaceWorldEvent>(this.OnBlockPlace, EventPriority.High);
            this.Events.Bind<InitReceiveEvent>(this.OnInitComplete, EventPriority.High);
            this.Events.Bind<AccessRoomEvent>(this.OnAccessRightChange, EventPriority.High);
            this.Events.Bind<ClearReceiveEvent>(this.OnClear, EventPriority.High);
            this.Events.Bind<ResetReceiveEvent>(this.OnReset, EventPriority.High);
        }

        private void OnReset(object sender, ResetReceiveEvent e)
        {
            this.Reset(this._world.RoomWidth, this._world.RoomHeight);
        }

        private void OnClear(object sender, ClearReceiveEvent e)
        {
            this.Reset(e.RoomWidth, e.RoomHeight);
        }

        private void OnAccessRightChange(object sender, AccessRoomEvent e)
        {
            if (e.NewRights >= AccessRight.Edit)
            {
                this._workThread.Start();
            }
            else
            {
                this._workThread.Stop();
            }
        }

        private void ServiceLoader_EnableComplete(object sender, EventArgs e)
        {
            this._world = this.ServiceLoader.Get<WorldService>();
            this.ServiceLoader.Get<RoomService>();
        }

        private void OnInitComplete(object sender, InitReceiveEvent e)
        {
            this.ResetUploaded(e.RoomWidth, e.RoomHeight);
        }

        private void OnUploadRequest(object sender, UploadRequestEvent e)
        {
            Action task = () =>
            {
                if (this.Send(e))
                {
                    Thread.Sleep(10);
                }

                if (this._workThread.Count == 0)
                {
                    // Make sure this is the last work item
                    // BugFix for single core machines
                    this.SynchronizePlatform.Do(() =>
                    {
                        if (this._workThread.Count != 0)
                            return;

                        this.DoLagCheck(true);
                    });
                }
            };

            if (e.IsUrgent)
            {
                this._workThread.QueueFront(task);
            }
            else
            {
                this._workThread.QueueBack(task);
            }
        }

        /// <summary>
        ///     Uploads the given block and makes sure it gets placed
        /// </summary>
        private bool Send(UploadRequestEvent request)
        {
            IBlockPlaceSendEvent e = request.SendEvent;

            request.SendTries++;
            // If not block already exists
            if (!this.IsUploaded(e))
            {
                this.RaiseEvent(e);
                this.EnqueueCheck(request);
                return true;
            }

            request.Verified = true;
            return false;
        }

        /// <summary>
        ///     Adds the request to the lag checking queue
        /// </summary>
        /// <param name="request"></param>
        private void EnqueueCheck(UploadRequestEvent request)
        {
            IBlockPlaceSendEvent e = request.SendEvent;

            lock (this._checkQueue)
            {
                if (!this._uploaded[(int)e.Layer, e.X, e.Y] && request.SendTries < 5)
                {
                    this._uploaded[(int)e.Layer, e.X, e.Y] = true;

                    this._checkQueue.Enqueue(request);
                }
            }
        }

        /// <summary>
        ///     Calls the correct event for the given BlockPlaceSendEvent.
        ///     Note that this does NOT check if the block was skipped!
        /// </summary>
        /// <param name="e"></param>
        public void RaiseEvent(IBlockPlaceSendEvent e)
        {
            var blockEvent = e as BlockPlaceSendEvent;
            if (blockEvent != null)
            {
                this.Events.Raise(blockEvent);
                return;
            }

            var coinEvent = e as CoinDoorPlaceSendEvent;
            if (coinEvent != null)
            {
                this.Events.Raise(coinEvent);
                return;
            }
            var labelEvent = e as LabelPlaceSendEvent;
            if (labelEvent != null)
            {
                this.Events.Raise(labelEvent);
                return;
            }
            var worldPortalEvent = e as WorldPortalPlaceSendEvent;
            if (worldPortalEvent != null)
            {
                this.Events.Raise(worldPortalEvent);
                return;
            }
            var rotatableEvent = e as RotatablePlaceSendEvent;
            if (rotatableEvent != null)
            {
                this.Events.Raise(rotatableEvent);
                return;
            }
            var soundEvent = e as SoundPlaceSendEvent;
            if (soundEvent != null)
            {
                this.Events.Raise(soundEvent);
                return;
            }
            var portalEvent = e as PortalPlaceSendEvent;
            if (portalEvent != null)
            {
                this.Events.Raise(portalEvent);
            }
        }

        private void OnBlockPlace(object sender, PlaceWorldEvent e)
        {
            if (this._uploaded[(int)e.WorldBlock.Layer, e.WorldBlock.X, e.WorldBlock.Y])
            {
                this.DoLagCheck(false);
            }
        }

        private void DoLagCheck(bool emptyQueue)
        {
            lock (this._checkQueue)
            {
                while (this._checkQueue.Count > 0)
                {
                    UploadRequestEvent request = this._checkQueue.Dequeue();
                    IBlockPlaceSendEvent er = request.SendEvent;

                    this._uploaded[(int)er.Layer, er.X, er.Y] = false;
                    if (this.IsUploaded(er))
                    {
                        request.Verified = true;

                        if (emptyQueue)
                            continue;

                        break;
                    }

                    request.IsUrgent = true;
                    this.Events.Raise(request);
                }

                if (this._workThread.Count == 0)
                {
                    this.Events.Raise(new FinishUploadEvent());
                }
            }
        }

        private bool IsUploaded(IBlockPlaceSendEvent e)
        {
            return (this._world[e.Layer, e.X, e.Y].IsSame(e));
        }

        public void ClearQueue()
        {
            this._workThread.Clear();
            this.DoLagCheck(true);
        }

        private void Reset(int width, int height)
        {
            bool enableLater = this._workThread.IsAlive;

            this._workThread.Stop();
            this.ClearQueue();
            this.ResetUploaded(width, height);
            this._workThread.Clear();
            if (enableLater)
                this._workThread.Start();
        }

        private void ResetUploaded(int width, int height)
        {
            this._uploaded = new bool[2, width, height];
        }

        public void UploadBlock(int x, int y, Block block)
        {
            this.UploadBlock(Layer.Foreground, x, y, block);
        }

        public void UploadBlock(Layer layer, int x, int y, Block block)
        {
            this.Events.Raise(this.GetBlock(layer, x, y, block));
        }

        public void UploadCoinDoor(int x, int y, CoinDoorBlock block, uint coinsToCollect)
        {
            this.Events.Raise(this.GetCoinDoor(x, y, block, coinsToCollect));
        }

        public void UploadLabel(int x, int y, LabelBlock block, string text)
        {
            this.Events.Raise(this.GetLabel(x, y, block, text));
        }

        public void UploadPortal(int x, int y, PortalBlock block, uint id, uint target, PortalRotation rotation)
        {
            this.Events.Raise(this.GetPortal(x, y, block, id, target, rotation));
        }

        public void UploadWorldPortal(int x, int y, WorldPortalBlock block, string roomId)
        {
            this.Events.Raise(this.GetWorldPortal(x, y, block, roomId));
        }

        public void UploadSound(int x, int y, SoundBlock block, uint soundId)
        {
            this.Events.Raise(this.GetSound(x, y, block, soundId));
        }

        public void UploadRotatable(int x, int y, RotatableBlock block, uint rotation)
        {
            this.Events.Raise(this.GetRotatable(x, y, block, rotation));
        }

        public UploadRequestEvent GetBlock(Layer layer, int x, int y, Block block)
        {
            var e = new BlockPlaceSendEvent(layer, x, y, block);
            return new UploadRequestEvent(e);
        }

        public UploadRequestEvent GetCoinDoor(int x, int y, CoinDoorBlock block, uint coinsToCollect)
        {
            var e = new CoinDoorPlaceSendEvent(Layer.Foreground, x, y, block, coinsToCollect);
            return new UploadRequestEvent(e);
        }

        public UploadRequestEvent GetLabel(int x, int y, LabelBlock block, string text)
        {
            var e = new LabelPlaceSendEvent(Layer.Foreground, x, y, block, text);
            return new UploadRequestEvent(e);
        }

        public UploadRequestEvent GetPortal(int x, int y, PortalBlock block, uint id, uint target,
            PortalRotation rotation)
        {
            var e = new PortalPlaceSendEvent(Layer.Foreground, x, y, block, id, target, rotation);
            return new UploadRequestEvent(e);
        }

        public UploadRequestEvent GetWorldPortal(int x, int y, WorldPortalBlock block, string roomId)
        {
            var e = new WorldPortalPlaceSendEvent(Layer.Foreground, x, y, block, roomId);
            return new UploadRequestEvent(e);
        }

        public UploadRequestEvent GetSound(int x, int y, SoundBlock block, uint soundId)
        {
            var e = new SoundPlaceSendEvent(Layer.Foreground, x, y, block, soundId);
            return new UploadRequestEvent(e);
        }

        public UploadRequestEvent GetRotatable(int x, int y, RotatableBlock block, uint rotation)
        {
            var e = new RotatablePlaceSendEvent(Layer.Foreground, x, y, block, rotation);
            return new UploadRequestEvent(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._workThread.Stop();
            }

            base.Dispose(disposing);
        }
    }
}