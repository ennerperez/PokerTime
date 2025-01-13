namespace PokerTime.Application.Symbols.Queries {
    using MediatR;

    public sealed class GetSymbolsQuery : IRequest<GetSymbolsQueryResponse> {
        public int SymbolSetId { get; }

        public GetSymbolsQuery(int symbolSetId) {
            SymbolSetId = symbolSetId;
        }
    }
}
