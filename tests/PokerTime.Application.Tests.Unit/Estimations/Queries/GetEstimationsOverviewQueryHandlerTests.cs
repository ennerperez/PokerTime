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
    public sealed class GetEstimationsOverviewQueryHandlerTests : QueryTestBase {
        [Test]
        public void GetEstimationsOverviewQueryHandlerTests_ThrowsNotFoundException_WhenNotFound() {
            // Given
            const string sessionId = "surely-not-found";
            var query = new GetEstimationsOverviewQuery(sessionId);
            var handler = new GetEstimationsOverviewQueryHandler(Context, Mapper);

            // When
            TestDelegate action = () => handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task GetEstimationsOverviewQueryHandlerTests_ReturnsUserStoryEstimations() {
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

            Context.UserStories.Add(new UserStory {
                Title = "First",
                Estimations =
                {
                    new Estimation
                        {Participant = session.Participants.First(), Symbol = Context.Symbols.Skip(1).First()},
                    new Estimation {Participant = session.Participants.Last(), Symbol = Context.Symbols.First()}
                },
                Session = session
            });
            await Context.SaveChangesAsync(CancellationToken.None);

            var query = new GetEstimationsOverviewQuery(sessionId);
            var handler = new GetEstimationsOverviewQueryHandler(Context, Mapper);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);

            Assert.That(result.UserStoryEstimations, Has.Count.EqualTo(2));
        }

    }
}
