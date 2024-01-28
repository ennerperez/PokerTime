namespace PokerTime.Application.Sessions.Commands.RejoinPokerSession {
    using MediatR;

    public sealed class RejoinPokerSessionCommand : IRequest {
        public string SessionId { get; }

        public int ParticipantId { get; }

        public RejoinPokerSessionCommand(string sessionId, int participantId) {
            SessionId = sessionId;
            ParticipantId = participantId;
        }
    }
}
