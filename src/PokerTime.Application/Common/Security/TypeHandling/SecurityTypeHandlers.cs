namespace PokerTime.Application.Common.Security.TypeHandling {
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Domain.Entities;
    using Models;

    internal static class SecurityTypeHandlers {
        private static readonly ITypeSecurityHandler[] All;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "There is only a single field and this is for clarity too.")]
        static SecurityTypeHandlers() {
            var searchAssembly = typeof(SecurityTypeHandlers).Assembly;
            var typeInterface = typeof(ITypeSecurityHandler);

            All =
                (from type in searchAssembly.GetTypes()
                 where type.IsClass
                 where type.IsAbstract == false
                 where typeInterface.IsAssignableFrom(type)
                 let inst = Activator.CreateInstance(type)
                 select (ITypeSecurityHandler)inst).ToArray();

            if (All.Length == 0) {
                Debug.Fail($"Unable to find {typeInterface} implementations");
            }
        }

        public static void HandleOperation(SecurityOperation operation, Session session, object entity, in CurrentParticipantModel currentParticipant) {
            foreach (var handler in All) {
                handler.HandleOperation(operation, session, entity, currentParticipant);
            }
        }
    }
}
