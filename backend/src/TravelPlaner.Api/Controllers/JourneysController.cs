using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Features.Journeys;

namespace TravelPlaner.Api.Controllers;

[Route("api/journeys")]
public class JourneysController(IMediator mediator) : BaseAuthController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JourneySummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await mediator.Send(new GetJourneysQuery(CurrentUserId), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JourneyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new GetJourneyQuery(id, CurrentUserId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(JourneySummaryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateJourneyRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateJourneyCommand(request.Name, request.Description, CurrentUserId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JourneySummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJourneyRequest request, CancellationToken ct)
        => Ok(await mediator.Send(
            new UpdateJourneyCommand(id, request.Name, request.Description, CurrentUserId), ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteJourneyCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/locations/{locationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddLocation(Guid id, Guid locationId, CancellationToken ct)
    {
        await mediator.Send(new AddLocationToJourneyCommand(id, locationId, CurrentUserId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/locations/{locationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveLocation(Guid id, Guid locationId, CancellationToken ct)
    {
        await mediator.Send(new RemoveLocationFromJourneyCommand(id, locationId, CurrentUserId), ct);
        return NoContent();
    }
}
