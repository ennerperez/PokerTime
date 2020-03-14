﻿// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinPokerSessionCommandHandler.cs
//  Project         : PokerTime.Application
// ******************************************************************************

namespace PokerTime.Application.Retrospectives.Commands.JoinPokerSession {
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
    using Notifications.RetrospectiveJoined;
    using Queries.GetParticipantsInfo;
    using PokerTime.Common;
    using Services;

    public sealed class JoinPokerSessionCommandHandler : IRequestHandler<JoinPokerSessionCommand, ParticipantInfo> {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public JoinPokerSessionCommandHandler(IPokerTimeDbContext pokerTimeDbContext, ICurrentParticipantService currentParticipantService, IMediator mediator, IMapper mapper) {
            this._pokerTimeDbContext = pokerTimeDbContext;
            this._currentParticipantService = currentParticipantService;
            this._mediator = mediator;
            this._mapper = mapper;
        }

        public async Task<ParticipantInfo> Handle(JoinPokerSessionCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Retrospective retrospective = await this._pokerTimeDbContext.Retrospectives.FindBySessionId(request.SessionId, cancellationToken);

            if (retrospective == null) {
                throw new NotFoundException(nameof(Retrospective), request.SessionId);
            }

            // Create domain object
            Participant participant = await this.GetOrCreateParticipantAsync(request.SessionId, request.Name, cancellationToken);

            participant.IsFacilitator = request.JoiningAsFacilitator;
            participant.Name = request.Name;
            participant.Session = retrospective;
            participant.Color = new ParticipantColor {
                R = Byte.Parse(request.Color[0..2], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                G = Byte.Parse(request.Color[2..4], NumberStyles.AllowHexSpecifier, Culture.Invariant),
                B = Byte.Parse(request.Color[4..6], NumberStyles.AllowHexSpecifier, Culture.Invariant),
            };

            // Save it
            bool isNew = !this._pokerTimeDbContext.Participants.Local.Contains(participant);
            if (isNew) this._pokerTimeDbContext.Participants.Add(participant);

            await this._pokerTimeDbContext.SaveChangesAsync(cancellationToken);

            // Update auth info
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(participant.Id, participant.Name, request.JoiningAsFacilitator));

            // Broadcast
            var participantInfo = this._mapper.Map<ParticipantInfo>(participant);

            if (isNew) {
                await this._mediator.Publish(new RetrospectiveJoinedNotification(request.SessionId, participantInfo), cancellationToken);
            }

            return participantInfo;
        }

        private async Task<Participant> GetOrCreateParticipantAsync(string sessionId, string name, CancellationToken cancellationToken) {
            Participant? existingParticipant = await this._pokerTimeDbContext.Participants.FirstOrDefaultAsync(x => x.Name == name && x.Session.UrlId.StringId == sessionId, cancellationToken);

            if (existingParticipant == null) {
                return new Participant();
            }

            return existingParticipant;
        }
    }
}
