namespace PokerTime.Application.Tests.Unit.Estimations.Queries {
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Estimations.Queries;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetEstimationsQueryHandlerTests : QueryTestBase {
        [Test]
        public void GetEstimationsQueryHandlerTests_ThrowsNotFoundException_WhenSessionNotFound() {
            // Given
            const string SessionId = "surely-not-found";
            var query = new GetEstimationsQuery(SessionId, 3);
            var handler = new GetEstimationsQueryHandler(Context, Mapper);

            // When
            TestDelegate action = () => handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task GetEstimationsQueryHandlerTests_ThrowsNotFoundException_WhenUserStoryNotFound() {
            // Given
            var session = new Session {
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
            var sessionId = session.UrlId.StringId;
            Context.Sessions.Add(session);
            await Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetEstimationsQuery(sessionId, -1);
            var handler = new GetEstimationsQueryHandler(Context, Mapper);

            // When
            TestDelegate action = () => handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task GetEstimationsQueryHandlerTests_ReturnsUserStoryEstimations() {
            // Given
            var session = new Session {
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
            var sessionId = session.UrlId.StringId;
            Context.Sessions.Add(session);
            await Context.SaveChangesAsync(CancellationToken.None);

            Context.UserStories.Add(new UserStory {
                Title = "First",
                Estimations =
                {
                    new Estimation {Participant = session.Participants.First(), Symbol = Context.Symbols.First()},
                    new Estimation {Participant = session.Participants.Last(), Symbol = Context.Symbols.Skip(1).First()}
                },
                Session = session
            });

            var lastUserStory = Context.UserStories.Add(new UserStory {
                Title = "First",
                Estimations =
                {
                    new Estimation
                        {Participant = session.Participants.First(), Symbol = Context.Symbols.Skip(1).First()},
                    new Estimation {Participant = session.Participants.Last(), Symbol = Context.Symbols.First()}
                },
                Session = session
            }).Entity;
            await Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetEstimationsQuery(sessionId, lastUserStory.Id);
            var handler = new GetEstimationsQueryHandler(Context, Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);

            Assert.That(result.Estimations, Has.Count.EqualTo(2));
        }

    }
}
