namespace PokerTime.Application.Sessions.Commands.CreatePokerSession {
    using MediatR;

    public class CreatePokerSessionCommand : IRequest<CreatePokerSessionCommandResponse> {

        public string Title { get; set; }

        public string FacilitatorPassphrase { get; set; }

        public int SymbolSetId { get; set; }

        public string Passphrase { get; set; }

        public string LobbyCreationPassphrase { get; set; }
    }
}
