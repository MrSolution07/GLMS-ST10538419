using GLMS.Web.Models;

namespace GLMS.Web.Services.Factories;

/// <summary>
/// Factory Method pattern (GoF) — decouples contract creation logic from controllers.
/// Each concrete factory produces a contract variant pre-configured for its service level.
/// </summary>
public interface IContractFactory
{
    Contract CreateContract(int clientId, string title, DateTime startDate, DateTime endDate);
    ServiceLevel SupportedLevel { get; }
}
