namespace PokerTime.Application.Tests.Unit.Sessions.Queries {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Sessions.Queries.GetParticipant;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetParticipantQueryHandlerTests : QueryTestBase {
        private string _retro1Id;
        private string _retro2Id;

        [OneTimeSetUp]
        public async Task OneTimeSetUp() {
            var retro = new Session {
                Title = "What",
                Participants =
                {
                    new Participant {Name = "John", Color = Color.BlueViolet},
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef",
                FacilitatorHashedPassphrase = "xxx"
            };
            _retro1Id = retro.UrlId.StringId;
            Context.Sessions.Add(retro);

            var retro2 = new Session {
                Title = "Who",
                Participants =
                {
                    new Participant {Name = "Foo", Color = Color.BlueViolet},
                    new Participant {Name = "Baz", Color = Color.Aqua},
                },
                HashedPassphrase = "abef",
                FacilitatorHashedPassphrase = "xxx"
            };
            _retro2Id = retro2.UrlId.StringId;
            Context.Sessions.Add(retro2);
            await Context.SaveChangesAsync(CancellationToken.None);
        }

        [Test]
        public async Task GetParticipantQueryHandler_ReturnsParticipantInfo_IfFound() {
            // Given
            var handler = new GetParticipantQueryHandler(Context, Mapper);
            var query = new GetParticipantQuery("John", _retro1Id);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("John"));
        }

        [Test]
        public async Task GetParticipantQueryHandler_ReturnsNull_IfNotFound1() {
            // Given
            var handler = new GetParticipantQueryHandler(Context, Mapper);
            var query = new GetParticipantQuery("Jane", _retro2Id);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetParticipantQueryHandler_ReturnsNull_IfNotFound2() {
            // Given
            var handler = new GetParticipantQueryHandler(Context, Mapper);
            var query = new GetParticipantQuery("Baz", _retro1Id);

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            Assert.That(result, Is.Null);
        }
    }
}
