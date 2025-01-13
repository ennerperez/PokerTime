namespace PokerTime.Application.Common.Mapping {
    using System;
    using AutoMapper;

    public interface IMapFrom<T> {
        void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            profile.CreateMap(typeof(T), GetType());
        }
    }
}
