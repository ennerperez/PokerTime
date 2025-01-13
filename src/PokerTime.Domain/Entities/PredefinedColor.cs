namespace PokerTime.Domain.Entities {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using ValueObjects;

    public class PredefinedParticipantColor {
        public PredefinedParticipantColor(string name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Color = new ParticipantColor();
        }

        public PredefinedParticipantColor(string name, ParticipantColor participantColor) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Color = participantColor;
        }

        public int Id { get; set; }

        public string Name { get; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public ParticipantColor Color { get; set; }
    }
}
