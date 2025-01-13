namespace PokerTime.Application.Symbols.Queries {
    using System.Diagnostics.CodeAnalysis;
    using FluentValidation;

    [SuppressMessage(category: "Naming",
        checkId: "CA1710:Identifiers should have correct suffix",
        Justification = "This is a validation rule set.")]
    public sealed class GetSymbolsQueryValidator : AbstractValidator<GetSymbolsQuery> {
        public GetSymbolsQueryValidator() {
            RuleFor(x => x.SymbolSetId).NotEmpty();
        }
    }
}
