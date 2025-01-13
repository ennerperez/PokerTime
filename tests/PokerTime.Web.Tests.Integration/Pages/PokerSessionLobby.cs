namespace PokerTime.Web.Tests.Integration.Pages {
    using System.Threading;
    using Common;
    using Components;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Interactions;

    public sealed class PokerSessionLobby : PageObject {
        public IWebElement WorkflowContinueButton => WebDriver.FindElementByTestElementId("workflow-continue-button");
        public IWebElement WorkflowEndButton => WebDriver.FindElementByTestElementId("workflow-end-button");

        public IWebElement WaitForStartMessageElement => WebDriver.FindElementByTestElementId("wait-for-start-message");
        public IWebElement CardChooserElement => WebDriver.FindElementByTestElementId("card-chooser");
        public IWebElement SessionFinishedElement => WebDriver.FindElementByTestElementId("session-finished-message");
        public IWebElement EstimationOverviewElement => WebDriver.FindElementByTestElementId("estimation-overview");

        public IWebElement UserStoryTitleInput => WebDriver.FindElement(By.Id("pokertime-userstory-title"));

        public CardChooserComponent CardChooser => new CardChooserComponent(CardChooserElement);
        public EstimationOverview EstimationOverview => new EstimationOverview(EstimationOverviewElement);

        public void InvokeContinueWorkflow() {
            WorkflowContinueButton.Click();

            // Insert sleep for AppVeyor and slower CI
            Thread.Sleep(1000);
        }

        public void InvokeEndWorkflow() {
            WorkflowEndButton.Click();

            // Insert sleep for AppVeyor and slower CI
            Thread.Sleep(1000);
        }

        public void SetUserStoryTitle(string title) =>
            new Actions(WebDriver)
                .Click(UserStoryTitleInput)
                .SendKeys(title)
                .SendKeys(Keys.Home)
                .Perform();
    }
}
