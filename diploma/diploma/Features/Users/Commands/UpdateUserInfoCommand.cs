using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using MediatR;

namespace diploma.Features.Users.Commands;

public class UpdateUserInfoCommand : IRequest
{
    public Guid CallerId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string AdditionalInfo { get; set; } = null!;
}

public class UpdateUserInfoCommandHandler : IRequestHandler<UpdateUserInfoCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;
    
    public UpdateUserInfoCommandHandler(ApplicationDbContext context, IMapper mapper, IClaimService claimService)
    {
        _context = context;
        _mapper = mapper;
        _claimService = claimService;
    }
    
    public async Task Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(request.CallerId);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Patronymic = request.Patronymic;
        user.AdditionalInfo = request.AdditionalInfo;
        await _context.SaveChangesAsync(cancellationToken);
    }
}