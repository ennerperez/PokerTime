namespace PokerTime.Application.Common.Mapping {
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;

    public class MappingProfile : Profile {
        public MappingProfile() {
            ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var mapFromType = typeof(IMapFrom<>);
            var types = assembly.GetExportedTypes().
                Where(predicate: t => t.GetInterfaces().
                    Any(predicate: i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType)).
                ToList();

            foreach (var type in types) {
                var instance = Activator.CreateInstance(type: type);

                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == mapFromType)
                    {
                        var methodInfo = interfaceType.GetMethod(name: "Mapping");
                        methodInfo.Invoke(obj: instance, new object[] { this });
                    }
                }
            }
        }
    }
}
