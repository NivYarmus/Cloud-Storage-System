using NivDrive.Network;
using NivDrive.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NivDrive.Commands.UserCommands
{
    internal abstract class UserCommandBase : CommandBase
    {
        protected ClientSocket _clientSocket;
        protected SecurityManager _security;

        protected UserCommandBase(ClientSocket clientSocket, SecurityManager security)
        {
            _clientSocket = clientSocket;
            _security = security;
        }
    }
}
