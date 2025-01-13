namespace PokerTime.Application.Sessions.Commands.RejoinPokerSession {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using Common.Models;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class RejoinPokerSessionCommandHandler : IRequestHandler<RejoinPokerSessionCommand> {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;
        private readonly ICurrentParticipantService _currentParticipantService;

        public RejoinPokerSessionCommandHandler(IPokerTimeDbContext pokerTimeDbContext, ICurrentParticipantService currentParticipantService) {
            _pokerTimeDbContext = pokerTimeDbContext;
            _currentParticipantService = currentParticipantService;
        }

        public async Task Handle(RejoinPokerSessionCommand request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _pokerTimeDbContext.Participants.AsNoTracking().
                Where(x => x.Session.UrlId.StringId == request.SessionId && x.Id == request.ParticipantId).
                FirstOrDefaultAsync(cancellationToken);

            if (result == null) {
                throw new NotFoundException(nameof(Participant), request.ParticipantId);
            }

            _currentParticipantService.SetParticipant(
                new CurrentParticipantModel(result.Id, result.Name, result.Color.ToHex(), result.IsFacilitator));

        }
    }
}
