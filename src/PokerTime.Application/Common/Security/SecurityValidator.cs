namespace PokerTime.Application.Common.Security {
    using System;
    using System.Threading.Tasks;
    using Abstractions;
    using Domain.Abstractions;
    using Domain.Entities;
    using Microsoft.Extensions.Logging;
    using Models;
    using TypeHandling;

    public interface ISecurityValidator {
        ValueTask EnsureOperation(Session session, SecurityOperation operation, object entity);
    }

    public sealed class SecurityValidator : ISecurityValidator {
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ILogger<SecurityValidator> _logger;

        public SecurityValidator(ICurrentParticipantService currentParticipantService, ILogger<SecurityValidator> logger) {
            _currentParticipantService = currentParticipantService;
            _logger = logger;
        }

        public async ValueTask EnsureOperation(Session session, SecurityOperation operation, object entity) {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var participant = await GetAuthenticatedParticipant(operation, entity.GetType());

            if (operation == SecurityOperation.AddOrUpdate || operation == SecurityOperation.Delete) {
                EnsureOperationSecurity(operation, entity, participant);
            }

            InvokeTypeSecurityChecks(operation, session, participant, entity);
        }

        private async ValueTask<CurrentParticipantModel> GetAuthenticatedParticipant(SecurityOperation operation, Type entityType) {
            var participant = await _currentParticipantService.GetParticipant();

            static void ThrowSecurityException(string message) {
                throw new OperationSecurityException(message);
            }

            if (participant.IsAuthenticated == false) {
                var message = $"Operation {operation} on type {entityType} not allowed: user is not authenticated.";
                _logger.LogError(message);

                ThrowSecurityException(message);
            }

            return participant;
        }

        private void EnsureOperationSecurity(
            SecurityOperation operation,
            object entity,
            in CurrentParticipantModel participant
        ) {
            if (entity is IOwnedByParticipant ownedEntity) {
                if (participant.Id != ownedEntity.ParticipantId && ownedEntity.ParticipantId != 0) {
                    var message =
                        $"Operation '{operation}': Not allowed - entity is owned by participant {ownedEntity.ParticipantId}. Operation is performed by {participant.Id} ({participant.Name})";
                    _logger.LogError(message);

                    throw new OperationSecurityException(message);
                }
            }
        }

        private void InvokeTypeSecurityChecks(SecurityOperation operation, Session session, in CurrentParticipantModel participant, object entity) {
            try {
                SecurityTypeHandlers.HandleOperation(operation, session, entity, participant);

                if (_logger.IsEnabled(LogLevel.Trace)) {
                    _logger.LogTrace($"Operation {operation} granted for entity {entity.GetType()} for participant #{participant.Id}");
                }
            }
            catch (OperationSecurityException ex) {
                var message =
                    $"Failure asserting operation '{operation}' for entity {entity.GetType()} for participant #{participant.Id} ({participant.Name})";
                _logger.LogError(ex, message);

                throw new OperationSecurityException(message, ex);
            }
        }
    }
}
