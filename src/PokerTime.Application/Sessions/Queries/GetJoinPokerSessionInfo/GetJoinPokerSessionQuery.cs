namespace PokerTime.Application.Sessions.Queries.GetJoinPokerSessionInfo {
    using MediatR;

    public sealed class GetJoinPokerSessionInfoQuery : IRequest<JoinPokerSessionInfo> {

        public string SessionId { get; set; }
    }
}
