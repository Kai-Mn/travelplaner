namespace TravelPlaner.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class NotFoundException(string entityName, object id)
    : DomainException($"{entityName} with id '{id}' was not found.");

public class UnauthorizedException(string message = "Unauthorized access.")
    : DomainException(message);

public class ConflictException(string message)
    : DomainException(message);
