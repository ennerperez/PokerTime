namespace PokerTime.Application.Tests.Unit.Sessions.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Sessions.Queries.GetJoinPokerSessionInfo;
    using Domain.Entities;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetJoinPokerSessionInfoQueryHandlerTests : QueryTestBase {
        [Test]
        public async Task GetJoinPokerSessionInfoCommandHandler_ReturnsNull_OnSessionNotFound() {
            // Given
            var sessionId = "whatever-whatever";
            var handler = new GetJoinPokerSessionInfoQueryHandler(Context, new NullLogger<GetJoinPokerSessionInfoQueryHandler>());
            var command = new GetJoinPokerSessionInfoQuery { SessionId = sessionId };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetJoinPokerSessionInfoCommandHandler_ReturnsInfo_OnSessionFound() {
            // Given
            var session = new Session {
                Title = "Hello",
                CreationTimestamp = DateTimeOffset.Now,
                HashedPassphrase = "hello",
                FacilitatorHashedPassphrase = "xxx"
            };
            var sessionId = session.UrlId.StringId;
            Context.Sessions.Add(session);
            await Context.SaveChangesAsync(CancellationToken.None);

            var handler = new GetJoinPokerSessionInfoQueryHandler(Context, new NullLogger<GetJoinPokerSessionInfoQueryHandler>());
            var command = new GetJoinPokerSessionInfoQuery { SessionId = sessionId };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Hello"));
            Assert.That(result.IsStarted, Is.False);
            Assert.That(result.IsFinished, Is.False);
            Assert.That(result.NeedsParticipantPassphrase, Is.True);
        }
    }
}
