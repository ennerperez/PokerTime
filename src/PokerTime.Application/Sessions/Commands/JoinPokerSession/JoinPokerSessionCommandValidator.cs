namespace PokerTime.Application.Sessions.Commands.JoinPokerSession {
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using FluentValidation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is a validation rule set.")]
    public sealed class JoinPokerSessionCommandValidator : AbstractValidator<JoinPokerSessionCommand> {
        private static readonly Expression<Func<Session, string>> GetFacilitatorHash = r => r.FacilitatorHashedPassphrase;
        private static readonly Expression<Func<Session, string>> GetParticipantHash = r => r.HashedPassphrase;

        private readonly IPokerTimeDbContextFactory _pokerTimeDbContext;
        private readonly IPassphraseService _passphraseService;

        public JoinPokerSessionCommandValidator(IPokerTimeDbContextFactory pokerTimeDbContext, IPassphraseService passphraseService) {
            _pokerTimeDbContext = pokerTimeDbContext;
            _passphraseService = passphraseService;

            RuleFor(e => e.Name).NotEmpty().MaximumLength(256);
            RuleFor(e => e.Color).NotEmpty()
                .WithMessage("Please select a color")
                .Matches("^#?([A-F0-9]{2}){3}$", RegexOptions.IgnoreCase)
                .WithMessage("Please select a color");

            RuleFor(e => e.Passphrase)
                .NotEmpty()
                .When(x => x.JoiningAsFacilitator);

            // Passphrase validation
            RuleFor(e => e.Passphrase).
                Must((obj, passphrase) => MustBeAValidPassphrase(obj.SessionId, obj.JoiningAsFacilitator, obj.Passphrase))
                .WithMessage("This passphrase is not valid. Please try again.");
        }

        private bool MustBeAValidPassphrase(string sessionId, in bool isFacilitatorRole, string passphrase) {
            using var dbContext = _pokerTimeDbContext.CreateForEditContext();

            var property = isFacilitatorRole ? GetFacilitatorHash : GetParticipantHash;
            var hash = dbContext.Sessions.Where(x => x.UrlId.StringId == sessionId).Select(property).FirstOrDefault();

            if (hash == null) {
                return true;
            }

            if (string.IsNullOrEmpty(passphrase)) {
                return false;
            }

            return _passphraseService.ValidatePassphrase(passphrase, hash);
        }
    }
}
