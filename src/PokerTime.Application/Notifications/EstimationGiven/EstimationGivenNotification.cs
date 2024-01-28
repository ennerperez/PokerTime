namespace PokerTime.Application.Notifications.EstimationGiven {
    using Common.Models;
    using MediatR;

    public sealed class EstimationGivenNotification : INotification {
        public EstimationModel Estimation { get; }

        public string SessionId { get; }

        public EstimationGivenNotification(string sessionId, EstimationModel estimation) {
            SessionId = sessionId;
            Estimation = estimation;
        }
    }
}
