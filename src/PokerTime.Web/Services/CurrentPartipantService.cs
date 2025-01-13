namespace PokerTime.Web.Services {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Common;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class CurrentParticipantService : ICurrentParticipantService {
        private const string ParticipantClaimType = ClaimTypes.NameIdentifier;
        private const string ParticipantNameClaimType = ClaimTypes.Name;
        private const string ParticipantColorClaimType = ClaimTypes.Country;
        private const string FacilitatorClaimType = ClaimTypes.Role;
        private const string FacilitatorClaimContent = "Facilitator";
        private HttpContext _httpContext;

        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private bool _hasNoHttpContext;
        private ClaimsPrincipal _currentClaimsPrincipal;

        public CurrentParticipantService(AuthenticationStateProvider authenticationStateProvider) {
            _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));

            _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task) =>
            task.ContinueWith(t => {
                if (t.IsCompletedSuccessfully) {
                    _currentClaimsPrincipal = t.Result?.User;
                }
            }, TaskScheduler.Current);

        internal void SetHttpContext(HttpContext httpContext) {
            _httpContext = httpContext;
            _hasNoHttpContext = false;
        }

        internal void SetNoHttpContext() => _hasNoHttpContext = true;

        public void SetParticipant(CurrentParticipantModel currentParticipant) {
            var hostEnvProvider = _authenticationStateProvider as IHostEnvironmentAuthenticationStateProvider;

            if (hostEnvProvider == null) {
                return;
            }

            (var participantId, var name, var color, var isFacilitator) = currentParticipant;

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ParticipantClaimType, participantId.ToString(Culture.Invariant), participantId.GetType().FullName));
            if (name is not null) identity.AddClaim(new Claim(ParticipantNameClaimType, name, typeof(string).FullName));
            if (color is not null) identity.AddClaim(new Claim(ParticipantColorClaimType, color, typeof(string).FullName));
            if (isFacilitator) {
                identity.AddClaim(new Claim(FacilitatorClaimType, FacilitatorClaimContent, FacilitatorClaimContent.GetType().FullName));
            }

            _currentClaimsPrincipal = new ClaimsPrincipal(identity);

            hostEnvProvider.SetAuthenticationState(Task.FromResult(new AuthenticationState(_currentClaimsPrincipal)));
        }

        public async ValueTask<CurrentParticipantModel> GetParticipant() {
            var user = await GetUser().ConfigureAwait(false);

            return new CurrentParticipantModel(
                GetParticipantId(user),
                GetNameLocal(user),
                GetColor(user),
                IsFacilitator(user)
            );
        }

        private async ValueTask<ClaimsPrincipal> GetUser() {
            if (_hasNoHttpContext) {
                return new ClaimsPrincipal();
            }

            if (_currentClaimsPrincipal != null) {
                return _currentClaimsPrincipal;
            }

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);

            if (authState != null) {
                _currentClaimsPrincipal = authState.User;
                return authState.User;
            }

            if (_httpContext == null) {
                throw new InvalidOperationException("HttpContext not set");
            }

            return _httpContext.User;
        }

        private static string GetNameLocal(ClaimsPrincipal user) => user.FindFirstValue(ParticipantNameClaimType);
        private static string GetColor(ClaimsPrincipal user) => user.FindFirstValue(ParticipantColorClaimType);

        private static bool IsFacilitator(ClaimsPrincipal user) {
            var rawParticipantId = user.FindFirstValue(FacilitatorClaimType);
            if (string.IsNullOrEmpty(rawParticipantId)) {
                return default;
            }

            return string.Equals(rawParticipantId, FacilitatorClaimContent, StringComparison.Ordinal);
        }

        private static int GetParticipantId(ClaimsPrincipal user) {
            var rawParticipantId = user.FindFirstValue(ParticipantClaimType);
            if (string.IsNullOrEmpty(rawParticipantId)) {
                return default;
            }

            if (!int.TryParse(rawParticipantId, out var participantId)) {
                return default;
            }

            return participantId;
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class CurrentParticipantServiceHttpContextSetterMiddleware {
        private readonly RequestDelegate _next;

        public CurrentParticipantServiceHttpContextSetterMiddleware(RequestDelegate next) {
            _next = next;
        }

        public Task InvokeAsync(HttpContext httpContext) {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var currentParticipantService =
                (CurrentParticipantService)httpContext.RequestServices.
                    GetRequiredService<ICurrentParticipantService>();
            currentParticipantService.SetHttpContext(httpContext);

            return _next.Invoke(httpContext);
        }
    }

    public static class AppBuilderExtensions {
        public static void UseCurrentParticipantService(this IApplicationBuilder appBuilder) {
            appBuilder.UseMiddleware<CurrentParticipantServiceHttpContextSetterMiddleware>();
        }
    }
}
