namespace PokerTime.Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors {
    using System.Collections.Generic;
    using MediatR;

    public class GetAvailablePredefinedParticipantColorsQuery : IRequest<IList<AvailableParticipantColorModel>> {
        public string SessionId { get; }

        public GetAvailablePredefinedParticipantColorsQuery(string sessionId) {
            SessionId = sessionId;
        }

        public override string ToString() => $"[{nameof(GetAvailablePredefinedParticipantColorsQuery)}] {SessionId}";
    }

}
