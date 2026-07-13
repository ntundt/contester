using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Users.Exceptions;
using contester.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace contester.Features.Users.Commands;

public class UpdateUserInfoCommand : IRequest, IAuthenticatedRequest
{
    [JsonIgnore]
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
    ApplicationDbContext context)
    : IRequestHandler<UpdateUserInfoCommand>
{
    public async Task Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
    {
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
