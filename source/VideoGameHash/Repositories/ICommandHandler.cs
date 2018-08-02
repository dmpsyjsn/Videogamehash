using System.Threading.Tasks;

namespace VideoGameHash.Repositories
{
    public interface ICommandHandler<in TCommand>
    {
        Task Handle(TCommand command);
    }
}