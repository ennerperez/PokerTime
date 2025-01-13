namespace PokerTime.Application.SymbolSets.Queries {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common.Abstractions;
    using Common.Models;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class GetSymbolSetsQueryHandler : IRequestHandler<GetSymbolSetsQuery, GetSymbolSetsQueryResponse> {
        private readonly IPokerTimeDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;

        public GetSymbolSetsQueryHandler(IPokerTimeDbContextFactory dbContextFactory, IMapper mapper) {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
        }

        public async Task<GetSymbolSetsQueryResponse> Handle(GetSymbolSetsQuery request, CancellationToken cancellationToken) {
            using var dbContext = _dbContextFactory.CreateForEditContext();

            var symbolSets = _mapper.Map<SymbolSetModel[]>(
                await dbContext.SymbolSets.Include(x => x.Symbols).OrderBy(x => x.Id).ToListAsync(cancellationToken));

            return new GetSymbolSetsQueryResponse(symbolSets);
        }
    }
}
