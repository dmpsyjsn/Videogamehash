using System.Threading.Tasks;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Decorators
{
    public class TransactionCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> _decorated;
        private readonly VGHDatabaseContainer _db;

        public TransactionCommandHandlerDecorator(ICommandHandler<TCommand> decorated, VGHDatabaseContainer db)
        {
            _decorated = decorated;
            _db = db;
        }

        public async Task Handle(TCommand command)
        {
            await _decorated.Handle(command);

            await _db.SaveChangesAsync();
        }
    }
}