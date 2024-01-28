namespace PokerTime.Application.Sessions.Queries.GetJoinPokerSessionInfo {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Domain.Entities;
    using Domain.Services;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Services;

    public sealed class GetJoinPokerSessionInfoQueryHandler : IRequestHandler<GetJoinPokerSessionInfoQuery, JoinPokerSessionInfo> {
        private readonly IPokerTimeDbContext _dbContext;
        private readonly ILogger<GetJoinPokerSessionInfoQueryHandler> _logger;

        public GetJoinPokerSessionInfoQueryHandler(IPokerTimeDbContext dbContext, ILogger<GetJoinPokerSessionInfoQueryHandler> logger) {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<JoinPokerSessionInfo> Handle(GetJoinPokerSessionInfoQuery request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var session = await _dbContext.Sessions.FindBySessionId(request.SessionId, cancellationToken);
            if (session == null) {
                _logger.LogWarning($"Session with id {request.SessionId} was not found");

                return null;
            }

            _logger.LogInformation($"Session with id {request.SessionId} was found");
            return new JoinPokerSessionInfo(
                session.Title,
                session.HashedPassphrase != null,
                session.IsStarted(),
                session.CurrentStage == SessionStage.Finished);
        }
    }
}
