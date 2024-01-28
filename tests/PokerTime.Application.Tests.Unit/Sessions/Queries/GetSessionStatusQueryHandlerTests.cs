namespace PokerTime.Application.Tests.Unit.Sessions.Queries {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Sessions.Queries.GetSessionStatus;
    using Domain.Entities;
    using NSubstitute;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetSessionStatusQueryHandlerTests : QueryTestBase {
        [Test]
        public void GetSessionStatusCommand_ThrowsNotFoundException_WhenNotFound() {
            // Given
            const string sessionId = "surely-not-found";
            var query = new GetSessionStatusQuery(sessionId);
            var handler = new GetSessionStatusQueryHandler(Context, Substitute.For<ISessionStatusMapper>());

            // When
            TestDelegate action = () => handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task GetSessionStatusCommand_ReturnsSessionInfo() {
            // Given
            var retro = new Session {
                Title = "Yet another test",
                Participants =
                {
                    new Participant { Name = "John", Color = Color.BlueViolet },
                    new Participant { Name = "Jane", Color = Color.Aqua },
                },
                HashedPassphrase = "abef",
                FacilitatorHashedPassphrase = "xxx",
                CurrentStage = SessionStage.Discussion
            };
            var sessionId = retro.UrlId.StringId;
            Context.Sessions.Add(retro);
            await Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetSessionStatusQuery(sessionId);
            var handler = new GetSessionStatusQueryHandler(Context, new SessionStatusMapper(Context, Mapper));

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result.SessionId, Is.EqualTo(sessionId));
            Assert.That(result.Title, Is.EqualTo(retro.Title));
        }
    }
}
