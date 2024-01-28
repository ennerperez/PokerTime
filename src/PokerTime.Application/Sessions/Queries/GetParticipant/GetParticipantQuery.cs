namespace PokerTime.Application.Sessions.Queries.GetParticipant {
    using GetParticipantsInfo;
    using MediatR;

    public sealed class GetParticipantQuery : IRequest<ParticipantInfo> {
        public string Name { get; }
        public string SessionId { get; }

        public GetParticipantQuery(string name, string sessionId) {
            Name = name;
            SessionId = sessionId;
        }
    }
}
