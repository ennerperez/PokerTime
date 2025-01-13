namespace PokerTime.Common
{
    using System;

    public interface ISystemClock
    {
        DateTime Now { get; }

        DateTimeOffset CurrentTimeOffset { get; }
    }
}
