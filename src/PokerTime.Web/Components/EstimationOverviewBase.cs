namespace PokerTime.Web.Components {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Common.Models;
    using Application.Estimations.Queries;
    using Application.Notifications;
    using Application.Notifications.EstimationGiven;
    using Application.Notifications.SessionJoined;
    using Application.Sessions.Queries.GetParticipantsInfo;
    using Application.Sessions.Queries.GetSessionStatus;
    using Microsoft.AspNetCore.Components;

    public abstract class EstimationOverviewBase : MediatorComponent, IDisposable, IEstimationGivenSubscriber, ISessionJoinedSubscriber {
        private int _userStoryId;


        [CascadingParameter]
        public SessionStatus SessionStatus { get; set; }

        [Inject]
        public INotificationSubscription<IEstimationGivenSubscriber> EstimationGivenSubscriber { get; set; }

        [Inject]
        public INotificationSubscription<ISessionJoinedSubscriber> SessionJoinedSubscriber { get; set; }

        public Guid UniqueId { get; } = Guid.NewGuid();

        private ParticipantsInfoList ParticipantList { get; set; }

        protected IEnumerable<string> ParticipantsWithoutEstimation {
            get {
                if (ParticipantList == null) {
                    yield break;
                }

                foreach (var participant in ParticipantList.Participants) {
                    if (!Estimations.ContainsKey(participant.Id)) {
                        yield return participant.Name;
                    }
                }
            }
        }

#nullable restore

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        protected IDictionary<int, EstimationModel> Estimations { get; set; } = new Dictionary<int, EstimationModel>();

        public Task OnEstimationGiven(EstimationGivenNotification notification) {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            if (notification.SessionId != SessionStatus.SessionId) {
                return Task.CompletedTask;
            }

            InvokeAsync(() => {
                Estimations[notification.Estimation.ParticipantId] = notification.Estimation;

                StateHasChanged();
            });

            return Task.CompletedTask;
        }

        protected override void OnInitialized() {
            base.OnInitialized();

            EstimationGivenSubscriber.Subscribe(this);
            SessionJoinedSubscriber.Subscribe(this);
        }

        protected override async Task OnInitializedAsync() {
            ParticipantList = await Mediator.Send(new GetParticipantsInfoQuery(SessionStatus.SessionId));
            Debug.Assert(ParticipantList != null);
        }

        protected override Task OnParametersSetAsync() {
            if (SessionStatus.UserStory?.Id == _userStoryId) {
                return base.OnParametersSetAsync();
            }

            if (SessionStatus.UserStory == null) {
                _userStoryId = 0;
                return base.OnParametersSetAsync();
            }

            var userStoryId = SessionStatus.UserStory.Id;
            async Task LoadCore() {
                await base.OnParametersSetAsync();

                var estimationsResponse = await Mediator.Send(
                    new GetEstimationsQuery(SessionStatus.SessionId, userStoryId));

                Estimations = estimationsResponse.Estimations.ToDictionary(x => x.ParticipantId, x => x);
                _userStoryId = userStoryId;
            }

            return LoadCore();
        }

        public Task OnParticipantJoinedSession(SessionEvent<ParticipantInfo> eventArgs) {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

            if (eventArgs.SessionId != SessionStatus?.SessionId) {
                return Task.CompletedTask;
            }

            var participantInfo = eventArgs.Argument;

            return InvokeAsync(() => {
                ParticipantList.Participants.Add(participantInfo);
                ParticipantList.Participants.Sort((a, b) => StringComparer.CurrentCulture.Compare(a.Name, b.Name));

                StateHasChanged();
            });
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                EstimationGivenSubscriber?.Unsubscribe(this);
                SessionJoinedSubscriber?.Unsubscribe(this);
            }
        }

        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
