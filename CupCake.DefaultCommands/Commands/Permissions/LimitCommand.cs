﻿using CupCake.Command;
using CupCake.Command.Source;
using CupCake.Permissions;

namespace CupCake.DefaultCommands.Commands.Permissions
{
    public sealed class LimitCommand : PermissionCommandBase
    {
        [MinArgs(1)]
        [MinGroup(Group.Operator)]
        [Label("limit", "limitplayer")]
        [CorrectUsage("player")]
        protected override void Run(IInvokeSource source, ParsedCommand message)
        {
            this.RunPermissionCommand(source, message, Group.Limited);
        }
    }
}