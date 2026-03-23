using MediatR;
using Microsoft.AspNetCore.Mvc;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Features.Images;

namespace TravelPlaner.Api.Controllers;

[Route("api/locations/{locationId:guid}/images")]
public class ImagesController(IMediator mediator) : BaseAuthController
{
    [HttpPost]
    [ProducesResponseType(typeof(ImageDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Upload(Guid locationId, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new UploadImageCommand(locationId, stream, file.FileName, CurrentUserId), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpDelete("{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid locationId, Guid imageId, CancellationToken ct)
    {
        await mediator.Send(new DeleteImageCommand(locationId, imageId, CurrentUserId), ct);
        return NoContent();
    }
}
