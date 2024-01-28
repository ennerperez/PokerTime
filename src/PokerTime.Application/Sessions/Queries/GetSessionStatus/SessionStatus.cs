namespace PokerTime.Application.Sessions.Queries.GetSessionStatus {
    using Common.Models;
    using Domain.Entities;

    public sealed class SessionStatus {
        public string SessionId { get; }

        public int SymbolSetId { get; }

        public string Title { get; }

        public SessionStage Stage { get; }

        public UserStoryModel UserStory { get; }

        public bool CanViewOwnCards => Stage != SessionStage.Finished && Stage != SessionStage.NotStarted;
        public bool CanChooseCards => Stage == SessionStage.Estimation;
        public bool CanViewEstimationPanel => Stage != SessionStage.Discussion && Stage != SessionStage.Finished && Stage != SessionStage.NotStarted;
        public bool CanViewEstimations => Stage == SessionStage.EstimationDiscussion || Stage == SessionStage.Finished;
        public bool ShowUserStoriesOverview => Stage == SessionStage.Finished;
        public bool IsStarted => Stage != SessionStage.NotStarted;

        public SessionStatus(
            string sessionId,
            string title,
            SessionStage sessionStage,
            int symbolSetId,
            UserStoryModel currentUserStory
        ) {
            SessionId = sessionId;
            Title = title;
            SymbolSetId = symbolSetId;
            Stage = sessionStage;
            UserStory = currentUserStory;
        }

        public SessionStatus() {
            SessionId = string.Empty;
            Title = string.Empty;
            Stage = SessionStage.NotStarted;
        }
    }
}
