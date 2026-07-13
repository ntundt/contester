using contester.Features.SchemaDescriptions.Commands;
using contester.Features.SchemaDescriptions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.SchemaDescriptions;

[Authorize]
[ApiController]
[Route("api/schema-descriptions")]
public class SchemaDescriptionController(IMediator mediator)
{
    [HttpGet]
    public async Task<GetSchemaDescriptionsQueryResult> GetSchemaDescriptions([FromQuery] GetSchemaDescriptionsQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPut]
    [Route("{schemaDescriptionId:guid}")]
    public async Task<SchemaDescriptionDto> UpdateSchemaDescription([FromRoute] Guid schemaDescriptionId, UpdateSchemaDescriptionCommand command)
    {
        command.Id = schemaDescriptionId;
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpPost]
    public async Task<SchemaDescriptionDto> CreateSchemaDescription([FromBody] CreateSchemaDescriptionCommand command)
    {
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpDelete]
    [Route("{schemaDescriptionId:guid}")]
    public async Task DeleteSchemaDescription([FromRoute] Guid schemaDescriptionId)
    {
        var command = new DeleteSchemaDescriptionCommand
        {
            Id = schemaDescriptionId,
        };
        await mediator.Send(command);
    }
    
    [HttpGet]
    [Route("{schemaDescriptionId:guid}/files")]
    public async Task<GetSchemaDescriptionFilesQueryResult> GetSchemaDescriptionFiles([FromRoute] Guid schemaDescriptionId)
    {
        var query = new GetSchemaDescriptionFilesQuery
        {
            SchemaDescriptionId = schemaDescriptionId,
        };
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPut]
    [Route("{schemaDescriptionId:guid}/files/{dbms}")]
    public async Task<SchemaDescriptionFileDto> UpdateSchemaDescriptionFile([FromRoute] Guid schemaDescriptionId, [FromRoute] string dbms, [FromBody] UpdateSchemaDescriptionFileCommand command)
    {
        command.SchemaDescriptionId = schemaDescriptionId;
        command.Dbms = dbms;
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpPost]
    [Route("{schemaDescriptionId:guid}/files")]
    public async Task<SchemaDescriptionFileDto> CreateSchemaDescriptionFile([FromRoute] Guid schemaDescriptionId, [FromBody] CreateSchemaDescriptionFileCommand command)
    {
        command.SchemaDescriptionId = schemaDescriptionId;
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpDelete]
    [Route("{schemaDescriptionId:guid}/files/{dbms}")]
    public async Task<IActionResult> DeleteSchemaDescriptionFile([FromRoute] Guid schemaDescriptionId, [FromRoute] string dbms)
    {
        var command = new DeleteSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schemaDescriptionId,
            Dbms = dbms,
        };
        
        await mediator.Send(command);
        return new OkResult();
    }
}
