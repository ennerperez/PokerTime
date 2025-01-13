namespace PokerTime.Application.Common.Models {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Domain.Entities;
    using Mapping;

    public sealed class SymbolSetModel : IMapFrom<SymbolSet> {
        public SymbolSetModel(int id, string name, IEnumerable<SymbolModel> symbols) {
            Id = id;
            Name = name;
            Symbols = new ReadOnlyCollection<SymbolModel>(symbols.OrderBy(keySelector: x => x.Order).ToList());
        }

        public SymbolSetModel() {
            Name = string.Empty;
            Symbols = new ReadOnlyCollection<SymbolModel>(new List<SymbolModel>(0));
        }

        public int Id { get; }

        public string Name { get; }

        public ReadOnlyCollection<SymbolModel> Symbols { get; }
    }
}
