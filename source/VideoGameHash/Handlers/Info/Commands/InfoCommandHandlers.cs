using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.Info.Commands;
using VideoGameHash.Models;

namespace VideoGameHash.Handlers.Info.Commands
{
    public class InfoCommandHandlers : 
        ICommandHandler<AddInfoType>,
        ICommandHandler<AddInfoSource>,
        ICommandHandler<AddUrl>
    {
        private readonly VGHDatabaseContainer _db;

        public InfoCommandHandlers(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task Handle(AddInfoType command)
        {
            var type = await GetInfoType(command.Type);
            if (type != null) return;

            var infoType = new InfoType
            {
                InfoTypeName = command.Type,
                UseGameSystem = true
            };
            _db.InfoTypes.Add(infoType);
            await _db.SaveChangesAsync();

            int? maxValue = _db.InfoTypeSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
            var order = new InfoTypeSortOrder();
            infoType = await GetInfoType(command.Type);
            order.Id = infoType.Id;
            order.InfoType = infoType;
            order.SortOrder = (int)(maxValue + 1);

            _db.InfoTypeSortOrders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(AddInfoSource command)
        {
            var info = await _db.GameSystems.SingleOrDefaultAsync(u => u.GameSystemName == command.Name);
            if (info != null) return;

            var infoSource = new InfoSource {InfoSourceName = command.Name};

            _db.InfoSources.Add(infoSource);

            await _db.SaveChangesAsync();
            
            var maxValue = _db.InfoSourceSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
            var order = new InfoSourceSortOrder
            {
                Id = infoSource.Id,
                InfoSource = infoSource,
                SortOrder = maxValue + 1
            };

            _db.InfoSourceSortOrders.Add(order);
        }

        public async Task Handle(AddUrl command)
        {
            var url = new InfoSourceRssUrls
            {
                InfoTypeId = await GetInfoTypeId(command.Section),
                InfoSourceId = await GetInfoSourceId(command.Source),
                GameSystemId = await GetGameSystemId(command.GameSystem),
                URL = command.Url
            };

            _db.InfoSourceRssUrls.Add(url);
        }

        #region Private Methods

        private async Task<int> GetInfoTypeId(string infoType)
        {
            return (await _db.InfoTypes.SingleAsync(u => u.InfoTypeName == infoType)).Id;
        }

        private async Task<InfoType> GetInfoType(string name)
        {
            return await _db.InfoTypes.SingleOrDefaultAsync(u => u.InfoTypeName == name);
        }

        private async Task<int> GetInfoSourceId(string source)
        {
            return (await _db.InfoSources.SingleAsync(u => u.InfoSourceName == source)).Id;
        }

        private async Task<int> GetGameSystemId(string gameSystem)
        {
            return (await _db.GameSystems.SingleAsync(u => u.GameSystemName == gameSystem)).Id;
        }

        #endregion
    }
}