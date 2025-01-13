namespace PokerTime.Application.Sessions.Queries.GetParticipant {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Common.Abstractions;
    using GetParticipantsInfo;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class GetParticipantQueryHandler : IRequestHandler<GetParticipantQuery, ParticipantInfo> {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;
        private readonly IMapper _mapper;

        public GetParticipantQueryHandler(IPokerTimeDbContext pokerTimeDbContext, IMapper mapper) {
            _pokerTimeDbContext = pokerTimeDbContext;
            _mapper = mapper;
        }

        public async Task<ParticipantInfo> Handle(GetParticipantQuery request, CancellationToken cancellationToken) {
            var result = await _pokerTimeDbContext.Participants.
                    Where(x => x.Session.UrlId.StringId == request.SessionId && x.Name == request.Name).
                    ProjectTo<ParticipantInfo>(_mapper.ConfigurationProvider).
                    FirstOrDefaultAsync(cancellationToken);

            return result;
        }
    }
}
