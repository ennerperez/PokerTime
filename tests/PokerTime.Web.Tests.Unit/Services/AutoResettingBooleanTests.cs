namespace PokerTime.Web.Tests.Unit.Services {
    using NUnit.Framework;
    using Web.Services;

    [TestFixture]
    public sealed class AutoResettingBooleanTests {
        [Test]
        public void AutoResettingBoolean_NoIntervention_GetsInitialValue() {
            // Given
            const bool InitialValue = true;
            var resettingBoolean = new AutoResettingBoolean(InitialValue);

            // When / then
            for (var i = 0; i < 10; i++) {
                Assert.That(resettingBoolean.GetValue, Is.EqualTo(InitialValue));
            }
        }

        [Test]
        public void AutoResettingBoolean_WhenSet_ResetsToInitialValue() {
            // Given
            const bool InitialValue = true;
            var resettingBoolean = new AutoResettingBoolean(InitialValue);

            // When
            resettingBoolean.Set();
            var first = resettingBoolean.GetValue();
            var second = resettingBoolean.GetValue();

            // Then
            Assert.That(first, Is.False);
            Assert.That(second, Is.True);
        }
    }
}
