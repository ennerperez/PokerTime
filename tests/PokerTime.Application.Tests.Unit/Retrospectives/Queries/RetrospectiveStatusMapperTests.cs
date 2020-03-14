﻿// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SessionStatusMapperTests.cs
//  Project         : PokerTime.Application.Tests.Unit
// ******************************************************************************

namespace PokerTime.Application.Tests.Unit.Retrospectives.Queries {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Retrospectives.Queries.GetSessionStatus;
    using Domain.Entities;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class SessionStatusMapperTests : QueryTestBase {
        [Test]
        public void SessionStatusMapper_NullArgument_ThrowsArgumentNullException() {
            // Given
            var mapper = new SessionStatusMapper(this.Context, this.Mapper);

            // When
            TestDelegate action = () => mapper.GetSessionStatus(null, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.ArgumentNullException);
        }

        [Test]
        public async Task SessionStatusMapper_ReturnsRetrospectiveInfo() {
            // Given
            var retro = new Retrospective {
                Title = "Yet another test",
                Participants =
                {
                    new Participant { Name = "John", Color = Color.BlueViolet },
                    new Participant { Name = "Jane", Color = Color.Aqua },
                },
                HashedPassphrase = "abef",
                CurrentStage = SessionStage.Writing
            };
            string sessionId = retro.UrlId.StringId;
            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None);

            var mapper = new SessionStatusMapper(this.Context, this.Mapper);

            // When
            var result = await mapper.GetSessionStatus(retro, CancellationToken.None);

            // Then
            Assert.That(result.SessionId, Is.EqualTo(sessionId));
            Assert.That(result.Title, Is.EqualTo(retro.Title));
        }
    }
}
