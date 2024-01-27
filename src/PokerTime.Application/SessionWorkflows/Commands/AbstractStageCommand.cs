// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
//
//  File:           : AbstractStageCommand.cs
//  Project         : PokerTime.Application
// ******************************************************************************

namespace PokerTime.Application.SessionWorkflows.Commands
{


    public abstract class AbstractStageCommand
    {
        public string SessionId { get; set; }
    }
}
