namespace PokerTime.Application.SessionWorkflows.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Common;
    using Domain.Entities;
    using MediatR;

    public sealed class InitiateDiscussionStageCommandHandler : AbstractStageCommandHandler<InitiateDiscussionStageCommand> {
        public InitiateDiscussionStageCommandHandler(IPokerTimeDbContext pokerTimeDbContext, ISessionStatusUpdateDispatcher sessionStatusUpdateDispatcher) : base(pokerTimeDbContext, sessionStatusUpdateDispatcher) {
        }

        protected override async Task<Unit> HandleCore(InitiateDiscussionStageCommand request, Session session, CancellationToken cancellationToken) {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.CurrentStage = request.IsReestimation ? SessionStage.Estimation : SessionStage.Discussion;

            var userStory = new UserStory {
                Session = session,
                Title = string.IsNullOrEmpty(request.UserStoryTitle) ? null : request.UserStoryTitle
            };

            DbContext.UserStories.Add(userStory);

            await DbContext.SaveChangesAsync(cancellationToken);

            await DispatchUpdate(session, cancellationToken);

            return Unit.Value;
        }
    }
}
