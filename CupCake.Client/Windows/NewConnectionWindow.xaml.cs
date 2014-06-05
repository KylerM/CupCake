﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CupCake.Client.Settings;
using CupCake.Protocol;

namespace CupCake.Client.Windows
{
    /// <summary>
    ///     Interaction logic for NewConnectionWindow.xaml
    /// </summary>
    public partial class NewConnectionWindow
    {
        private readonly ClientHandle _handle;
        private readonly bool _isDebug;
        private readonly RecentWorld _recentWorld;

        public NewConnectionWindow(ClientHandle handle, RecentWorld recentWorld, bool isDebug)
        {
            this._recentWorld = recentWorld;
            this._isDebug = isDebug;
            this.InitializeComponent();

            this._handle = handle;
            this._handle.ReceiveClose += this._handle_ReceiveClose;

            this.Closed += this.NewConnectionWindow_Closed;

            foreach (Profile profile in SettingsManager.Settings.Profiles.OrderBy(v => v.Id))
            {
                var item = new TextBlock(new Run(profile.Name.GetVisualName())) {Tag = profile};
                this.ProfileComboBox.Items.Add(item);

                if (recentWorld.Profile == profile.Id)
                    this.ProfileComboBox.SelectedItem = item;
            }

            foreach (Account account in SettingsManager.Settings.Accounts.OrderBy(v => v.Id))
            {
                var item = new TextBlock(new Run(account.Email.GetVisualName())) {Tag = account};
                this.AccountComboBox.Items.Add(item);

                if (recentWorld.Profile == account.Id)
                    this.AccountComboBox.SelectedItem = item;
            }

            this.WorldIdTextBox.Text = recentWorld.WorldId;
        }

        private void NewConnectionWindow_Closed(object sender, EventArgs e)
        {
            this._handle.ReceiveClose -= this._handle_ReceiveClose;
        }

        private void _handle_ReceiveClose()
        {
            Dispatch.Invoke(() =>
            {
                if (this.DialogResult == null)
                    this.DialogResult = false;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int pId = default(int);
            string pFolder = String.Empty;
            DatabaseType dbType = default(DatabaseType);
            string dbCs = String.Empty;

            if (this.ProfileComboBox.SelectedItem != null)
            {
                var profile = (Profile)((TextBlock)this.ProfileComboBox.SelectedItem).Tag;
                pId = profile.Id;
                pFolder = profile.Folder;

                Database database = SettingsManager.Settings.Databases.FirstOrDefault(db => db.Id == profile.Database);

                if (database != null)
                {
                    dbType = database.Type;
                    dbCs = database.ConnectionString;
                }
            }

            int aId = default(int);
            AccountType aType = default(AccountType);
            string aEmail = String.Empty;
            string aPass = String.Empty;

            if (this.AccountComboBox.SelectedItem != null)
            {
                var account = (Account)((TextBlock)this.AccountComboBox.SelectedItem).Tag;
                aId = account.Id;
                aType = account.Type;
                aEmail = account.Email;
                aPass = account.Password.DecryptString().ToInsecureString();
            }

            string worldId = this.WorldIdTextBox.Text;

            var folders = new List<string>
            {
                SettingsManager.PluginsPath,
                pFolder
            };

            if (this._isDebug)
                folders.Add(SettingsManager.DebugPath);

            this._handle.DoSendSetData(aType, aEmail, aPass, worldId, folders.ToArray(), dbType, dbCs);

            this._recentWorld.Account = aId;
            this._recentWorld.Profile = pId;
            this._recentWorld.WorldId = worldId;

            this.DialogResult = true;
        }
    }
}