namespace PokerTime.Application.Common.Models {
    using System;
    using AutoMapper;
    using Domain.Entities;
    using Mapping;



    public sealed class EstimationModel : IMapFrom<Estimation> {
        public int Id { get; set; }

        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public ColorModel ParticipantColor { get; set; }

        public SymbolModel Symbol { get; set; }

        public void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            profile.CreateMap<Estimation, EstimationModel>();
        }
    }
}
