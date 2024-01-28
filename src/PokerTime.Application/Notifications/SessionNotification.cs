namespace PokerTime.Application.Notifications {
    using MediatR;

    public abstract class SessionNotification : INotification {
        protected SessionNotification(string sessionId) {
            SessionId = sessionId;
        }

        public string SessionId { get; }
    }
}
