using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<PagedList<ActivityDTO>>> 
        {
            public ActivityParams Info { get; set; }
        }
        public class Handler : IRequestHandler<Query, Result<PagedList<ActivityDTO>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
            }

            public async Task<Result<PagedList<ActivityDTO>>> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<ActivityDTO> query = _context.Activities
                    .Where(x => x.Date >= request.Info.StartDate)
                    .OrderBy(d => d.Date)
                    .ProjectTo<ActivityDTO>(_mapper.ConfigurationProvider
                    , new { currentUserName = _userAccessor.GetUsername() })
                    .AsQueryable();

                if(request.Info.IsGoing && !request.Info.IsHost)
                {
                    query = query.Where(x => x.Attendees.Any(a => a.Username == _userAccessor.GetUsername()));
                }

                if(request.Info.IsHost && !request.Info.IsGoing)
                {
                    query = query.Where(x => x.HostUsername == _userAccessor.GetUsername());
                }

                return Result<PagedList<ActivityDTO>>
                    .Success(await PagedList<ActivityDTO>.CreateAsync
                    (
                        query,
                        request.Info.PageNumber,
                        request.Info.PageSize
                    ));
            }
        }
    }
}
