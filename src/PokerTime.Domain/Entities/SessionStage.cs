namespace PokerTime.Domain.Entities {
    public enum SessionStage {
        /// <summary>
        /// The session is not started yet and waiting for a facilitator to appear
        /// </summary>
        NotStarted,

        /// <summary>
        /// The session is underway: participants are currently writing down their findings. Notes are private to the creator.
        /// </summary>
        Discussion,

        /// <summary>
        /// The session is underway: written down notes are currently being discussed. Notes cannot be modified anymore. Notes are public.
        /// </summary>
        Estimation,

        /// <summary>
        /// The session is underway: notes are currently being grouped by the facilitator.
        /// </summary>
        EstimationDiscussion,

        /// <summary>
        /// The session has been finished
        /// </summary>
        Finished
    }
}
