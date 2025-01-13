namespace PokerTime.Application.SessionWorkflows.Common {
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Entities;
    using MediatR;
    using Notifications.SessionStatusUpdated;
    using Sessions.Queries.GetSessionStatus;

    public interface ISessionStatusUpdateDispatcher {
        Task DispatchUpdate(Session session, CancellationToken cancellationToken);
    }

    public sealed class SessionStatusUpdateDispatcher : ISessionStatusUpdateDispatcher {
        private readonly ISessionStatusMapper _sessionStatusMapper;
        private readonly IMediator _mediator;

        public SessionStatusUpdateDispatcher(ISessionStatusMapper sessionStatusMapper, IMediator mediator) {
            _sessionStatusMapper = sessionStatusMapper;
            _mediator = mediator;
        }

        public async Task DispatchUpdate(Session session, CancellationToken cancellationToken) {
            var sessionStatus = await _sessionStatusMapper.GetSessionStatus(session, cancellationToken);

            await _mediator.Publish(new SessionStatusUpdatedNotification(sessionStatus), cancellationToken);
        }
    }
}
