namespace PokerTime.Application.Common.Behaviours {
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public sealed class RequestPerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull, IRequest<TResponse> {
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ILogger<TRequest> _logger;
        private readonly Stopwatch _timer;

        public RequestPerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentParticipantService currentParticipantService
        ) {
            _timer = new Stopwatch();

            _logger = logger;
            _currentParticipantService = currentParticipantService;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        ) {
            if (next == null) throw new ArgumentNullException(nameof(next));

            _timer.Start();

            var response = await next().ConfigureAwait(continueOnCapturedContext: false);

            _timer.Stop();

            if (_timer.ElapsedMilliseconds > 500) {
                var name = typeof(TRequest).Name;

                _logger.LogWarning(
                    message:
                    "PokerTime.App Long running request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                    name,
                    _timer.ElapsedMilliseconds,
                    (await _currentParticipantService.GetParticipant().ConfigureAwait(false)).Id,
                    request);
            }

            return response;
        }
    }
}
