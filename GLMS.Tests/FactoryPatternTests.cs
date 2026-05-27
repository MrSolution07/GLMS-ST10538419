using GLMS.Web.Models;
using GLMS.Web.Services.Factories;

namespace GLMS.Tests;

/// <summary>
/// Tests the Factory Method pattern implementation — ensures each factory
/// produces a correctly configured Contract and that the resolver dispatches correctly.
/// </summary>
public class FactoryPatternTests
{
    private readonly ContractFactoryResolver _resolver;

    public FactoryPatternTests()
    {
        var factories = new IContractFactory[]
        {
            new StandardContractFactory(),
            new PremiumContractFactory(),
            new EnterpriseContractFactory()
        };
        _resolver = new ContractFactoryResolver(factories);
    }

    [Fact(DisplayName = "Standard factory creates contract with Standard service level")]
    public void StandardFactory_SetsCorrectServiceLevel()
    {
        var factory  = _resolver.Resolve(ServiceLevel.Standard);
        var contract = factory.CreateContract(1, "Freight SLA", DateTime.Today, DateTime.Today.AddYears(1));

        Assert.Equal(ServiceLevel.Standard, contract.ServiceLevel);
        Assert.Equal(ContractStatus.Draft, contract.Status);
    }

    [Fact(DisplayName = "Premium factory prefixes title with [PREMIUM]")]
    public void PremiumFactory_PrefixesTitle()
    {
        var factory  = _resolver.Resolve(ServiceLevel.Premium);
        var contract = factory.CreateContract(1, "Express Logistics", DateTime.Today, DateTime.Today.AddYears(1));

        Assert.Equal(ServiceLevel.Premium, contract.ServiceLevel);
        Assert.Contains("[PREMIUM]", contract.Title);
    }

    [Fact(DisplayName = "Enterprise factory prefixes title with [ENTERPRISE]")]
    public void EnterpriseFactory_PrefixesTitle()
    {
        var factory  = _resolver.Resolve(ServiceLevel.Enterprise);
        var contract = factory.CreateContract(1, "Global Operations", DateTime.Today, DateTime.Today.AddYears(1));

        Assert.Equal(ServiceLevel.Enterprise, contract.ServiceLevel);
        Assert.Contains("[ENTERPRISE]", contract.Title);
    }

    [Fact(DisplayName = "All factories produce Draft status by default")]
    public void AllFactories_ProduceDraftStatus()
    {
        foreach (ServiceLevel level in Enum.GetValues<ServiceLevel>())
        {
            var contract = _resolver.Resolve(level)
                .CreateContract(1, "Test", DateTime.Today, DateTime.Today.AddYears(1));

            Assert.Equal(ContractStatus.Draft, contract.Status);
        }
    }

    [Fact(DisplayName = "Resolver throws for unregistered service level")]
    public void Resolver_UnknownLevel_Throws()
    {
        var resolver = new ContractFactoryResolver(Array.Empty<IContractFactory>());

        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(ServiceLevel.Standard));
    }

    [Fact(DisplayName = "Created contract inherits provided ClientId")]
    public void Factory_AssignsClientId()
    {
        var factory  = _resolver.Resolve(ServiceLevel.Standard);
        var contract = factory.CreateContract(42, "Test", DateTime.Today, DateTime.Today.AddYears(1));

        Assert.Equal(42, contract.ClientId);
    }
}
