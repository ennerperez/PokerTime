namespace PokerTime.Application.Common.Abstractions {
    public interface IPokerTimeDbContextFactory {
        /// <summary>
        ///     Creates a disposable copy for edit operations
        /// </summary>
        /// <returns></returns>
        IPokerTimeDbContext CreateForEditContext();
    }
}
