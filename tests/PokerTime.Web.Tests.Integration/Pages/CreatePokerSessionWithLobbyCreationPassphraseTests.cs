namespace PokerTime.Web.Tests.Integration.Pages {
    using System;
    using System.Globalization;
    using System.Linq;
    using Application.Common.Settings;
    using Common;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public sealed class CreatePokerSessionWithLobbyCreationPassphraseTests : PageFixture<CreatePokerSessionPage> {
        private static readonly string SecurityPassword = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        private TemporarySettingsScope<SecuritySettings> _temporarySettingsScope;

        [SetUp]
        public void SetSecuritySettings() {
            _temporarySettingsScope = new TemporarySettingsScope<SecuritySettings>(App);
            _temporarySettingsScope.SaveSettings(securitySettings => {
                securitySettings.LobbyCreationPassphrase = SecurityPassword;
            });
        }

        [TearDown]
        public void ResetSecuritySettings() => _temporarySettingsScope.RestoreSettings();

        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSession_SubmitValid_ShowDialog() {
            // Given
            Page.Navigate(App);

            // When
            Page.SessionTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName.Split("_").LastOrDefault());
            Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            Page.ScrollDown();
            Page.Submit();

            // Then
            Assert.That(Page.LobbyCreationPassphraseModal, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                "Expected modal for the passphrase to become visible");
        }

        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSession_PasswordDialog_InvalidPassphrase_ShowErrorInsideDialog() {
            // Given
            Page.Navigate(App);

            // When
            Page.SessionTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName.Split("_").LastOrDefault());
            Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            Page.ScrollDown();
            Page.Submit();
            EnsurePasswordDialogVisible();

            Page.LobbyCreationPassphraseInput.SendKeys("invalid password");
            Page.ModalSubmit();
            Page.ScrollDown();

            // Then
            var messages = new DefaultWait<CreatePokerSessionPage>(Page)
                .Until(p => {
                    var collection = p.GetValidationMessages();
                    if (collection.Count == 0) return null;
                    return collection;
                })
                .Select(el => el.Text)
                .ToArray();

            Assert.That(messages, Has.One.Contains("Invalid pre-shared passphrase entered needed for creating a session"));

            // Assert.That(() => this.Page.LobbyCreationPassphraseModal.Displayed, Is.True.Retry(),
            //     "Expected the modal to become and stay visible because the validation error is shown inside the modal");
        }

        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSessionOnSubmitValidPassphrase_CreateSession() {
            // Given
            Page.Navigate(App);

            // When
            Page.SessionTitleInput.SendKeys(TestContext.CurrentContext.Test.FullName.Split("_").LastOrDefault());
            Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            Page.ScrollDown();
            Page.Submit();
            EnsurePasswordDialogVisible();

            Page.LobbyCreationPassphraseInput.SendKeys(SecurityPassword);
            Page.ModalSubmit();

            // Then
            Assert.That(Page.GetUrlShown(), Does.Match(@"http://localhost:\d+/pokertime-session/([A-z0-9]+)/join"));
        }
        [Test]
        public void LobbyCreationPassphraseActive_CreatePokerSessionOnSubmitWithValidPassphraseWithFormErrors_HidesModal() {
            // Given
            Page.Navigate(App);

            // When
            // ... (don't enter a title, which is a required field)
            Page.FacilitatorPassphraseInput.SendKeys("my secret facilitator password");

            Page.ScrollDown();
            Page.Submit();
            EnsurePasswordDialogVisible();

            Page.LobbyCreationPassphraseInput.SendKeys(SecurityPassword);
            Page.ModalSubmit();

            // Then
            Assert.That(() => Page.LobbyCreationPassphraseModalIsDisplayed, Is.False.Retry(),
                "Expected the modal to become hidden because the error is on the form itself");
        }

        private void EnsurePasswordDialogVisible() =>
            Assume.That(Page.LobbyCreationPassphraseModal,
                Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                "Expected modal for the passphrase to become visible");
    }
}
