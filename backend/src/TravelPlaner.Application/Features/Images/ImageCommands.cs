using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Images;

public record UploadImageCommand(Guid LocationId, Stream ImageStream, string FileName, Guid UserId)
    : IRequest<ImageDto>;

public class UploadImageCommandHandler(ILocationRepository locationRepo, IImageStore imageStore)
    : IRequestHandler<UploadImageCommand, ImageDto>
{
    public async Task<ImageDto> Handle(UploadImageCommand request, CancellationToken ct)
    {
        var location = await locationRepo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        var filePath = await imageStore.SaveAsync(request.ImageStream, request.FileName, ct);
        var image = location.AddImage(filePath);
        await locationRepo.SaveChangesAsync(ct);

        return new ImageDto(image.Id, imageStore.GetPublicUrl(filePath));
    }
}

public record DeleteImageCommand(Guid LocationId, Guid ImageId, Guid UserId) : IRequest;

public class DeleteImageCommandHandler(ILocationRepository locationRepo, IImageStore imageStore)
    : IRequestHandler<DeleteImageCommand>
{
    public async Task Handle(DeleteImageCommand request, CancellationToken ct)
    {
        var location = await locationRepo.GetByIdAsync(request.LocationId, ct)
            ?? throw new NotFoundException(nameof(Location), request.LocationId);

        if (location.UserId != request.UserId)
            throw new UnauthorizedException();

        var image = location.Images.FirstOrDefault(i => i.Id == request.ImageId)
            ?? throw new NotFoundException(nameof(Image), request.ImageId);

        await imageStore.DeleteAsync(image.FilePath, ct);

        // Remove from DB via EF cascade — handled by deleting entity in repo
        await locationRepo.DeleteImageAsync(image, ct);
        await locationRepo.SaveChangesAsync(ct);
    }
}
