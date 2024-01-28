namespace PokerTime.Application.Sessions.Commands.JoinPokerSession {
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using Domain.ValueObjects;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Notifications.SessionJoined;
    using Queries.GetParticipantsInfo;
    using PokerTime.Common;
    using Services;

    public sealed class JoinPokerSessionCommandHandler : IRequestHandler<JoinPokerSessionCommand, ParticipantInfo> {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public JoinPokerSessionCommandHandler(IPokerTimeDbContext pokerTimeDbContext, ICurrentParticipantService currentParticipantService, IMediator mediator, IMapper mapper) {
            _pokerTimeDbContext = pokerTimeDbContext;
            _currentParticipantService = currentParticipantService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ParticipantInfo> Handle(JoinPokerSessionCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var Session = await _pokerTimeDbContext.Sessions.FindBySessionId(request.SessionId, cancellationToken);

            if (Session == null) {
                throw new NotFoundException(nameof(Session), request.SessionId);
            }

            // Create domain object
            var participant = await GetOrCreateParticipantAsync(request.SessionId, request.Name, cancellationToken);

            participant.IsFacilitator = request.JoiningAsFacilitator;
            participant.Name = request.Name;
            participant.Session = Session;
            participant.Color = new ParticipantColor {
                R = byte.Parse(request.Color[0..2], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                G = byte.Parse(request.Color[2..4], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                B = byte.Parse(request.Color[4..6], NumberStyles.AllowHexSpecifier, Culture.Invariant),
            };

            // Save it
            var isNew = !_pokerTimeDbContext.Participants.Local.Contains(participant);
            if (isNew) _pokerTimeDbContext.Participants.Add(participant);

            await _pokerTimeDbContext.SaveChangesAsync(cancellationToken);

            // Update auth info
            _currentParticipantService.SetParticipant(new CurrentParticipantModel(participant.Id, participant.Name, participant.Color.ToHex(), request.JoiningAsFacilitator));

            // Broadcast
            var participantInfo = _mapper.Map<ParticipantInfo>(participant);

            if (isNew) {
                await _mediator.Publish(new SessionJoinedNotification(request.SessionId, participantInfo), cancellationToken);
            }

            return participantInfo;
        }

        private async Task<Participant> GetOrCreateParticipantAsync(string sessionId, string name, CancellationToken cancellationToken) {
            var existingParticipant = await _pokerTimeDbContext.Participants.FirstOrDefaultAsync(x => x.Name == name && x.Session.UrlId.StringId == sessionId, cancellationToken);

            if (existingParticipant == null) {
                return new Participant();
            }

            return existingParticipant;
        }
    }
}
