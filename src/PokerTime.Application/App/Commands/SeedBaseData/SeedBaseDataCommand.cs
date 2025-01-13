namespace PokerTime.Application.App.Commands.SeedBaseData {
    using Common.Abstractions;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public sealed class SeedBaseDataCommand : IRequest;

    public sealed class SeedBaseDataCommandHandler : IRequestHandler<SeedBaseDataCommand> {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;

        public SeedBaseDataCommandHandler(IPokerTimeDbContext pokerTimeDbContext) {
            _pokerTimeDbContext = pokerTimeDbContext;
        }

        public async Task Handle(SeedBaseDataCommand request, CancellationToken cancellationToken) {
            var seeder = new BaseDataSeeder(_pokerTimeDbContext);

            await seeder.SeedAllAsync(cancellationToken);
        }
    }
}
