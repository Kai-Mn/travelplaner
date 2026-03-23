using MediatR;
using TravelPlaner.Application.DTOs;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Domain.Entities;
using TravelPlaner.Domain.Exceptions;

namespace TravelPlaner.Application.Features.Auth;

public record RegisterCommand(string Email, string Password) : IRequest<AuthResponse>;

public class RegisterCommandHandler(IUserRepository userRepo, IAuthService authService)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existing = await userRepo.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var hash = authService.HashPassword(request.Password);
        var user = User.Create(request.Email, hash);
        await userRepo.AddAsync(user, ct);
        await userRepo.SaveChangesAsync(ct);

        var token = authService.GenerateToken(user.Id, user.Email);
        return new AuthResponse(token, user.Email, user.Id);
    }
}
