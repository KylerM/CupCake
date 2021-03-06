﻿using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using CupCake.Chat;
using CupCake.Core;
using CupCake.Core.Events;
using CupCake.Core.Storage;
using CupCake.Host;
using CupCake.HostAPI.IO;
using CupCake.HostAPI.Status;
using CupCake.HostAPI.Title;
using CupCake.Protocol;
using CupCake.Server.SyntaxProviders;
using PlayerIOClient;
using Rabbit;
using Rabbit.Auth;

namespace CupCake.Server
{
    public class CupCakeClientHost
    {
        public const string GameId = "everybody-edits-su9rn58o40itdbnw69plyw";
        private CupCakeClient _client;
        private EventsPlatform _eventsPlatform;
        private SynchronizePlatform _synchronizePlatform;
        private IStorageProvider _storage;

        public event Action<string> Output;

        protected virtual void OnOutput(string e)
        {
            Action<string> handler = this.Output;
            if (handler != null) handler(e);
        }

        public event Action<string> Title;

        protected virtual void OnTitle(string title)
        {
            Action<string> handler = this.Title;
            if (handler != null) handler(title);
        }

        public event Action<string> Status;

        protected virtual void OnStatus(string status)
        {
            Action<string> handler = this.Status;
            if (handler != null) handler(status);
        }

        public void Input(string input)
        {
            this._synchronizePlatform.DoSynchronously(() =>
            {
                this._eventsPlatform.Event<InputEvent>().Raise(this, new InputEvent(input));
            });
        }

        private void PlatformLoader_EnableComplete(object sender, EventArgs e)
        {
            if (this._storage != null)
            {
                // Change the default storage source
                var storagePlatform = this._client.PlatformLoader.Get<StoragePlatform>();
                storagePlatform.StorageProvider = this._storage;
            }

            // Listen to HostAPI events
            this._synchronizePlatform = this._client.PlatformLoader.Get<SynchronizePlatform>();
            this._eventsPlatform = this._client.PlatformLoader.Get<EventsPlatform>();
            this._eventsPlatform.Event<OutputEvent>().Bind(this.OnOutput, EventPriority.Lowest);
            this._eventsPlatform.Event<ChangeTitleEvent>().Bind(this.OnChangeTitle, EventPriority.Lowest);
            this._eventsPlatform.Event<ChangeStatusEvent>().Bind(this.OnChangeStatus, EventPriority.Lowest);
        }

        private void ServiceLoader_EnableComplete(object sender, EventArgs e)
        {
            // Change the default chat, io and status formats
            this._client.ServiceLoader.Get<ChatService>().SyntaxProvider = new CupCakeChatSyntaxProvider();
            this._client.ServiceLoader.Get<IOService>().SyntaxProvider = new CupCakeIOSyntaxProvider();
            this._client.ServiceLoader.Get<StatusService>().SyntaxProvider = new CupCakeStatusSyntaxProvider();
        }

        private void OnOutput(object sender, OutputEvent e)
        {
            this.OnOutput(e.Message);
        }

        private void OnChangeTitle(object sender, ChangeTitleEvent e)
        {
            this.OnTitle(e.NewTitle);
        }

        private void OnChangeStatus(object sender, ChangeStatusEvent e)
        {
            this.OnStatus(e.NewStatus);
        }

        private void connection_OnDisconnect(object sender, string message)
        {
            this.Disconnected();
        }

        private void Disconnected()
        {
            this.LogMessage("Disconnected from Everybody Edits");
            this.Dispose();
            Environment.Exit(1);
        }

        private void LogMessage(string str)
        {
            this.OnOutput(String.Format("*** {0}", str));
        }

        public void Start(string email, string password, string roomId, string[] directories,
            IStorageProvider storage)
        {
            this._storage = storage;

            this.LogMessage(String.Format("Welcome to CupCake! (API version: {0})", this.GetVersion()));

            // Create the client
            this._client = new CupCakeClient(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            this._client.PlatformLoader.EnableComplete += this.PlatformLoader_EnableComplete;
            this._client.ServiceLoader.EnableComplete += this.ServiceLoader_EnableComplete;

            // Load plugins
            this.LogMessage("Loading plugin dlls...");
            foreach (string dir in directories)
            {
                if (Directory.Exists(dir))
                    this._client.AggregateCatalog.Catalogs.Add(new DirectoryCatalog(dir));
                else
                    this.LogMessage("Invalid folder: " + dir);
            }

            // Get room version
            this.LogMessage("Joining room...");
            // Connect to playerIO and join room
            Connection connection = new RabbitAuth().LogIn(email, roomId, password);

            // Start
            this.LogMessage("Starting plugins...");
            this._client.Start(connection);

            // Handle disconnect, if we are already too late, disconnect
            connection.OnDisconnect += this.connection_OnDisconnect;
            if (!connection.Connected)
                this.Disconnected();

            this.LogMessage("Done.");
        }

        private string GetVersion()
        {
            var attribute =
                (AssemblyInformationalVersionAttribute)Assembly.GetAssembly(typeof(CupCakeClient))
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault();

            if (attribute != null)
                return attribute.InformationalVersion;
            return "Unknown!";
        }

        public void Dispose()
        {
            if (this._client != null)
                this._client.Dispose();
        }
    }
}