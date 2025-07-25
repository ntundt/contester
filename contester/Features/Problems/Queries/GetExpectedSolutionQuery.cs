﻿using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Problems.Exceptions;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Queries;

public class GetExpectedSolutionQuery : IRequest<ExpectedSolutionDto>
{
    public Guid ProblemId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetExpectedSolutionQueryHandler(
    ApplicationDbContext dbContext,
    IMapper mapper,
    IPermissionService permissionService,
    IFileService fileService)
    : IRequestHandler<GetExpectedSolutionQuery, ExpectedSolutionDto>
{
    private readonly IMapper _mapper = mapper;

    public async Task<ExpectedSolutionDto> Handle(GetExpectedSolutionQuery request, CancellationToken cancellationToken)
    {
        var problem = await dbContext.Problems
            .Include(p => p.Contest)
            .Include(p => p.SchemaDescription)
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageAttempts, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageAttempts);
        }

        return new ExpectedSolutionDto
        {
            ProblemId = problem.Id,
            Dbms = problem.SolutionDbms,
            Solution = await fileService.ReadApplicationDirectoryFileAllTextAsync(problem.SolutionPath, cancellationToken),
        };
    }
}
