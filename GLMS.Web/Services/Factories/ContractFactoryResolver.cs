using GLMS.Web.Models;

namespace GLMS.Web.Services.Factories;

/// <summary>
/// Resolves the correct IContractFactory based on ServiceLevel — uses the Factory Method
/// registry pattern with dependency-injected factory implementations.
/// </summary>
public class ContractFactoryResolver
{
    private readonly IEnumerable<IContractFactory> _factories;

    public ContractFactoryResolver(IEnumerable<IContractFactory> factories)
    {
        _factories = factories;
    }

    public IContractFactory Resolve(ServiceLevel level)
    {
        return _factories.FirstOrDefault(f => f.SupportedLevel == level)
               ?? throw new InvalidOperationException($"No factory registered for ServiceLevel: {level}");
    }
}
