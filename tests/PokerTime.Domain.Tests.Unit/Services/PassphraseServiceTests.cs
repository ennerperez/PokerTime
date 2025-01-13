namespace PokerTime.Domain.Tests.Unit.Services {
    using Domain.Services;
    using NUnit.Framework;

    [TestFixture]
    public sealed class PassphraseServiceTests {
        private readonly IPassphraseService _passphraseService = new PassphraseService();

        [Test]
        public void PassphraseService_NullArgument_ThrowsArgumentNullException() {
            // Given
            string passphrase = null;

            // When
            TestDelegate action = () => _passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(action, Throws.ArgumentNullException);
        }

        [Test]
        public void PassphraseService_EmptyArgument_ThrowsArgumentException() {
            // Given
            var passphrase = string.Empty;

            // When
            TestDelegate action = () => _passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(action, Throws.ArgumentException);
        }

        [Test]
        public void PassphraseService_Passphrase_Creates64LengthString() {
            // Given
            var passphrase = "test";

            // When
            var hashed = _passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(hashed, Has.Length.EqualTo(64));
        }

        [Test]
        [Repeat(10)]
        public void PassphraseService_Passphrase_CreatesValidPassphrase() {
            // Given
            var passphrase = TestContext.CurrentContext.Random.NextGuid() + "_" + TestContext.CurrentContext.Random.NextGuid();

            // When
            var hashed = _passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(hashed, Is.Not.EqualTo(passphrase));
            Assert.That(_passphraseService.ValidatePassphrase(passphrase, hashed), Is.True, $"Unable to validate passphrase [{passphrase}]");
        }
    }
}
