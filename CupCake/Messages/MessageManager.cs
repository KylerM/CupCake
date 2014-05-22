﻿using System.Collections.Generic;
using CupCake.Core.Events;
using CupCake.EE.Events.Receive;

namespace CupCake.Messages
{
    public class MessageManager
    {
        private readonly EventManager _eventsPlatform;

        private readonly Dictionary<string, IRegisteredMessage> _messageDictionary =
            new Dictionary<string, IRegisteredMessage>();

        public MessageManager(EventManager eventsPlatform)
        {
            this._eventsPlatform = eventsPlatform;
        }

        public IRegisteredMessage this[string str]
        {
            get
            {
                lock (this._messageDictionary)
                {
                    return this._messageDictionary[str];
                }
            }
        }

        public void RegisterMessage<T>(string str) where T : ReceiveEvent
        {
            lock (this._messageDictionary)
            {
                this._messageDictionary.Add(str, new RegisteredMessage<T>(this._eventsPlatform));
            }
        }

        public void UnRegisterMessage(string str)
        {
            lock (this._messageDictionary)
            {
                this._messageDictionary.Remove(str);
            }
        }

        public bool Contains(string str)
        {
            lock (this._messageDictionary)
            {
                return this._messageDictionary.ContainsKey(str);
            }
        }

        public void UnRegisterAll()
        {
            lock (this._messageDictionary)
            {
                this._messageDictionary.Clear();
            }
        }
    }
}