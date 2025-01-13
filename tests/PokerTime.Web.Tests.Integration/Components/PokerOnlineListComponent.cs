namespace PokerTime.Web.Tests.Integration.Components {
    using System.Collections.Generic;
    using Common;
    using OpenQA.Selenium;

    public class PokerOnlineListComponent {
        private readonly IWebDriver _webDriver;

        public PokerOnlineListComponent(IWebDriver webDriver) {
            _webDriver = webDriver;
        }

        public IEnumerable<IWebElement> OnlineListItems => _webDriver.FindElements(By.CssSelector("#poker-online-list span[data-participant-id]"));
        public IWebElement GetListItem(int id) => _webDriver.FindVisibleElement(By.CssSelector($"#poker-online-list span[data-participant-id=\"{id}\"]"));
    }
}
