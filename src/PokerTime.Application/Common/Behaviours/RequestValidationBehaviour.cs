namespace PokerTime.Application.Common.Behaviours {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using MediatR;
    using ValidationException = ValidationException;

    public sealed class RequestValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull, IRequest<TResponse> {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public RequestValidationBehaviour(IEnumerable<IValidator<TRequest>> validators) {
            _validators = validators;
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        ) {
            if (next == null) throw new ArgumentNullException(nameof(next));
            var context = new ValidationContext<TRequest>(request);

            var failures = _validators.Select(selector: v => v.Validate(context: context)).
                SelectMany(selector: result => result.Errors).
                Where(predicate: f => f != null).
                ToList();

            if (failures.Count != 0) {
                throw new ValidationException(failures);
            }

            return next();
        }
    }
}
