﻿using CupCake.Command;
using CupCake.Command.Source;
using CupCake.Permissions;

namespace CupCake.DefaultCommands.Commands.Permissions
{
    public sealed class OpCommand : PermissionCommandBase
    {
        [MinArgs(1)]
        [MinGroup(Group.Admin)]
        [Label("op", "opplayer")]
        [CorrectUsage("player")]
        protected override void Run(IInvokeSource source, ParsedCommand message)
        {
            this.RunPermissionCommand(source, message, Group.Operator);
        }
    }
}