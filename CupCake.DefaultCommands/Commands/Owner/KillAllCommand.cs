﻿using CupCake.Command;
using CupCake.Command.Source;
using CupCake.Permissions;

namespace CupCake.DefaultCommands.Commands.Owner
{
    public sealed class KillAllCommand : OwnerCommandBase
    {
        [MinGroup(Group.Moderator)]
        [Label("killall", "killemall")]
        [CorrectUsage("")]
        protected override void Run(IInvokeSource source, ParsedCommand message)
        {
            this.RequireOwner();
            this.Chatter.KillAll();
            source.Reply("Killed everyone.");
        }
    }
}