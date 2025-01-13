namespace PokerTime.Application.Poker.Commands {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.EstimationGiven;
    using Services;

    public sealed class PlayCardCommandHandler : IRequestHandler<PlayCardCommand> {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IPokerTimeDbContextFactory _dbContextFactory;
        private readonly ICurrentParticipantService _currentParticipantService;

        public PlayCardCommandHandler(IMediator mediator, IPokerTimeDbContextFactory dbContextFactory, ICurrentParticipantService currentParticipantService, IMapper mapper) {
            _mediator = mediator;
            _dbContextFactory = dbContextFactory;
            _currentParticipantService = currentParticipantService;
            _mapper = mapper;
        }

        public async Task Handle(PlayCardCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using var dbContext = _dbContextFactory.CreateForEditContext();

            var session = await dbContext.Sessions.FindBySessionId(request.SessionId, cancellationToken);
            if (session == null) {
                throw new NotFoundException(nameof(Session), request.SessionId);
            }

            var userStory = await dbContext.UserStories.FirstOrDefaultAsync(x => x != null && x.Session.UrlId.StringId == request.SessionId && x.Id == request.UserStoryId, cancellationToken);
            if (userStory == null) {
                throw new NotFoundException(nameof(UserStory), request.UserStoryId);
            }

            var desiredSymbol = await dbContext.Symbols.FirstOrDefaultAsync(x => x != null && x.Id == request.SymbolId, cancellationToken);
            if (desiredSymbol == null) {
                throw new NotFoundException(nameof(Symbol), request.SymbolId);
            }

            if (desiredSymbol.SymbolSetId != session.SymbolSetId) {
                throw new InvalidOperationException($"The chosen symbol #{request.SymbolId} is not part of symbol set #{session.SymbolSetId}");
            }

            var currentParticipantInfo = await _currentParticipantService.GetParticipant();

            // Add or update estimation
            var estimation = await dbContext.Estimations
                .Include(x => x!.Participant)
                .Where(x => x != null && x.UserStory != null && x.UserStory.Session.UrlId.StringId == session.UrlId.StringId)
                .FirstOrDefaultAsync(x => x != null && x.UserStory != null && x.UserStory.Id == userStory.Id && x.ParticipantId == currentParticipantInfo.Id, cancellationToken);

            if (estimation == null) {
                estimation = new Estimation {
                    ParticipantId = currentParticipantInfo.Id,
                    Participant = dbContext.Participants.First(x => x.Id == currentParticipantInfo.Id),
                    UserStory = userStory,
                    Symbol = desiredSymbol
                };

                dbContext.Estimations.Add(estimation);
            }
            else {
                estimation.Symbol = desiredSymbol;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var estimationNotification = new EstimationGivenNotification(
                session.UrlId.StringId,
                _mapper.Map<EstimationModel>(estimation)
            );

            await _mediator.Publish(estimationNotification, cancellationToken);
        }
    }
}
