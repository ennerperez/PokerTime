namespace PokerTime.Application.Common.Models {
    using System;
    using AutoMapper;
    using Domain.ValueObjects;
    using Mapping;

    public class ColorModel : IMapFrom<ParticipantColor> {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public string HexString => $"{R:X2}{G:X2}{B:X2}";

        public virtual void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            profile.CreateMap<ParticipantColor, ColorModel>();
        }

        public bool HasSameColors(ColorModel other) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return (other.R, other.G, other.B) == (R, G, B);
        }
    }
}
