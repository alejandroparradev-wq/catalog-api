using CatalogApi.Specs.Infrastructure;
using CatalogApi.Specs.Steps;
using Reqnroll;

namespace CatalogApi.Specs.Support;

/// <summary>
/// Objeto "mundo" por escenario: agrupa los objetos que los pasos comparten
/// (cliente HTTP, contexto, acciones). Se guardan en el ScenarioContext, que
/// Reqnroll si inyecta siempre; esto evita depender de APIs de DI de versiones
/// específicas de Reqnroll.
/// </summary>
public sealed class TestRuntime
{
    private readonly ScenarioContext _scenario;
    private readonly Lazy<ApiContext> _context;
    private readonly Lazy<HttpClient> _client;

    public TestRuntime(ScenarioContext scenario)
    {
        _scenario = scenario;
        _context = new Lazy<ApiContext>(() => GetOrCreate("ApiContext", () => new ApiContext()));
        _client = new Lazy<HttpClient>(() => GetOrCreate("HttpClient", TestFactory.CreateClient));
    }

    public ApiContext Context => _context.Value;

    public HttpClient Client => _client.Value;

    public ApiActions Actions => new(Client, Context);

    private T GetOrCreate<T>(string key, Func<T> factory) where T : class
    {
        if (_scenario.ContainsKey(key))
            return (T)_scenario[key];
        var value = factory();
        _scenario[key] = value;
        return value;
    }
}