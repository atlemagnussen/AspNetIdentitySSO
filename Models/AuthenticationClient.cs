namespace AspNetIdentity.Models;

public record AuthenticationClient
{
    public required string ClientId { get; init; }
}