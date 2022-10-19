using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Messages.Info.Queries;
using VideoGameHash.Models;

namespace VideoGameHash.Handlers.Info.Queries
{
    public class InfoQueryHandlers : IQueryHandler<GetInfoAddUrl, AddUrlViewModel>,
        IQueryHandler<GetInfoType, InfoTypeViewModel>,
        IQueryHandler<GetPolls, List<Poll>>
    {
        private readonly VGHDatabaseContainer _db;

        public InfoQueryHandlers(VGHDatabaseContainer db)
        {
            _db = db;
        }

        #region Public Methods

        public async Task<AddUrlViewModel> Handle(GetInfoAddUrl query)
        {
            var urlModel = new AddUrlModel();
            var model = new AddUrlViewModel(urlModel);
            model.Section = new SelectList(await _db.InfoTypes.Select(x => x.InfoTypeName).ToListAsync(), model.Section);
            model.Source = new SelectList(await _db.InfoSources.Select(x => x.InfoSourceName).ToListAsync(), model.Source);
            model.GameSystem = new SelectList(await _db.GameSystems.OrderBy(x => x.GameSystemSortOrder.SortOrder).Select(x => x.GameSystemName).ToListAsync(), model.GameSystem);

            return model;
        }

        public async Task<InfoTypeViewModel> Handle(GetInfoType query)
        {
            return new InfoTypeViewModel
            {
                InfoSources = await _db.InfoSources.OrderBy(x => x.InfoSourceSortOrder.SortOrder).ToListAsync(),
                InfoTypes = await _db.InfoTypes.ToListAsync(),
                InfoSourceRssUrls = await _db.InfoSourceRssUrls.ToListAsync(),
                Polls = await _db.Polls.OrderByDescending(x => x.DateCreated).Take(6).ToListAsync()
            };
        }

        public async Task<List<Poll>> Handle(GetPolls query)
        {
            return await _db.Polls.ToListAsync();
        }

        #endregion

        #region Private Methods


        #endregion
    }
}