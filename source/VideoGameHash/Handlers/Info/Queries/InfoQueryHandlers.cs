using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Messages.Info.Queries;
using VideoGameHash.Models;

namespace VideoGameHash.Handlers.Info.Queries
{
    public class InfoQueryHandlers : IQueryHandler<GetInfoAddUrlViewModel, AddUrlViewModel>
    {
        private readonly VGHDatabaseContainer _db;

        public InfoQueryHandlers(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<AddUrlViewModel> Handle(GetInfoAddUrlViewModel query)
        {
            var urlModel = new AddUrlModel();
            var model = new AddUrlViewModel(urlModel);
            model.Section = new SelectList(await _db.InfoTypes.Select(x => x.InfoTypeName).ToListAsync(), model.Section);
            model.Source = new SelectList(await _db.InfoSources.Select(x => x.InfoSourceName).ToListAsync(), model.Source);
            model.GameSystem = new SelectList(await _db.GameSystems.OrderBy(x => x.GameSystemSortOrder.SortOrder).Select(x => x.GameSystemName).ToListAsync(), model.GameSystem);

            return model;
        }
    }
}