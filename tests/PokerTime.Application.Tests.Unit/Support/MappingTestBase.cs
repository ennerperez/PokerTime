namespace PokerTime.Application.Tests.Unit.Support {
    using Application.Common.Mapping;
    using AutoMapper;

    public class MappingTestBase {
        public MappingTestBase() {
            ConfigurationProvider =
                new MapperConfiguration(configure: cfg => { cfg.AddProfile<MappingProfile>(); });

            Mapper = ConfigurationProvider.CreateMapper();
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public IMapper Mapper { get; }
    }
}
