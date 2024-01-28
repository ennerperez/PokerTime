namespace PokerTime.Web.Tests.Integration.Components {
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using OpenQA.Selenium;

    public class CardChooserComponent {
        public CardChooserComponent(IWebElement webElement) {
            WebElement = webElement;
        }

        public IWebElement WebElement { get; }

        public IEnumerable<CardComponent> Cards => WebElement.FindElementsByTestElementId("user-poker-card").Select(x => new CardComponent(x));
    }
}
