namespace PokerTime.Domain.Entities {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using Services;
    using ValueObjects;

    /// <summary>
    /// A session consists of notes created by participants. A session has a unique identifier.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore")]
    public class Session {
        private ICollection<Participant> _participants;

        private readonly SessionIdentifier _urlId = SessionIdentifierService.CreateNewInternal();

        public int Id { get; set; }

        /// <summary>
        /// Identifier (random string) of the session
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public SessionIdentifier UrlId => _urlId;

        /// <summary>
        /// Gets or sets the current stage of the session
        /// </summary>
        public SessionStage CurrentStage { get; set; }

        /// <summary>
        /// Gets the optional hashed passphrase necessary to access the session lobby
        /// </summary>
        public string HashedPassphrase { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Title { get; set; }

        /// <summary>
        /// Gets the passphrase used for the facilitator to log into the session lobby
        /// </summary>
        public string FacilitatorHashedPassphrase { get; set; }

        public SymbolSet SymbolSet { get; set; }

        public int SymbolSetId { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ICollection<Participant> Participants => _participants ??= new Collection<Participant>();

        public DateTimeOffset CreationTimestamp { get; set; }
    }
}
