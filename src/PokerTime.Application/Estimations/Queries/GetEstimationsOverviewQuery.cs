namespace PokerTime.Application.Estimations.Queries {
    using MediatR;

    public sealed class GetEstimationsOverviewQuery : IRequest<GetEstimationsOverviewQueryResponse> {
        public string SessionId { get; }

        public GetEstimationsOverviewQuery(string sessionId) {
            SessionId = sessionId;
        }
    }
}
