namespace PokerTime.Application.Common.Behaviours {
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using MediatR.Pipeline;
    using Microsoft.Extensions.Logging;

    public sealed class RequestLogger<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull {
        private readonly ILogger _logger;
        private readonly ICurrentParticipantService _currentUserService;

        public RequestLogger(ILogger<TRequest> logger, ICurrentParticipantService currentUserService) {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task Process(TRequest request, CancellationToken cancellationToken) {
            var name = typeof(TRequest).Name;

            _logger.LogInformation("PokerTime.App Request: {Name} {@UserId} {@Request}",
                name, (await _currentUserService.GetParticipant().ConfigureAwait(false)).Id, request);
        }
    }
}
