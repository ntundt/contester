using System.Security.Claims;
using diploma.Features.AttachedFiles.Commands;
using diploma.Features.AttachedFiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.AttachedFiles;

[ApiController]
[Route("api/file")]
public class AttachedFileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly Features.Authentication.Services.IAuthorizationService _authorizationService;
    
    public AttachedFileController(IMediator mediator, Features.Authentication.Services.IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<CreateAttachedFileCommandResult> SaveFileAsync([FromForm] CreateAttachedFileCommand command, CancellationToken cancellationToken)
    {
        command.CallerId = _authorizationService.GetUserId();
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpGet("{fileId}")]
    public async Task<IActionResult> GetFileAsync([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAttachedFileQuery { FileId = fileId }, cancellationToken);
        return File(result.File, "application/octet-stream", result.FileName);
    }
}
