using System.Collections.Generic;
using VideoGameHash.Handlers;
using VideoGameHash.Models;

namespace VideoGameHash.Messages.Info.Queries
{
    public class GetInfoAddUrl : IQuery<AddUrlViewModel> { }

    public class GetInfoType : IQuery<InfoTypeViewModel> { }

    public class GetPolls : IQuery<List<Poll>> { }
}