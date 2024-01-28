namespace PokerTime.Application.SessionWorkflows.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Common;
    using Domain.Entities;
    using MediatR;
    using Services;

    public abstract class AbstractStageCommandHandler<TRequest> : IRequestHandler<TRequest> where TRequest : AbstractStageCommand, IRequest {
        private readonly ISessionStatusUpdateDispatcher _sessionStatusUpdateDispatcher;
        private readonly IPokerTimeDbContextFactory _dbContextFactory;


        protected IPokerTimeDbContext DbContext { get; private set; }


        protected AbstractStageCommandHandler(IPokerTimeDbContextFactory pokerTimeDbContext, ISessionStatusUpdateDispatcher sessionStatusUpdateDispatcher) {
            _dbContextFactory = pokerTimeDbContext;
            _sessionStatusUpdateDispatcher = sessionStatusUpdateDispatcher;
        }

        public async Task Handle(TRequest request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                DbContext = _dbContextFactory.CreateForEditContext();
                var session = await DbContext.Sessions.FindBySessionId(request.SessionId, cancellationToken);

                if (session == null) {
                    throw new NotFoundException();
                }

                await HandleCore(request, session, cancellationToken);
            }
            finally {
                DbContext?.Dispose();
            }
        }

        protected abstract Task<Unit> HandleCore(TRequest request, Session session, CancellationToken cancellationToken);

        protected Task DispatchUpdate(Session session, CancellationToken cancellationToken) => _sessionStatusUpdateDispatcher.DispatchUpdate(session, cancellationToken);
    }
}
