// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
//
//  File:           : Participant.cs
//  Project         : PokerTime.Domain
// ******************************************************************************

namespace PokerTime.Domain.Entities {
    using Abstractions;
    using ValueObjects;


    public class Participant : IIdPrimaryKey {
        public int Id { get; set; }

        public ParticipantColor Color { get; set; }

        public Session Session { get; set; }

        public string Name { get; set; }

        public bool IsFacilitator { get; set; }
    }
}
