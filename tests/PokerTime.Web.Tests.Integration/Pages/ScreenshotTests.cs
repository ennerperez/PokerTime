namespace PokerTime.Web.Tests.Integration.Pages {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Services;
    using Common;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.Extensions;

    /// <summary>
    /// Not really a real test, but more because I'm tired of creating screenshots. Note these tests don't really
    /// run independently, and they are ordered by the [Order] attribute.
    /// </summary>
    /// <remarks>
    /// Client1: Roger (facilitator)
    /// Client2: Hong (participant)
    /// </remarks>
    [TestFixture]
    public sealed class ScreenshotTests : PokerSessionLobbyTestsBase {
        [TearDown]
        public void SkipOnAppVeyor() {
            if (TestContext.CurrentContext.Result.Outcome != ResultState.Success &&
                string.Equals(Environment.GetEnvironmentVariable("APPVEYOR"), bool.TrueString, StringComparison.OrdinalIgnoreCase)) {
                throw new IgnoreException("AppVeyor is too slow to run this test fixture - this test is skipped on AppVeyor");
            }
        }

        [Test]
        [Order((int)SessionStage.NotStarted)]
        public void Screenshot_CreateSession() {
            // Given
            using var createRetroPage = new CreatePokerSessionPage();
            createRetroPage.InitializeFrom(Client1);
            createRetroPage.Navigate(App);

            // void SetResolution(IWebDriver webDriver) {
            //     webDriver.Manage().Window.Size = new Size(1450, 1024);
            // }
            //
            // SetResolution(this.Client1.WebDriver);
            // SetResolution(this.Client2.WebDriver);

            // When
            createRetroPage.SessionTitleInput.SendKeys("Sprint 1: Initial prototype");
            createRetroPage.FacilitatorPassphraseInput.SendKeys("scrummaster");
            createRetroPage.ParticipantPassphraseInput.SendKeys("secret");
            createRetroPage.WebDriver.TryCreateScreenshot();

            // Then
            CreateDocScreenshot(createRetroPage.WebDriver, "create-session");

            createRetroPage.ScrollDown();
            createRetroPage.Submit();

            var url = createRetroPage.GetUrlShown();
            var retroId = Regex.Match(url, "/pokertime-session/(?<sessionId>[A-z0-9]+)/join", RegexOptions.IgnoreCase).
                Groups["sessionId"].
                Value;
            SessionId = retroId;
        }

        [Test]
        [Order((int)SessionStage.Discussion)]
        public async Task Screenshot_Discussion() {
            EnsureSessionInStage(SessionStage.NotStarted);

            // Given
            using (var scope = App.CreateTestServiceScope()) {
                await scope.SetSession(SessionId, r => r.HashedPassphrase = null);
            }

            Join(Client1, true, "Roger", colorName: "Driver", submitCallback: () => CreateDocScreenshot(Client1.WebDriver, "join-poker-session"));
            Join(Client2, false, "Hong", colorName: "green");

            WaitNavigatedToLobby();

            Client1.SetUserStoryTitle("9345: As a user I want to be able to log in through SSO");
            CreateDocScreenshot(Client1.WebDriver, "prepare-discussion");

            // Then
            Client1.InvokeContinueWorkflow();

            CreateDocScreenshot(Client1.WebDriver, "discussion");

            Client1.InvokeContinueWorkflow();
        }

        [Test]
        [Order((int)SessionStage.Estimation)]
        public async Task Screenshot_Estimation() {
            EnsureSessionInStage(SessionStage.Estimation);

            // When
            using (var scope = App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(SessionId).
                    HasExistingParticipant("Roger").
                    HasExistingParticipant("Hong").
                    WithParticipant("Aaron", false).
                    WithParticipant("Ashley", false).
                    WithParticipant("Josh", false).
                    WithParticipant("Patrick", false).
                    WithParticipant("Sarah", false).
                    PlayCard("Aaron", "3").
                    PlayCard("Josh", "2").
                    PlayCard("Patrick", "8").
                    PlayCard("Sarah", "☕").
                    Build();
            }

            void PlayCard(PokerSessionLobby client, string symbolText) {
                var card = client.CardChooser.Cards.FirstOrDefault(x => x.SymbolText == symbolText);

                if (card == null) {
                    Assert.Fail($"Unable to find card with symbol '{symbolText}'");
                }

                card.Click();
            }

            PlayCard(Client1, "?");
            PlayCard(Client2, "5");

            CreateDocScreenshot(Client2.WebDriver, "estimation");

            // Then
            Client1.InvokeContinueWorkflow();
        }

        [Test]
        [Order((int)SessionStage.EstimationDiscussion)]
        public void Screenshot_EstimationDiscussion() {
            EnsureSessionInStage(SessionStage.EstimationDiscussion);

            // When
            CreateDocScreenshot(Client1.WebDriver, "estimation-discussion");
        }

        [Test]
        [Order((int)SessionStage.Finished)]
        public async Task Screenshot_Finished() {
            // Play some rounds
            using (var scope = App.CreateTestServiceScope()) {
                await scope.TestCaseBuilder(SessionId).
                    HasExistingParticipant("Roger").
                    HasExistingParticipant("Hong").
                    HasExistingParticipant("Aaron").
                    HasExistingParticipant("Josh").
                    HasExistingParticipant("Patrick").
                    HasExistingParticipant("Sarah").
                    NewRound("9346: As a user I want to change my password").
                    PlayCard("Roger", "5").
                    PlayCard("Hong", "3").
                    PlayCard("Aaron", "3").
                    PlayCard("Josh", "2").
                    PlayCard("Patrick", "8").
                    PlayCard("Sarah", "☕").
                    CloseEstimationPhase().
                    NewRound("9347: As a user I want to use two-factor authentication").
                    PlayCard("Roger", "8").
                    PlayCard("Hong", "8").
                    PlayCard("Aaron", "13").
                    PlayCard("Josh", "13").
                    PlayCard("Patrick", "?").
                    PlayCard("Sarah", "20").
                    CloseEstimationPhase().
                    NewRound("9348: As a user I want to change my e-mail address").
                    PlayCard("Roger", "3").
                    PlayCard("Hong", "2").
                    PlayCard("Aaron", "2").
                    PlayCard("Josh", "5").
                    PlayCard("Patrick", "2").
                    PlayCard("Sarah", "5").
                    CloseEstimationPhase().
                    NewRound("9347: As a user I want to reset my recovery code").
                    PlayCard("Roger", "8").
                    PlayCard("Hong", "8").
                    PlayCard("Aaron", "20").
                    PlayCard("Josh", "5").
                    PlayCard("Patrick", "5").
                    PlayCard("Sarah", "8").
                    CloseEstimationPhase().
                    Build();
            }

            // When
            Client1.InvokeEndWorkflow();
            EnsureSessionInStage(SessionStage.Finished);

            // Then
            CreateDocScreenshot(Client1.WebDriver, "finished");
        }

        private static void CreateDocScreenshot(IWebDriver webDriver, string name) {
            if (webDriver == null) throw new ArgumentNullException(nameof(webDriver));

            // Scroll to top, set cursor / focus to 0,0
            Thread.Sleep(1000);
            webDriver.ExecuteJavaScript("window.scrollTo(0, 0)");
            //new Actions(webDriver).MoveToElement(webDriver.FindElement(By.ClassName("navbar-menu")), 0, 0, MoveToElementOffsetOrigin.Center).Click().Perform();
            Thread.Sleep(1000);

            // Create a path
            var docStagingDirectory = Path.Combine(Paths.TestArtifactDir, "doc-staging");
            Directory.CreateDirectory(docStagingDirectory);

            var fileName = Path.Combine(docStagingDirectory, name + ".png");

            TestContext.WriteLine($"Creating doc screenshot: {fileName}");
            webDriver.TakeScreenshot().SaveAsFile(fileName);
        }

        private void EnsureSessionInStage(SessionStage sessionStage) {
            using var scope = App.CreateTestServiceScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>();
            Assume.That(() => dbContext.Sessions.AsNoTracking().FindBySessionId(SessionId, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult(),
                Has.Property(nameof(Session.CurrentStage)).EqualTo(sessionStage).Retry(),
                $"Session {SessionId} is not in stage {sessionStage} required for this test. Are the tests running in the correct order?");
        }
    }
}
