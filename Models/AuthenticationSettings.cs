namespace AspNetIdentity.Models;

public record AuthenticationSettings
{
    public bool EnableRegistration { get; init; }
    public required AuthenticationClient Microsoft { get; set; }
}