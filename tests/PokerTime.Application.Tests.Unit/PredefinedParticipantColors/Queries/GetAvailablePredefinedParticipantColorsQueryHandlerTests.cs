namespace PokerTime.Application.Tests.Unit.PredefinedParticipantColors.Queries {
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class GetAvailablePredefinedParticipantColorsQueryHandlerTests : QueryTestBase {
        [Test]
        public async Task GetAvailablePredefinedParticipantColorsTest() {
            // Given
            var pokerSession = new Session {
                CreationTimestamp = DateTimeOffset.UtcNow,
                FacilitatorHashedPassphrase = "xxx",
                Title = "xxx",
                Participants = { new Participant { Name = "John", Color = Color.Gold } }
            };
            Trace.Assert(pokerSession.UrlId.ToString() != null);
            Context.Sessions.Add(pokerSession);
            await Context.SaveChangesAsync(CancellationToken.None);

            // When
            var command = new GetAvailablePredefinedParticipantColorsQueryHandler(Context, Mapper);

            var result = await command.Handle(new GetAvailablePredefinedParticipantColorsQuery(pokerSession.UrlId.StringId), CancellationToken.None);

            // Then
            var colors = result.Select(x => Color.FromArgb(255, x.R, x.G, x.B).ToArgb()).ToList();

            Assert.That(colors, Does.Not.Contains(Color.Gold.ToArgb()));
            Assert.That(colors, Does.Contain(Color.Blue.ToArgb()));
        }
    }
}
