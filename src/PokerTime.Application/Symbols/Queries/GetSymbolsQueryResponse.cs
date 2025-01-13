namespace PokerTime.Application.Symbols.Queries {
    using System.Collections.Generic;
    using Common.Models;

    public sealed class GetSymbolsQueryResponse {
        public ICollection<SymbolModel> Symbols { get; }

        internal GetSymbolsQueryResponse(ICollection<SymbolModel> symbols) {
            Symbols = symbols;
        }
    }
}
