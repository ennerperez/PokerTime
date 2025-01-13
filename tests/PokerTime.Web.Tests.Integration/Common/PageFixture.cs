namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [RetryTest(3)]
    public abstract class PageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {

        protected TPageObject Page { get; private set; }

        public override void OnInitialized()
        {
            Page = App.CreatePageObject<TPageObject>();
        }
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Page?.Dispose();
            }
        }

        [TearDown]
        public void CreateScreenshots() {
            Page?.WebDriver?.TryCreateScreenshot("Client_AfterTest");
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    [RetryTest(3)]
    public abstract class TwoClientPageFixture<TPageObject> : ScopedFixture, IDisposable where TPageObject : IPageObject, new() {
        protected TPageObject Client1 { get; private set; }
        protected TPageObject Client2 { get; private set; }

        public override void OnInitialized() {
            Client1 = App.CreatePageObject<TPageObject>();
            Client2 = App.CreatePageObject<TPageObject>();
        }

        [TearDown]
        public void CreateScreenshots() {
            Client1?.WebDriver?.TryCreateScreenshot("Client1_AfterTest");
            Client2?.WebDriver?.TryCreateScreenshot("Client1_AfterTest");
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Client1?.Dispose();
                Client2?.Dispose();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void MultiAssert(Action<TPageObject> action) =>
            Task.WaitAll(
                Task.Run(() => action.Invoke(Client1)),
                Task.Run(() => action.Invoke(Client2))
            );
    }

    [UseRunningApp]
    [CleanupDisposables]
    public abstract class ScopedFixture : IAppFixture {
        public PokerTimeAppFactory App { get; set; }
        public virtual void OnInitialized() { }

        protected IServiceScope ServiceScope { get; private set; }

        [SetUp] public void SetUpServiceScope() => ServiceScope = App.Services.CreateScope();

        [TearDown]
        public void KillServiceScope() {
            ServiceScope?.Dispose();
            ServiceScope = null;
        }
    }
}
