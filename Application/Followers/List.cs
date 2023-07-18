using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using ApplicationProfile = Application.Profiles.Profile;

namespace Application.Followers
{
    public class List
    {
        public class Query : IRequest<Result<List<ApplicationProfile>>>
        {
            public string Predicate { get; set; }
            public string UserName { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ApplicationProfile>>>
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

            public async Task<Result<List<ApplicationProfile>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var profiles = new List<ApplicationProfile>();

                switch (request.Predicate)
                {
                    case "followers":
                        profiles = await _context.UserFollowings.Where(x => x.Target.UserName == request.UserName)
                            .Select(u => u.Observer)
                            .ProjectTo<ApplicationProfile>(_mapper.ConfigurationProvider, 
                                new {currentUserName = _userAccessor.GetUsername()})
                            .ToListAsync();
                        break;

                    case "following":
                        profiles = await _context.UserFollowings.Where(x => x.Target.UserName == request.UserName)
                            .Select(u => u.Observer)
                            .ProjectTo<ApplicationProfile>(_mapper.ConfigurationProvider,
                                new { currentUserName = _userAccessor.GetUsername() })
                            .ToListAsync();
                        break;
                }

                return Result<List<ApplicationProfile>>.Success(profiles);
            }
        }
    }
}
