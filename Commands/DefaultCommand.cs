using System;

namespace NivDrive.Commands
{
    internal class DefaultCommand : CommandBase
    {
        private Action<object?> _command;
        public DefaultCommand(Action<object?> command)
        {
            _command = command;
        }

        public override void Execute(object? parameter)
        {
            _command(parameter);
        }
    }
}
