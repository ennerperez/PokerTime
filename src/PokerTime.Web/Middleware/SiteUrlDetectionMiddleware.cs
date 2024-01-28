namespace PokerTime.Web.Middleware {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Services;

    public sealed class SiteUrlDetectionMiddleware {
        private readonly ISiteUrlDetectionService _siteUrlDetectionService;
        private readonly RequestDelegate _next;

        public SiteUrlDetectionMiddleware(ISiteUrlDetectionService siteUrlDetectionService, RequestDelegate next) {
            _siteUrlDetectionService = siteUrlDetectionService;
            _next = next;
        }

        public Task Invoke(HttpContext context) {
            _siteUrlDetectionService.Update(context);

            return _next.Invoke(context);
        }
    }
}
