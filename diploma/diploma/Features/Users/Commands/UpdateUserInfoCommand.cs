using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using FluentValidation;
using FluentValidation.Results;
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

public class UpdateUserInfoCommandValidator : AbstractValidator<UpdateUserInfoCommand>
{
    public UpdateUserInfoCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Patronymic).MaximumLength(50);
        RuleFor(x => x.AdditionalInfo).NotEmpty()
            .MaximumLength(150);
    }
}

public class UpdateUserInfoCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IPermissionService permissionService,
    IValidator<UpdateUserInfoCommand> validator)
    : IRequestHandler<UpdateUserInfoCommand>
{
    private readonly IMapper _mapper = mapper;
    private readonly IPermissionService _permissionService = permissionService;

    public async Task Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = await context.Users.FindAsync(request.CallerId, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Patronymic = request.Patronymic;
        user.AdditionalInfo = request.AdditionalInfo;
        await context.SaveChangesAsync(cancellationToken);
    }
}
