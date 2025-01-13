namespace PokerTime.Application.Common.Abstractions {
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEntityStateFacilitator {
        Task Reload(object entity, CancellationToken cancellationToken);
    }
}
