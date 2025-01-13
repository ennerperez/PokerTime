namespace PokerTime.Web.Tests.Integration.Pages {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Application.Sessions.Commands.CreatePokerSession;
    using Application.Sessions.Queries.GetParticipantsInfo;
    using Common;
    using Components;
    using Domain.Entities;
    using Domain.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    [TestFixture]
    public sealed class JoinPokerSessionTests : PageFixture<JoinPokerSessionPage> {
        [Test]
        public void JoinPokerSessionPage_UnknownSession_ShowNotFoundMessage() {
            // Given
            var sessionIdentifier = new SessionIdentifierService().CreateNew().StringId;

            // When
            Page.Navigate(App, sessionIdentifier);

            // Then
            Assert.That(() => Page.WebDriver.FindElements(By.CssSelector(".alert.alert-danger")), Has.Count.EqualTo(1).Retry());
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSession_FormShownWithValidation() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            Page.Navigate(App, sessionId);

            // When
            Page.ScrollDown();
            Page.Submit();

            // Then
            var messages = new DefaultWait<JoinPokerSessionPage>(Page)
                .Until(p => {
                    var collection = p.GetValidationMessages();
                    if (collection.Count == 0) return null;
                    return collection;
                })
                .Select(el => el.Text)
                .ToArray();

            Assert.That(messages, Has.One.Contain("'Name' must not be empty"));
            Assert.That(messages, Has.One.Contain("This passphrase is not valid. Please try again"));
            Assert.That(messages, Has.One.Contain("Please select a color"));
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSessionAlreadyStarted_ShowMessage() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            await SetSession(sessionId, retro => retro.CurrentStage = SessionStage.Discussion);

            // When
            Page.Navigate(App, sessionId);

            // Then
            Assert.That(() => Page.WebDriver.FindElements(By.CssSelector(".alert.alert-info")), Has.Count.EqualTo(1).Retry());
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSessionFinished_ShowMessage() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            await SetSession(sessionId, retro => retro.CurrentStage = SessionStage.Finished);

            // When
            Page.Navigate(App, sessionId);

            // Then
            Assert.That(() => Page.WebDriver.FindElements(By.CssSelector(".alert.alert-warning")), Has.Count.EqualTo(1).Retry());
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSession_ValidatesParticipantPassphaseAndRedirectsToLobby() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            var myName = Name.Create();
            Page.Navigate(App, sessionId);

            // When
            Page.NameInput.SendKeys(myName);
            new SelectElement(Page.ColorSelect).SelectByIndex(1);
            Page.ParticipantPassphraseInput.SendKeys("secret");
            Page.ScrollDown();
            Page.Submit();

            // Then
            Assert.That(() => Page.WebDriver.Url, Does.Match("/pokertime-session/" + sessionId + "/lobby").Retry());
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSession_JoinParticipantUpdatesParticipantListInRealtime() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            var myName = Name.Create();
            Page.Navigate(App, sessionId);

            var secondInstance = App.CreatePageObject<JoinPokerSessionPage>().RegisterAsTestDisposable();
            secondInstance.Navigate(App, sessionId);

            // When
            Page.NameInput.SendKeys(myName);
            new SelectElement(Page.ColorSelect).SelectByIndex(1);
            Page.ParticipantPassphraseInput.SendKeys("secret");
            Page.ScrollDown();
            Page.Submit();

            // Then
            Assert.That(() => secondInstance.OnlineList.OnlineListItems.Select(x => x.Text), Has.One.Contains(myName));
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSession_JoinParticipantUpdatesColorListInRealtime() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            var myName = Name.Create();
            Page.Navigate(App, sessionId);

            var secondInstance = App.CreatePageObject<JoinPokerSessionPage>().RegisterAsTestDisposable();
            secondInstance.Navigate(App, sessionId);

            IList<AvailableParticipantColorModel> availableColors;
            {
                using var scope = App.CreateTestServiceScope();
                scope.SetNoAuthenticationInfo();
                availableColors = await scope.Send(new GetAvailablePredefinedParticipantColorsQuery(sessionId));
            }
            var colorToSelect = availableColors[TestContext.CurrentContext.Random.Next(0, availableColors.Count)];

            // When
            var selectList = new SelectElement(Page.ColorSelect);
            //Assert.That(() => selectList.Options.Select(x => x.GetProperty("value")).Where(x => !String.IsNullOrEmpty(x)), Is.EquivalentTo(availableColors.Select(x => "#" + x.HexString)).Retry(),
            //    "Cannot find all available colors in the selection list");
            selectList.SelectByValue("#" + colorToSelect.HexString);

            Page.NameInput.SendKeys(myName);
            Page.ParticipantPassphraseInput.SendKeys("secret");
            Page.ScrollDown();
            Page.Submit();

            // Then
            Assert.That(() => new SelectElement(secondInstance.ColorSelect).Options.Select(x => x.GetAttribute("value")), Does.Not.Contains("#" + colorToSelect.HexString).And.Not.EquivalentTo(availableColors.Select(x => "#" + x.HexString)).Retry());
        }

        [Test]
        public async Task JoinPokerSessionPage_KnownSession_JoinAsFacilitatorUpdatesParticipantListInRealtime() {
            // Given
            var sessionId = await CreatePokerSession("scrummaster", "secret");
            var myName = Name.Create();
            Page.Navigate(App, sessionId);

            var secondInstance = App.CreatePageObject<JoinPokerSessionPage>().RegisterAsTestDisposable();
            secondInstance.Navigate(App, sessionId);

            // When
            Page.NameInput.SendKeys(myName);
            new SelectElement(Page.ColorSelect).SelectByIndex(2);
            Page.IsFacilitatorCheckbox.Click();
            Page.WebDriver.Retry(_ => {
                Page.FacilitatorPassphraseInput.SendKeys("scrummaster");
                return true;
            });
            Page.ScrollDown();
            Page.Submit();

            Thread.Sleep(500);

            using var scope = App.CreateTestServiceScope();
            scope.SetNoAuthenticationInfo();
            var participants = await scope.Send(new GetParticipantsInfoQuery(sessionId));
            var facilitator = participants.Participants.First(x => x.Name == myName);

            // Then
            Assert.That(() => secondInstance.OnlineList.OnlineListItems.Select(x => x.Text), Has.One.Contains(myName));
            Assert.That(() => secondInstance.OnlineList.GetListItem(facilitator.Id).FindElements(By.ClassName("fa-crown")), Is.Not.Empty.Retry());
        }

        private Task SetSession(string sessionId, Action<Session> action) {
            using var scope = App.CreateTestServiceScope();
            return scope.SetSession(sessionId, action);
        }
        private async Task<string> CreatePokerSession(string facilitatorPassword, string password) {
            var command = new CreatePokerSessionCommand {
                Title = TestContext.CurrentContext.Test.FullName.Split("_").LastOrDefault(),
                FacilitatorPassphrase = facilitatorPassword,
                Passphrase = password,
                SymbolSetId = (await ServiceScope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>().SymbolSets.FirstAsync()).Id
            };

            ServiceScope.SetNoAuthenticationInfo();

            var result = await ServiceScope.Send(command);
            return result.Identifier.StringId;
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Dynamically instantiated")]
    public sealed class JoinPokerSessionPage : PageObject {
        public IWebElement Title => WebDriver.FindElement(By.CssSelector("h1.title"));
        public IWebElement NameInput => WebDriver.FindElement(By.Id("pokertime-name"));
        public IWebElement FacilitatorPassphraseInput => WebDriver.FindElement(By.Id("pokertime-facilitator-passphrase"));
        public IWebElement ParticipantPassphraseInput => WebDriver.FindElement(By.Id("pokertime-passphrase"));
        public IWebElement ColorInput => WebDriver.FindElement(By.Id("pokertime-color"));
        public IWebElement ColorSelect => WebDriver.FindElement(By.Id("pokertime-color-choices"));
        public IWebElement IsFacilitatorCheckbox => WebDriver.FindElement(By.Id("pokertime-is-facilitator"));
        public IWebElement SubmitButton => WebDriver.FindVisibleElement(By.Id("join-pokertime-button"));
        public void Submit() => SubmitButton.Click();

        public ReadOnlyCollection<IWebElement> GetValidationMessages() => WebDriver.FindElements(By.ClassName("validation-message"));
        public void Navigate(PokerTimeAppFactory app, string sessionId) => WebDriver.NavigateToBlazorPage(app.CreateUri($"pokertime-session/{sessionId}/join"));

        public PokerOnlineListComponent OnlineList => new PokerOnlineListComponent(WebDriver);
    }
}
