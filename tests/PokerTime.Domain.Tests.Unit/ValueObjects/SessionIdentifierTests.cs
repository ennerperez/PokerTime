namespace PokerTime.Domain.Tests.Unit.ValueObjects {
    using System.Collections.Generic;
    using Domain.Services;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public sealed class SessionIdentifierTests {
        private readonly ISessionIdentifierService _sessionIdentifierService = new SessionIdentifierService();

        [Test]
        [Retry(1)]
        public void SessionIdentifier_CreateNew_ReturnsRandomId() {
            // Given
            var generatedIds = new HashSet<string>(1000);

            // When / then
            for (var count = 1000; count > 0; count--) {
                var identifier = _sessionIdentifierService.CreateNew();

                Assert.That(generatedIds.Add(identifier.StringId), Is.True, $"Non-unique identifier created: {identifier}");
            }
        }

        [Test]
        [Repeat(100)]
        public void SessionIdentifier_CreateNew_CreatesValidId() {
            // Given
            var sessionIdentifier = _sessionIdentifierService.CreateNew();

            // When
            var isValid = _sessionIdentifierService.IsValid(sessionIdentifier.StringId);

            // Then
            ClassicAssert.IsTrue(isValid, $"Id {sessionIdentifier} is not valid");
        }


        [Test]
        [Repeat(100)]
        public void SessionIdentifier_CreateNew_CreatesIdOfLengthLessThanOrEqualTo32() {
            // Given / when
            var sessionIdentifier = _sessionIdentifierService.CreateNew();

            // Then
            Assert.That(sessionIdentifier.StringId, Has.Length.LessThanOrEqualTo(32), $"Id {sessionIdentifier} is not valid");
        }
    }
}
