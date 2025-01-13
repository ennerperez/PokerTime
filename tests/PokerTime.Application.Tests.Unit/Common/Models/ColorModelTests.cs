namespace PokerTime.Application.Tests.Unit.Common.Models {
    using System.Drawing;
    using Application.Common.Models;
    using NUnit.Framework;

    [TestFixture]
    public static class ColorModelTests {
        [Test]
        public static void ColorModel_HasSameColors_ReturnsTrue() {
            // Given
            var derivedColor = new DerivedColorModel(Color.BlueViolet);
            var color = new ColorModel { R = derivedColor.R, B = derivedColor.B, G = derivedColor.G };

            // When
            var result = color.HasSameColors(derivedColor);

            // Then
            Assert.That(result, Is.True);
        }

        [Test]
        public static void ColorModel_HasSameColors_ReturnsFalse() {
            // Given
            var derivedColor = new DerivedColorModel(Color.BlueViolet);
            var color = new ColorModel { R = derivedColor.R, B = derivedColor.B, G = derivedColor.G };

            derivedColor.G = 0;

            // When
            var result = color.HasSameColors(derivedColor);

            // Then
            Assert.That(result, Is.False);
        }

        private sealed class DerivedColorModel : ColorModel {
            public string KnownName { get; }

            public DerivedColorModel(Color color) {
                KnownName = color.Name;
                R = color.R;
                G = color.G;
                B = color.B;
            }
        }
    }
}
