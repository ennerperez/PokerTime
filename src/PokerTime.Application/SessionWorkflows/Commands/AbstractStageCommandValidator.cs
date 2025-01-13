namespace PokerTime.Application.SessionWorkflows.Commands {
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public abstract class AbstractStageCommandValidator<TCommand> : AbstractValidator<TCommand> where TCommand : AbstractStageCommand {
        protected AbstractStageCommandValidator() {
            RuleFor(x => x.SessionId).NotEmpty();
        }
    }
}
