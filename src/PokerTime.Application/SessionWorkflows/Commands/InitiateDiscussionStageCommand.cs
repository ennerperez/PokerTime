namespace PokerTime.Application.SessionWorkflows.Commands {
    using MediatR;

    public sealed class InitiateDiscussionStageCommand : AbstractStageCommand, IRequest {
        public string UserStoryTitle { get; set; }

        public bool IsReestimation { get; set; }
    }

}
