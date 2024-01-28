using System;

namespace PokerTime.Web.Services {
    using Configuration;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public interface ISiteUrlDetectionService {
        void Update(HttpContext httpContext);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "Merged into uri")]
        string GetSiteUrl();
    }

    public class SiteUrlDetectionService : ISiteUrlDetectionService {
        private readonly ILogger<SiteUrlDetectionService> _logger;
        private string _siteUrl;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We log it and continue")]
        public SiteUrlDetectionService(IOptions<ServerOptions> serverOptions, ILogger<SiteUrlDetectionService> logger) {
            if (serverOptions == null) throw new ArgumentNullException(nameof(serverOptions));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _siteUrl = serverOptions.Value?.BaseUrl;

            if (string.IsNullOrEmpty(_siteUrl)) {
                _siteUrl = null;
            }
            else {
                try {
                    _siteUrl = new Uri(_siteUrl, UriKind.Absolute).GetLeftPart(UriPartial.Authority);

                    _logger.LogInformation("Normalized base URL: {0}", _siteUrl);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Unable to normalize base url");
                }
            }
        }


        public void Update(HttpContext httpContext) {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (_siteUrl == null) {
                _logger.LogWarning("You have not set an explicit base URL of the application via the [Server:BaseUrl] option. The base URL is now automatically detected. This detection is possibly insecure, and can lead to incorrect results.");

                var request = httpContext.Request;
                _siteUrl = request.GetUri().GetLeftPart(UriPartial.Authority);

                _logger.LogInformation("Detected base URL: {0}", _siteUrl);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "Merged into uri")]
        public string GetSiteUrl() => _siteUrl ?? throw new InvalidOperationException("Base url not determined yet");
    }
}
