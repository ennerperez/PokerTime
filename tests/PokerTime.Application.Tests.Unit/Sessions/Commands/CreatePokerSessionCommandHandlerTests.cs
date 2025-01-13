namespace PokerTime.Application.Tests.Unit.Sessions.Commands {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Sessions.Commands.CreatePokerSession;
    using Domain.Entities;
    using Domain.Services;
    using Domain.ValueObjects;
    using Microsoft.Extensions.Logging.Abstractions;
    using NSubstitute;
    using NUnit.Framework;
    using PokerTime.Common;
    using Services;
    using Support;

    [TestFixture]
    public sealed class CreatePokerSessionCommandHandlerTests : CommandTestBase {
        [Test]
        public async Task Handle_GivenValidRequest_ShouldSaveSessionWithHash() {
            // Given
            var passphraseService = Substitute.For<IPassphraseService>();
            var systemClock = Substitute.For<ISystemClock>();
            var urlGenerator = Substitute.For<IUrlGenerator>();
            var handler = new CreatePokerSessionCommandHandler(Context, passphraseService, systemClock, urlGenerator, new NullLogger<CreatePokerSessionCommandHandler>());

            passphraseService.CreateHashedPassphrase("anything").Returns("myhash");
            passphraseService.CreateHashedPassphrase("facilitator password").Returns("facilitatorhash");

            urlGenerator.GenerateUrlToPokerSessionLobby(Arg.Any<SessionIdentifier>()).Returns(new Uri("https://example.com/session/1"));

            systemClock.CurrentTimeOffset.Returns(DateTimeOffset.UnixEpoch);

            var symbolSet = new SymbolSet
            {
                Name = "Test123"
            };
            Context.SymbolSets.Add(symbolSet);
            await Context.SaveChangesAsync(CancellationToken.None);

            var request = new CreatePokerSessionCommand {
                Passphrase = "anything",
                FacilitatorPassphrase = "facilitator password",
                Title = "Hello",
                SymbolSetId = symbolSet.Id
            };

            // When
            var result = await handler.Handle(request, CancellationToken.None);

            // Then
            Assert.That(result.Identifier.StringId, Is.Not.Null);
            Assert.That(Context.Sessions.Any(), Is.True);
            Assert.That(Context.Sessions.First().FacilitatorHashedPassphrase, Is.EqualTo("facilitatorhash"));
            Assert.That(Context.Sessions.First().CreationTimestamp, Is.EqualTo(DateTimeOffset.UnixEpoch));
        }
    }
}
