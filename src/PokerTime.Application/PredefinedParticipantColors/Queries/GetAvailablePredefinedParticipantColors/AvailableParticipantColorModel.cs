namespace PokerTime.Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors {
    using System;
    using AutoMapper;
    using Common.Models;
    using Domain.Entities;



    public class AvailableParticipantColorModel : ColorModel {
        public string Name { get; set; }

        public override void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            profile.CreateMap<PredefinedParticipantColor, AvailableParticipantColorModel>().
                ForMember(e => e.R, m => m.MapFrom(e => e.Color.R)).
                ForMember(e => e.B, m => m.MapFrom(e => e.Color.B)).
                ForMember(e => e.G, m => m.MapFrom(e => e.Color.G));
        }
    }
}
