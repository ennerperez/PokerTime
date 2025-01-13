namespace PokerTime.Application.Notifications {
    public sealed class SessionEvent<T> {
        public string SessionId { get; }

        public T Argument { get; }

        public SessionEvent(string sessionId, T argument) {
            SessionId = sessionId;
            Argument = argument;
        }
    }
}
