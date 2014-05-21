﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CupCake.Core.Events
{
    public class EventHandle<T> where T : Event
    {
        private static readonly Dictionary<int, EventHandle<T>> _eventManagers = new Dictionary<int, EventHandle<T>>();

        private readonly LinkedList<EventHandler<T>> _eventHandlers = new LinkedList<EventHandler<T>>();

        private EventHandle()
        {
        }

        public int Count
        {
            get
            {
                lock (this._eventHandlers)
                {
                    return this._eventHandlers.Count;
                }
            }
        }

        public void Clear()
        {
            lock (this._eventHandlers)
            {
                this._eventHandlers.Clear();
            }
        }

        public bool Contains(EventHandler<T> item)
        {
            lock (this._eventHandlers)
            {
                return this._eventHandlers.Contains(item);
            }
        }

        public bool Remove(EventHandler<T> item)
        {
            lock (this._eventHandlers)
            {
                return this._eventHandlers.Remove(item);
            }
        }

        internal static EventHandle<T> Get(int id)
        {
            lock (_eventManagers)
            {
                if (!_eventManagers.ContainsKey(id))
                    _eventManagers[id] = new EventHandle<T>();

                return _eventManagers[id];
            }
        }

        public void Bind(EventHandler<T> callback, EventPriority priority = EventPriority.Normal)
        {
            lock (this._eventHandlers)
            {
                switch (priority)
                {
                    case EventPriority.Normal:
                        this._eventHandlers.AddLast(callback);
                        break;
                    case EventPriority.BeforeMost:
                        this._eventHandlers.AddFirst(callback);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown priority.");
                }
            }
        }

        public void Raise(object sender, T e)
        {
            EventHandler<T>[] handlers;
            lock (this._eventHandlers)
            {
                handlers = this._eventHandlers.ToArray();
            }

            foreach (var handler in handlers)
            {
                handler.Invoke(sender, e);
            }
        }
    }
}