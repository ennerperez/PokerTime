namespace PokerTime.Web.Tests.Integration.Components {
    using System;
    using Common;
    using OpenQA.Selenium;

    public class CardComponent {
        public CardComponent(IWebElement webElement) {
            WebElement = webElement;
        }

        public IWebElement WebElement { get; }

        public int Id => WebElement.GetAttribute<int>("data-id");
        public int SymbolId => WebElement.GetAttribute<int>("data-symbol-id");
        public string SymbolText => WebElement.FindElementByTestElementId("symbol").Text.Trim();
        public bool IsEnabled => WebElement.GetAttribute("class").Contains("user-card--enabled", StringComparison.OrdinalIgnoreCase);

        public void Click() => WebElement.Click();
    }
}
