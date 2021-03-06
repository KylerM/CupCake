﻿using CupCake.Command;
using CupCake.Command.Source;
using CupCake.Permissions;

namespace CupCake.DefaultCommands.Commands.Owner
{
    public sealed class RespawnAllCommand : OwnerCommandBase
    {
        [MinGroup(Group.Moderator)]
        [Label("respawnall")]
        [CorrectUsage("")]
        protected override void Run(IInvokeSource source, ParsedCommand message)
        {
            this.RequireOwner();
            this.Chatter.RespawnAll();
            source.Reply("Respawned all.");
        }
    }
}