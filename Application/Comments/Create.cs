using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class Create
    {
        public class Command : IRequest<Result<CommentDTO>>
        {
            public string Body { get; set; }
            public Guid ActivityId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Body).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<CommentDTO>>
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
            public async Task<Result<CommentDTO>> Handle(Command request, CancellationToken cancellationToken)
            {
                Activity activity = await _context.Activities
                    .Include(x => x.Comments)
                    .ThenInclude(x => x.Author)
                    .ThenInclude(x => x.Photos)
                    .FirstOrDefaultAsync(x => x.Id == request.ActivityId);

                if (activity == null) return null;

                AppUser user = await _context.Users
                    .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

                Comment comment = new Comment
                {
                    Author = user,
                    Activity = activity,
                    Body = request.Body
                };

                activity.Comments.Add(comment);

                bool success = await _context.SaveChangesAsync() > 0;

                if (!success) return Result<CommentDTO>.Failure("Failed to add comment");

                return Result<CommentDTO>.Success(_mapper.Map<CommentDTO>(comment));
            }
        }
    }
}
