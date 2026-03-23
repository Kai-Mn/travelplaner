using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Features.Locations;
using TravelPlaner.Application.Features.Tags;

namespace TravelPlaner.Api.Controllers;

[Route("api/locations")]
public class LocationsController(IMediator mediator) : BaseAuthController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetLocationsQuery(CurrentUserId), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new GetLocationQuery(id, CurrentUserId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLocationRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateLocationCommand(request.Name, request.Coordinates, request.Description, CurrentUserId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken ct)
        => Ok(await mediator.Send(
            new UpdateLocationCommand(id, request.Name, request.Coordinates, request.Description, CurrentUserId), ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteLocationCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/tags")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTag(Guid id, [FromBody] AddTagRequest request, CancellationToken ct)
    {
        var tag = await mediator.Send(new AddTagCommand(id, request.Name, CurrentUserId), ct);
        return CreatedAtAction(nameof(GetById), new { id }, tag);
    }

    [HttpDelete("{id:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken ct)
    {
        await mediator.Send(new RemoveTagCommand(id, tagId, CurrentUserId), ct);
        return NoContent();
    }
}
