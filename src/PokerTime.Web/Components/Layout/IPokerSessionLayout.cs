namespace PokerTime.Web.Components.Layout {
    using System;
    using Domain.Entities;

    public interface IPokerSessionLayout {
        void Update(in PokerSessionLayoutInfo layoutInfo);
    }

    public readonly struct PokerSessionLayoutInfo : IEquatable<PokerSessionLayoutInfo> {
        public SessionStage? Stage { get; }

        public string Title { get; }

        public PokerSessionLayoutInfo(string title) : this() {
            Title = title;
        }

        public PokerSessionLayoutInfo(string title, SessionStage? stage) {
            Stage = stage;
            Title = title;
        }

        public bool Equals(PokerSessionLayoutInfo other) => Stage == other.Stage && Title == other.Title;

        public override bool Equals(object obj) => obj is PokerSessionLayoutInfo other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                return (Stage.GetHashCode() * 397) ^ (Title != null ? Title.GetHashCode(StringComparison.InvariantCulture) : 0);
            }
        }

        public static bool operator ==(PokerSessionLayoutInfo left, PokerSessionLayoutInfo right) => left.Equals(right);

        public static bool operator !=(PokerSessionLayoutInfo left, PokerSessionLayoutInfo right) => !left.Equals(right);
    }
}
