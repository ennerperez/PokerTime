namespace PokerTime.Web.Services
{
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    public class ClipboardService
    {
        private readonly IJSRuntime _jsRuntime;

        public ClipboardService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public ValueTask WriteTextAsync(string text) => _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}
