namespace PokerTime.Application.Sessions.Queries.GetParticipantsInfo {
    using MediatR;

    public sealed class GetParticipantsInfoQuery : IRequest<ParticipantsInfoList> {
        public GetParticipantsInfoQuery(string sessionId) {
            SessionId = sessionId;
        }

        public string SessionId { get; }
    }
}
