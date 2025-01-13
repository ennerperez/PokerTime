namespace PokerTime.Application.Sessions.Queries.GetSessionStatus {
    using MediatR;

    public sealed class GetSessionStatusQuery : IRequest<SessionStatus> {
        public string SessionId { get; }

        public GetSessionStatusQuery(string sessionId) {
            SessionId = sessionId;
        }
    }
}
