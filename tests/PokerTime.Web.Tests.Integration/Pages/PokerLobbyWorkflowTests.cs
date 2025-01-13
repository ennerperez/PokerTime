namespace PokerTime.Web.Tests.Integration.Pages {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Services;
    using Common;
    using Domain.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using OpenQA.Selenium;

    [TestFixture]
    public class PokerLobbyWorkflowTests : PokerSessionLobbyTestsBase {
        [SetUp]
        public async Task SetUp() {
            using var scope = App.CreateTestServiceScope();
            SessionId = await scope.CreatePokerSession("scrummaster");
        }

        [Test]
        public async Task PokerLobby_ShowsPlainBoard_OnJoiningNewSession() {
            // Given
            await Task.WhenAll(
                Task.Run(() => Join(Client1, true)),
                Task.Run(() => Join(Client2, false))
            );

            // When
            WaitNavigatedToLobby();

            // Then
            MultiAssert(client => Assert.That(() => client.WaitForStartMessageElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry()));
            MultiAssert(client => Assert.That(() => client.WebDriver.FindElementsByTestElementId("add-note-button"), Has.Count.EqualTo(0).Retry()));
        }

        [Test]
        public async Task PokerLobby_ShowsCards_OnAdvancingSession() {
            // Given
            await Task.WhenAll(
                Task.Run(() => Join(Client1, true)),
                Task.Run(() => Join(Client2, false))
            );
            WaitNavigatedToLobby();

            // When
            Client1.WorkflowContinueButton.Click();

            // Then
            var dbContext = ServiceScope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>();
            var currentSession = await dbContext.Sessions.FindBySessionId(SessionId, CancellationToken.None);
            var dbCardIds = dbContext.Symbols.Where(x => x.SymbolSetId == currentSession.SymbolSetId).Select(x => x.Id).ToList();

            MultiAssert(client => {
                Assert.That(() => client.CardChooserElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                    "The card chooser is not displayed");

                Assert.That(() => {
                    return client.CardChooser.Cards.Select(x => x.Id);
                }, Is.EquivalentTo(dbCardIds).And.Not.Empty.Retry(), "Not all poker cards are shown");

                Assert.That(() => client.CardChooser.Cards.All(x => x.IsEnabled == false),
                    Is.True.Retry(), "Some cards are interactable in this stage, which shouldn't be the case");
            });
        }

        [Test]
        public async Task PokerLobby_ShowsInteractableCards_OnEstimationSession() {
            await SetCurrentUserStory();
            await SetSession(s => s.CurrentStage = SessionStage.Discussion);

            // Given
            await Task.WhenAll(
                Task.Run(() => Join(Client1, true)),
                Task.Run(() => Join(Client2, false))
            );
            WaitNavigatedToLobby();

            // When
            Client1.InvokeContinueWorkflow();

            // Then
            var dbContext = ServiceScope.ServiceProvider.GetRequiredService<IPokerTimeDbContext>();
            var currentSession = await dbContext.Sessions.FindBySessionId(SessionId, CancellationToken.None);
            var dbCardIds = dbContext.Symbols.Where(x => x.SymbolSetId == currentSession.SymbolSetId).Select(x => x.Id).ToList();

            MultiAssert(client => {
                Assert.That(() => client.CardChooserElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                    "The card chooser is not displayed");

                Assert.That(() => client.EstimationOverviewElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                    "The estimation overview is not displayed");

                Assert.That(() => {
                    return client.CardChooser.Cards.Select(x => x.Id);
                }, Is.EquivalentTo(dbCardIds).And.Not.Empty.Retry(), "Not all poker cards are shown");

                Assert.That(() => client.CardChooser.Cards.All(x => x.IsEnabled == true),
                    Is.True.Retry(), "Some cards are not interactable in this stage, all should be interactable");
            });
        }

        [Test]
        public async Task PokerLobby_CardEstimation_UpdatesOtherView() {
            await SetCurrentUserStory();
            await SetSession(s => s.CurrentStage = SessionStage.Estimation);

            // Given
            await Task.WhenAll(
                Task.Run(() => Join(Client1, true)),
                Task.Run(() => Join(Client2, false))
            );

            WaitNavigatedToLobby();

            MultiAssert(client => {
                Assume.That(() => client.EstimationOverviewElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                    "The estimation overview is not displayed");
            });

            // When
            var cards = Client2.CardChooser.Cards.ToList();
            var randomCard = cards[TestContext.CurrentContext.Random.Next(cards.Count)];

            var cardId = randomCard.Id;
            TestContext.Write($"Choosing card with symbol #{cardId}");
            randomCard.Click();

            // Then
            MultiAssert(client => {
                Assert.That(() => client.EstimationOverview.Cards.Select(c => c.SymbolId), Is.EquivalentTo(new[] { cardId }),
                    $"Expected the card with ID '{cardId}' to be come visible in the estimation overview");
            });
        }

        [Test]
        public async Task PokerLobby_JoiningWithInProgress_UpdatesUnestimatedCardSection() {
            await SetCurrentUserStory();
            await SetSession(s => s.CurrentStage = SessionStage.Estimation);

            // Given
            var client1Name = Name.Create();

            Join(Client1, true, client1Name);
            WaitNavigatedToLobby(Client1);

            Assume.That(() => Client1.EstimationOverviewElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                "The estimation overview is not displayed");
            Assume.That(() => Client1.EstimationOverview.UnestimatedCards, Contains.Item(client1Name).Retry(),
                "Expected unestimated cards to include the client 1 name");

            // When
            var client2Name = Name.Create();

            Join(Client2, false, client2Name);
            WaitNavigatedToLobby(Client2);

            // Then
            MultiAssert(client => {
                Assert.That(() => client.EstimationOverview.UnestimatedCards, Contains.Item(client2Name).Retry(),
                    $"Expected the unestimated cards now to include the just joined client '{client2Name}'");
            });
        }

        [Test]
        public async Task PokerLobby_CardEstimationDiscussion_CardsBecomeNonChoosable() {
            await SetCurrentUserStory();
            await SetSession(s => s.CurrentStage = SessionStage.Estimation);

            // Given
            await Task.WhenAll(
                Task.Run(() => Join(Client1, true)),
                Task.Run(() => Join(Client2, false))
            );

            MultiAssert(client => {
                Assume.That(() => client.CardChooserElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                    "The card chooser is not displayed");
            });

            // When
            Client1.WorkflowContinueButton.Click();

            MultiAssert(client => {
                Assert.That(() => client.CardChooser.Cards.All(x => x.IsEnabled == false),
                    Is.True.Retry(), "Some cards are interactable in this stage, which shouldn't be the case");
            });
        }

        [Test]
        public async Task PokerLobby_CardFinished_EndMessageBecomesVisible() {
            await SetCurrentUserStory();
            await SetSession(s => s.CurrentStage = SessionStage.EstimationDiscussion);

            // Given
            await Task.WhenAll(
                Task.Run(() => Join(Client1, true)),
                Task.Run(() => Join(Client2, false))
            );

            // When
            Client1.WorkflowEndButton.Click();

            MultiAssert(client => {
                Assume.That(() => client.SessionFinishedElement, Has.Property(nameof(IWebElement.Displayed)).EqualTo(true).Retry(),
                    "The end message is not displayed");
            });
        }
    }
}
