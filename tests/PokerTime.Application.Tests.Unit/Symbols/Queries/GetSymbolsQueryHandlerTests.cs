namespace PokerTime.Application.Tests.Unit.Symbols.Queries {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Symbols.Queries;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetSymbolsQueryHandlerTests : QueryTestBase {
        [Test]
        public async Task GetSymbolsQueryHandlerTest_ReturnsSymbols() {
            // Given
            var symbolSetId = Context.SymbolSets.First().Id;
            var query = new GetSymbolsQuery(symbolSetId);
            var handler = new GetSymbolsQueryHandler(Context, Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result.Symbols.Select(x => x.Id), Is.EquivalentTo(Context.Symbols.Where(x => x.SymbolSetId == symbolSetId).Select(x => x.Id)));
        }
    }
}
