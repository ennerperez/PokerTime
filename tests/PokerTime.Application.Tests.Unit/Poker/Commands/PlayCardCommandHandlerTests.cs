namespace PokerTime.Application.Tests.Unit.Poker.Commands {
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Notifications.EstimationGiven;
    using Application.Poker.Commands;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using NSubstitute;
    using NSubstitute.ReceivedExtensions;
    using NUnit.Framework;
    using Support;

    [TestFixture]
    public sealed class PlayCardCommandHandlerTests : CommandTestBase {
        private Session _session;
        private int _participantId;

        [OneTimeSetUp]
        public async Task OneTimeSetUp() {
            var symbolSet = new SymbolSet {
                Name = "New"
            };

            var session = new Session {
                Title = "What",
                Participants =
                {
                    new Participant {Name = "John", Color = Color.BlueViolet},
                    new Participant {Name = "Jane", Color = Color.Aqua},
                },
                HashedPassphrase = "abef",
                FacilitatorHashedPassphrase = "xxx",
                SymbolSet = symbolSet
            };

            Context.Sessions.Add(session);

            Context.Symbols.Add(new Symbol {
                Type = SymbolType.Number,
                ValueAsNumber = 1,
                SymbolSet = symbolSet
            });

            Context.Symbols.Add(new Symbol {
                Type = SymbolType.Number,
                ValueAsNumber = 2,
                SymbolSet = symbolSet
            });

            Context.UserStories.Add(new UserStory {
                Session = session
            });

            var participant = new Participant {
                Name = "Henk",
            };

            Context.Participants.Add(participant);
            await Context.SaveChangesAsync(CancellationToken.None);

            _participantId = participant.Id;
            _session = session;
        }

        [Test]
        public async Task PlayCardCommandHandler_ThrowsException_WhenSessionNotFound() {
            // Given
            var command = new PlayCardCommand("not found",
                (await Context.UserStories.FirstAsync()).Id, (await Context.Symbols.FirstAsync()).Id);
            var handler = new PlayCardCommandHandler(Substitute.For<IMediator>(), Context, Substitute.For<ICurrentParticipantService>(), Substitute.For<IMapper>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task PlayCardCommandHandler_ThrowsException_WhenUserStoryNotFound() {
            // Given
            var command = new PlayCardCommand(_session.UrlId.StringId,
                543, (await Context.Symbols.FirstAsync()).Id);
            var handler = new PlayCardCommandHandler(Substitute.For<IMediator>(), Context, Substitute.For<ICurrentParticipantService>(), Substitute.For<IMapper>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task PlayCardCommandHandler_ThrowsException_WhenSymbolNotFound() {
            // Given
            var command = new PlayCardCommand(_session.UrlId.StringId, (await Context.UserStories.FirstAsync()).Id, -1);
            var handler = new PlayCardCommandHandler(Substitute.For<IMediator>(), Context, Substitute.For<ICurrentParticipantService>(), Substitute.For<IMapper>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task PlayCardCommandHandler_ThrowsException_ForEstimationInWrongSymbolSet() {
            // Given
            var mediator = Substitute.For<IMediator>();
            var currentParticipantService = Substitute.For<ICurrentParticipantService>();

            currentParticipantService.GetParticipant().
                Returns(new ValueTask<CurrentParticipantModel>(
                    new CurrentParticipantModel(_participantId, null, null, false)
                ));

            var symbol = Context.Symbols.Add(new Symbol {
                Type = SymbolType.Number,
                ValueAsNumber = 1,
                SymbolSet = new SymbolSet { Name = "wrong " }
            }).Entity;

            await Context.SaveChangesAsync();

            var command = new PlayCardCommand(
                _session.UrlId.StringId,
                (await Context.UserStories.FirstAsync()).Id,
                symbol.Id
            );

            var handler = new PlayCardCommandHandler(mediator,
                Context,
                currentParticipantService,
                Substitute.For<IMapper>());

            // When
            TestDelegate action = () => handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public async Task PlayCardCommandHandler_AddsEstimationAndBroadcast_ForNewEstimation() {
            // Given
            var mediator = Substitute.For<IMediator>();
            var currentParticipantService = Substitute.For<ICurrentParticipantService>();

            currentParticipantService.GetParticipant().
                Returns(new ValueTask<CurrentParticipantModel>(
                    new CurrentParticipantModel(_participantId, null, null, false)
                ));

            var command = new PlayCardCommand(
                _session.UrlId.StringId,
                (await Context.UserStories.FirstAsync()).Id,
                (await Context.Symbols.Where(x => x.SymbolSetId == _session.SymbolSetId).FirstAsync()).Id
            );
            var handler = new PlayCardCommandHandler(mediator,
                Context,
                currentParticipantService,
                Substitute.For<IMapper>());

            // When
            await handler.Handle(command, CancellationToken.None);

            // Then
            await currentParticipantService.ReceivedWithAnyArgs(Quantity.Exactly(1))
                .GetParticipant();

            var checkUserStory = await Context.UserStories.
                Include(x => x.Estimations).
                LastOrDefaultAsync();

            Assert.That(checkUserStory.Estimations.Select(x => x.Symbol.Id), Is.EquivalentTo(new[] { command.SymbolId }));

            await mediator.Received().
                Publish(Arg.Any<EstimationGivenNotification>(), Arg.Any<CancellationToken>());
        }
    }
}
