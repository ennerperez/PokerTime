namespace PokerTime.Application.Notifications.SessionJoined {
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sessions.Queries.GetParticipantsInfo;

    public sealed class SessionJoinedNotificationDispatcher : NotificationDispatcher<SessionJoinedNotification, ISessionJoinedSubscriber> {
        public SessionJoinedNotificationDispatcher(ILogger<NotificationDispatcher<SessionJoinedNotification, ISessionJoinedSubscriber>> logger) : base(logger)
        {
        }

        protected override Task DispatchCore(ISessionJoinedSubscriber subscriber, SessionJoinedNotification notification) => subscriber.OnParticipantJoinedSession(new SessionEvent<ParticipantInfo>(notification.SessionId, notification.ParticipantInfo));
    }
}
