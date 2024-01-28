namespace PokerTime.Application.Common.Models {
    using System;

    public readonly struct CurrentParticipantModel : IEquatable<CurrentParticipantModel> {
        public int Id { get; }
        public string Name { get; }
        public string HexColorString { get; }
        public bool IsFacilitator { get; }
        public bool IsAuthenticated => Id != 0;

        public CurrentParticipantModel(int id, string name, string color, bool isFacilitator) {
            Id = id;
            Name = name;
            HexColorString = color;
            IsFacilitator = isFacilitator;
        }

        public bool Equals(CurrentParticipantModel other) => Id == other.Id;

        public override bool Equals(object obj) => obj is CurrentParticipantModel other && Equals(other);

        public override int GetHashCode() => Id;

        public static bool operator ==(CurrentParticipantModel left, CurrentParticipantModel right) => left.Equals(right);

        public static bool operator !=(CurrentParticipantModel left, CurrentParticipantModel right) => !left.Equals(right);

        public void Deconstruct(out int participantId, out string name, out string color, out bool isFacilitator) {
            participantId = Id;
            name = Name;
            color = HexColorString;
            isFacilitator = IsFacilitator;
        }

        public override string ToString() => $"[{Id}|{(IsFacilitator ? "M" : "P")}|{Name}]";
    }
}
