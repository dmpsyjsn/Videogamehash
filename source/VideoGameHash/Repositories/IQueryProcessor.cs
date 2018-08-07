using System.Diagnostics;
using System.Threading.Tasks;
using SimpleInjector;

namespace VideoGameHash.Repositories
{
    public interface IQueryProcessor
    {
        Task<TResult> Process<TResult>(IQuery<TResult> query);
    }

    sealed class QueryProcessor : IQueryProcessor
    {
        private readonly Container container;

        public QueryProcessor(Container container)
        {
            this.container = container;
        }

        [DebuggerStepThrough]
        public Task<TResult> Process<TResult>(IQuery<TResult> query)
        {
            var handlerType =
                typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = container.GetInstance(handlerType);

            return handler.Handle((dynamic)query);
        }
    }
    
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query);
    }

    public interface IQuery<TResult>
    {
    }
}