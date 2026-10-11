namespace CatalogApi.Application;

/// <summary>
/// Representa el incumplimiento de una regla de negocio.
/// Lleva el código HTTP y el mensaje exacto que debe ver el cliente.
/// El pipeline la traduce a una respuesta JSON {"message": "..."} con ese status.
/// </summary>
public sealed class DomainRuleException : Exception
{
    public int StatusCode { get; }

    public DomainRuleException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}