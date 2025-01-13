namespace PokerTime.Application.Estimations.Queries {
    using System.Collections.Generic;
    using System.Linq;
    using Common.Models;

    public sealed class GetEstimationsOverviewQueryResponse {
        public ICollection<UserStoryEstimation> UserStoryEstimations { get; }

        public GetEstimationsOverviewQueryResponse(IEnumerable<UserStoryEstimation> userStoryEstimations) {
            UserStoryEstimations = userStoryEstimations.ToList();
        }
    }
}
