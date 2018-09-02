using System.Threading.Tasks;

namespace VideoGameHash.Handlers
{
    public interface ICommandHandler<in TCommand>
    {
        Task Handle(TCommand command);
    }
}