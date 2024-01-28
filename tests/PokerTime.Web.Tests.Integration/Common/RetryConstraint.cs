namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using PokerTime.Common;

    /// <summary>
    ///     Provides extension methods for a constraint that can be retried
    /// </summary>
    public static class RetryConstraint {
        /// <summary>
        ///     Gets or sets the time to wait between retrying
        /// </summary>
        public static int DefaultRetryTimeGap { get; set; } = 1000;

        /// <summary>
        ///     Gets or sets the number of time to retry
        /// </summary>
        public static int DefaultRetryCount { get; set; } = 10;

        /// <summary>
        ///     Specifies the current constraint must be retried a certain amount of time
        /// </summary>
        public static IResolveConstraint Retry(this IResolveConstraint source, int count = -1, int timeGap = -1) {
            if (count <= 0) {
                count = DefaultRetryCount;
            }

            if (timeGap <= 0) {
                timeGap = DefaultRetryTimeGap;
            }

            var settings = new RetrySettings(retryCount: count, timeGap: timeGap);
            if (source is IConstraint constraint) {
                return new RetryableWithResolveConstraintImpl(wrapped: constraint, retrySettings: settings);
            }

            return new RetryableResolveConstraintImpl(wrapped: source, retrySettings: settings);
        }

        internal static bool IsRetry(IResolveConstraint source) =>
            source is RetryableResolveConstraintImpl || source is RetryableWithResolveConstraintImpl;

        [StructLayout(layoutKind: LayoutKind.Auto)]
        private struct RetrySettings {
            public RetrySettings(int retryCount, int timeGap) {
                RetryCount = retryCount;
                TimeGap = timeGap;
            }

            public override string ToString() => string.Format(Culture.Invariant, "Retry {0} times waiting {1} ms", RetryCount, TimeGap);

            public readonly int TimeGap;

            public readonly int RetryCount;
        }


        private sealed class RetryableResolveConstraintImpl : IResolveConstraint {
            private readonly RetrySettings _retrySettings;


            private readonly IResolveConstraint _wrapped;

            /// <inheritdoc />
            public RetryableResolveConstraintImpl(IResolveConstraint wrapped, RetrySettings retrySettings) {
                _wrapped = wrapped;
                _retrySettings = retrySettings;
            }

            /// <inheritdoc />
            public IConstraint Resolve() =>
                new RetryableWithResolveConstraintImpl(_wrapped.Resolve(), retrySettings: _retrySettings);
        }


        private sealed class RetryableWithResolveConstraintImpl : IConstraint {
            private readonly RetrySettings _retrySettings;


            private readonly IConstraint _wrapped;

            /// <inheritdoc />
            public RetryableWithResolveConstraintImpl(IConstraint wrapped, RetrySettings retrySettings) {
                _wrapped = wrapped;
                _retrySettings = retrySettings;
            }


            public IConstraint Resolve() =>
                new RetryableWithResolveConstraintImpl(_wrapped.Resolve(), retrySettings: _retrySettings);

            /// <inheritdoc />
            public ConstraintResult ApplyTo<TActual>(TActual actual) =>
                ApplyToRetry(resultFactory: () => _wrapped.ApplyTo(actual: actual));

            /// <inheritdoc />
            public ConstraintResult ApplyTo<TActual>(ActualValueDelegate<TActual> del) =>
                ApplyToRetry(resultFactory: () => _wrapped.ApplyTo(del: del));

            /// <inheritdoc />
            public ConstraintResult ApplyTo<TActual>(ref TActual actual) {
                var copy = actual;
                ConstraintResult result;
                try {
                    result = ApplyToRetry(resultFactory: delegate {
                        var copy2 = copy;
                        var result2 = _wrapped.ApplyTo(actual: ref copy2);
                        copy = copy2;
                        return result2;
                    });
                }
                finally {
                    actual = copy;
                }

                return result;
            }

            /// <inheritdoc />

            public string DisplayName => $"{_wrapped.DisplayName} ({_retrySettings})";

            /// <inheritdoc />

            public string Description => $"{_wrapped.Description} ({_retrySettings})";

            /// <inheritdoc />

            public object[] Arguments => _wrapped.Arguments;

            /// <inheritdoc />
            public ConstraintBuilder Builder {
                get => _wrapped.Builder;
                set => _wrapped.Builder = value;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Constraint testing")]
            private ConstraintResult ApplyToRetry(Func<ConstraintResult> resultFactory) {
                var result = new ConstraintResult(this, this, ConstraintStatus.Failure);
                for (var counter = 0; counter < _retrySettings.RetryCount; counter++) {
                    try {
                        result = resultFactory();
                        if (result.IsSuccess) {
                            TestContext.WriteLine($"Constraint {result.Status}: {result.Name} {result.Description} - Success");
                            return result;
                        }

                        TestContext.WriteLine($"Constraint {result.Status}: {result.Name} {result.Description} - Retrying");
                    }
                    catch (Exception ex) {
                        TestContext.WriteLine($"Constraint exception: {ex} - Retrying");
                    }

                    Thread.Sleep(_retrySettings.TimeGap);
                }

                return result;
            }
        }
    }
}
