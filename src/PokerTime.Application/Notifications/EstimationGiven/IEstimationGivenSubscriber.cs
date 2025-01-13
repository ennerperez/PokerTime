namespace PokerTime.Application.Notifications.EstimationGiven {
    using System.Threading.Tasks;

    public interface IEstimationGivenSubscriber : ISubscriber {
        Task OnEstimationGiven(EstimationGivenNotification notification);
    }
}
