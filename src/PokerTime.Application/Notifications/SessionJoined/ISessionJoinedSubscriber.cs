namespace PokerTime.Application.Notifications.SessionJoined {
    using System.Threading.Tasks;
    using Sessions.Queries.GetParticipantsInfo;

    public interface ISessionJoinedSubscriber : ISubscriber {
        Task OnParticipantJoinedSession(SessionEvent<ParticipantInfo> eventArgs);
    }
}
