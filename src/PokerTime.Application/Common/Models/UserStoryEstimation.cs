namespace PokerTime.Application.Common.Models {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Entities;
    using Mapping;

    public sealed class UserStoryEstimation : IMapFrom<UserStory> {
        public int Id { get; }

        public string Title { get; }

        public ICollection<EstimationModel> Estimations { get; }

        public UserStoryEstimation(int id, string title, IEnumerable<EstimationModel> estimations) {
            var allEstimations = estimations.ToList();

            Id = id;
            Title = title;
            Estimations = allEstimations.OrderByDescending(x => allEstimations.Count(e => e.Symbol.Id == x.Symbol.Id)).ToList();
        }

        // This constructor exists for the automapping
        public UserStoryEstimation() {
            Id = 0;
            Title = null;
            Estimations = Array.Empty<EstimationModel>();
        }
    }
}
