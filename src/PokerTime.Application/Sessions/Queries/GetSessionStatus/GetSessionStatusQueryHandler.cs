namespace PokerTime.Application.Sessions.Queries.GetSessionStatus {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions;
    using MediatR;
    using Services;

    public sealed class GetSessionStatusQueryHandler : IRequestHandler<GetSessionStatusQuery, SessionStatus> {
        private readonly IPokerTimeDbContext _pokerTimeDbContext;
        private readonly ISessionStatusMapper _mapper;

        public GetSessionStatusQueryHandler(IPokerTimeDbContext pokerTimeDbContext, ISessionStatusMapper mapper) {
            _pokerTimeDbContext = pokerTimeDbContext;
            _mapper = mapper;
        }

        public async Task<SessionStatus> Handle(GetSessionStatusQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var session = await _pokerTimeDbContext.Sessions.FindBySessionId(request.SessionId, cancellationToken);

            if (session == null) {
                throw new NotFoundException();
            }

            return await _mapper.GetSessionStatus(session, cancellationToken);
        }
    }
}
