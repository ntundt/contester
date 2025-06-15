using System.Security.Claims;
using diploma.Features.AttachedFiles.Commands;
using diploma.Features.AttachedFiles.Queries;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.AttachedFiles;

[ApiController]
[Route("api/file")]
public class AttachedFileController(
    IMediator mediator,
    AuthorizationService authorizationService)
    : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<CreateAttachedFileCommandResult> SaveFileAsync([FromForm] CreateAttachedFileCommand command, CancellationToken cancellationToken)
    {
        command.CallerId = authorizationService.GetUserId();
        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpGet("{fileId}")]
    public async Task<IActionResult> GetFileAsync([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAttachedFileQuery { FileId = fileId }, cancellationToken);
        return File(result.File, "application/octet-stream", result.FileName);
    }
}
