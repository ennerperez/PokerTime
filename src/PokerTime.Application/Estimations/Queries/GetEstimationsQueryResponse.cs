namespace PokerTime.Application.Estimations.Queries {
    using System.Collections.Generic;
    using Common.Models;

    public sealed class GetEstimationsQueryResponse {
        public ICollection<EstimationModel> Estimations { get; }

        public GetEstimationsQueryResponse(ICollection<EstimationModel> estimations) {
            Estimations = estimations;
        }
    }
}
