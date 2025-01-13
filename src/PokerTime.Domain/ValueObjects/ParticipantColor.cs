namespace PokerTime.Domain.ValueObjects {
    using System.Collections.Generic;
    using System.Drawing;
    using Common;

    public class ParticipantColor : ValueObject {
        public ParticipantColor() {
        }

        public ParticipantColor(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static implicit operator ParticipantColor(Color color) => FromColor(color);
        public static ParticipantColor FromColor(Color color) => new ParticipantColor(color.R, color.G, color.B);

        public string ToHex() => $"{R:X2}{G:X2}{B:X2}";

        protected override IEnumerable<object> GetAtomicValues() {
            yield return R;
            yield return G;
            yield return B;
        }
    }
}
