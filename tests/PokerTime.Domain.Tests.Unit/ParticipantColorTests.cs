namespace PokerTime.Domain.Tests.Unit {
    using System.Drawing;
    using Domain.ValueObjects;
    using NUnit.Framework;

    [TestFixture]
    public static class ParticipantColorTests {
        [Test]
        public static void ParticipantColor_FromColor_AssignsComponentsCorrectly() {
            // Given
            var source = Color.BlueViolet;

            // When
            ParticipantColor target = source;

            // Then
            Assert.That(target.R, Is.EqualTo(source.R));
            Assert.That(target.G, Is.EqualTo(source.G));
            Assert.That(target.B, Is.EqualTo(source.B));
        }

        [Test]
        public static void ParticipantColor_ToHex_FormatsCorrectly() {
            // Given
            var source = Color.BlueViolet;
            ParticipantColor target = source;

            // When
            var hexStr = target.ToHex();

            // Then
            Assert.That(hexStr, Is.EqualTo("8A2BE2"));
        }
    }
}
