namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.Extensions;

    public abstract class PageObject : IPageObject {
        private bool _ownsWebdriver;
        private WebDriverContainer _webDriverContainer;

        public IWebDriver WebDriver => _webDriverContainer?.WebDriver;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1033:Interface methods should be callable by child types",
            Justification = "Not necessary for testing framework")]
        void IPageObject.SetWebDriver(WebDriverContainer webDriver) {
            _webDriverContainer = webDriver;
            _ownsWebdriver = true;
        }

        public void Unfocus() {
            TestContext.WriteLine("Unfocus by sending tab");
            WebDriver.FindElement(By.CssSelector("body")).SendKeys("\t");
        }

        public void InitializeFrom(PageObject owner) {
            _webDriverContainer = owner._webDriverContainer;
            _ownsWebdriver = false;
        }

        public void ScrollDown()
        {
            WebDriver.ExecuteJavaScript("window.scrollTo(0, document.body.scrollHeight)");
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_ownsWebdriver) {
                    _webDriverContainer?.Dispose();
                }

                _webDriverContainer = null;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IPageObject : IDisposable {
        void SetWebDriver(WebDriverContainer webDriver);

        IWebDriver WebDriver { get; }
    }
}
