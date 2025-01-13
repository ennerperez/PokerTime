namespace PokerTime.Application.Notifications.SessionJoined {
    using Sessions.Queries.GetParticipantsInfo;

    public sealed class SessionJoinedNotification : SessionNotification {
        public ParticipantInfo ParticipantInfo { get; }

        public SessionJoinedNotification(string sessionId, ParticipantInfo participantInfo) : base(sessionId) {
            ParticipantInfo = participantInfo;
        }
    }
}
