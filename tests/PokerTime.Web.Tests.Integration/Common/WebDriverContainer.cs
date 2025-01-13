namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using OpenQA.Selenium;

    public sealed class WebDriverContainer : IDisposable {
        private readonly PokerTimeAppFactory _owner;
        private IWebDriver _webDriver;

        internal WebDriverContainer(IWebDriver webDriver, PokerTimeAppFactory owner) {
            _webDriver = webDriver;
            _owner = owner;
        }

        public IWebDriver WebDriver => _webDriver ?? throw new ObjectDisposedException(ToString());

        public void Dispose() {
            if (_webDriver != null) {
                _owner.Return(_webDriver);
                _webDriver = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
