namespace PokerTime.Application.Poker.Commands {
    using MediatR;

    public sealed class PlayCardCommand : IRequest {
        public string SessionId { get; }

        public int UserStoryId { get; }

        public int SymbolId { get; }

        public PlayCardCommand(string sessionId, int userStoryId, int symbolId) {
            SessionId = sessionId;
            UserStoryId = userStoryId;
            SymbolId = symbolId;
        }
    }
}
