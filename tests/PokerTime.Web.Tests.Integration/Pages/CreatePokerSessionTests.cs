namespace PokerTime.Web.Tests.Integration.Pages {
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public class CreatePokerSessionTests : PageFixture<CreatePokerSessionPage> {
        [Test]
        public void CreatePokerSession_SubmitWithoutValidation_ShowsValidationMessages() {
            // Given
            Page.Navigate(App);

            // When
            Page.ScrollDown();
            Page.Submit();

            // Then
            var messages = new DefaultWait<CreatePokerSessionPage>(Page)
                .Until(p => {
                    var collection = p.GetValidationMessages();
                    if (collection.Count == 0) return null;
                    return collection;
                })
                .Select(el => el.Text)
                .ToArray();

            Assert.That(messages, Has.One.Contain("'Title' must not be empty"));
            Assert.That(messages, Has.One.Contain("'Facilitator Passphrase' must not be empty"));
        }

        [Test]
        public void CreatePokerSession_SubmitValidWithBothPassphrases_ShowQrCodeAndLink() {
            // Given
            Page.Navigate(App);

            // When
            Page.SessionTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName.Split("_").LastOrDefault());
            Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");
            Page.ParticipantPassphraseInput.SendKeys("the participator password");

            Page.ScrollDown();
            Page.Submit();

            // Then
            Assert.That(Page.GetUrlShown(), Does.Match(@"http://localhost:\d+/pokertime-session/([A-z0-9]+)/join"));

            Assert.That(Page.FacilitatorInstructions.Text, Contains.Substring("my secret facilitator password"));
            Assert.That(Page.ParticipatorInstructions.Text, Contains.Substring("the participator password"));
        }

        [Test]
        public void CreatePokerSession_SubmitValidWithOnlyFacilitatorPassphrase_ShowQrCodeAndLink() {
            // Given
            Page.Navigate(App);

            // When
            Page.SessionTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName.Split("_").LastOrDefault());
            Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            Page.ScrollDown();
            Page.Submit();

            // Then
            Assert.That(Page.GetUrlShown(), Does.Match(@"http://localhost:\d+/pokertime-session/([A-z0-9]+)/join"));

            Assert.That(Page.FacilitatorInstructions.Text, Contains.Substring("my secret facilitator password"));
            Assert.That(Page.ParticipatorInstructions.Text, Contains.Substring("no password is required"));
        }


    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Dynamically instantiated")]
    public sealed class CreatePokerSessionPage : PageObject {
        public IWebElement SessionTitleInput => WebDriver.FindVisibleElement(By.Id("pokertime-title"));
        public IWebElement FacilitatorPassphraseInput => WebDriver.FindVisibleElement(By.Id("pokertime-facilitator-passphrase"));
        public IWebElement ParticipantPassphraseInput => WebDriver.FindVisibleElement(By.Id("pokertime-passphrase"));
        public IWebElement SubmitButton => WebDriver.FindVisibleElement(By.Id("create-pokertime-button"));
        public IWebElement ModalSubmitButton => WebDriver.FindVisibleElement(By.Id("modal-create-pokertime-button"));

        public IWebElement UrlLocationInput => WebDriver.FindVisibleElement(By.Id("pokertime-location"));
        public IWebElement ParticipatorInstructions => WebDriver.FindElementByTestElementId("participator instructions");
        public IWebElement FacilitatorInstructions => WebDriver.FindElementByTestElementId("facilitator instructions");

        public IWebElement LobbyCreationPassphraseInput => WebDriver.FindVisibleElement(By.Id("pokertime-lobby-creation-passphrase"));
        public IWebElement LobbyCreationPassphraseModal => WebDriver.FindElementByTestElementId("lobby-creation-passphrase-modal");

        public bool LobbyCreationPassphraseModalIsDisplayed => WebDriver.FindElement(By.CssSelector("[data-test-element-id=\"lobby-creation-passphrase-modal\"]")).Displayed;

        public void Navigate(PokerTimeAppFactory app) => WebDriver.NavigateToBlazorPage(app.CreateUri("create-poker-session"));
        public void Submit() => SubmitButton.Click();
        public void ModalSubmit() => ModalSubmitButton.Click();

        public string GetUrlShown() => WebDriver.Retry(_ => UrlLocationInput.GetAttribute("value"));
        public ReadOnlyCollection<IWebElement> GetValidationMessages() => WebDriver.FindElements(By.ClassName("validation-message"));
    }
}
