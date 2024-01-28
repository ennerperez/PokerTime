namespace PokerTime.Web.Services {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Behaviours;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public sealed class ScopeSafeMediatorDecorator : IMediator, IDisposable {
        private readonly IMediator _mediator;
        private readonly SemaphoreSlim _lock;
        private readonly ILogger<ScopeSafeMediatorDecorator> _logger;

        public ScopeSafeMediatorDecorator(IMediator mediator, ILogger<ScopeSafeMediatorDecorator> logger) {
            _mediator = mediator;
            _logger = logger;
            _lock = new SemaphoreSlim(1, 1);
        }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = new CancellationToken()
        ) {
            if (request is ILockFreeRequest) {
                return _mediator.Send(request, cancellationToken);
            }

            return SendWithRequestLock(request: request, cancellationToken: cancellationToken);
        }

        private async Task<TResponse> SendWithRequestLock<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken) {
            try {
                await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

                return await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
            }
            finally {
                try {
                    _lock.Release();
                }
                catch (ObjectDisposedException ex) {
                    _logger.LogWarning(ex, "Semaphore was already disposed - this may happen after a crash.");
                }
            }
        }
        private async Task SendWithRequestLock(IRequest request, CancellationToken cancellationToken) {
            try {
                await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

                await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
            }
            finally {
                try {
                    _lock.Release();
                }
                catch (ObjectDisposedException ex) {
                    _logger.LogWarning(ex, "Semaphore was already disposed - this may happen after a crash.");
                }
            }
        }
        public Task<object> Send(object request, CancellationToken cancellationToken = new CancellationToken()) => throw new NotSupportedException("We don't implement this currently. If this exception is thrown, we should probably implement it!");
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = new CancellationToken()) where TRequest : IRequest // => throw new NotImplementedException();
        {
            if (request is ILockFreeRequest) {
                return _mediator.Send(request, cancellationToken);
            }

            return SendWithRequestLock(request: request, cancellationToken: cancellationToken);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = new CancellationToken()
        ) =>
            throw new NotImplementedException();

        public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

        public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken()) => _mediator.Publish(notification, cancellationToken);

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification => _mediator.Publish(notification, cancellationToken);

        public void Dispose() => _lock?.Dispose();
    }
}
