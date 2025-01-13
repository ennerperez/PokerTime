namespace PokerTime.Web.Tests.Integration.Components {
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using OpenQA.Selenium;

    public class EstimationOverview {
        public EstimationOverview(IWebElement webElement) {
            WebElement = webElement;
        }

        public IWebElement WebElement { get; }

        public IEnumerable<CardComponent> Cards => WebElement.FindElementsByTestElementId("estimation-card").Select(x => new CardComponent(x));
        public IEnumerable<string> UnestimatedCards => WebElement.FindElementsByTestElementId("unestimated-card").Select(x => x.GetAttribute("data-participant-name"));
    }
}
