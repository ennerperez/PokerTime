namespace PokerTime.Application.Common.Settings {

    public sealed class SecuritySettings {
        /// <summary>
        /// Passphrase for creating lobby's - set on server side to prevent just anyone from hosting poker sessions
        /// </summary>
        public string LobbyCreationPassphrase { get; set; }

        public bool LobbyCreationNeedsPassphrase => !string.IsNullOrEmpty(LobbyCreationPassphrase);

        /// <summary>
        /// Enable detection of X-HTTP-Forwarded-For HTTP headers
        /// </summary>
        public bool EnableProxyMode { get; set; }
    }
}
