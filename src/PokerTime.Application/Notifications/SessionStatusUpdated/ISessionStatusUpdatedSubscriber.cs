namespace PokerTime.Application.Notifications.SessionStatusUpdated {
    using System.Threading.Tasks;
    using Sessions.Queries.GetSessionStatus;

    public interface ISessionStatusUpdatedSubscriber : ISubscriber {
        Task OnSessionStatusUpdated(SessionStatus sessionStatus);
    }
}
