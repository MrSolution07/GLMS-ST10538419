using GLMS.Web.Models;

namespace GLMS.Web.Services.Observers;

/// <summary>
/// Observer pattern (GoF) — observers react to contract status-change events
/// without coupling to the business logic that triggers them.
/// </summary>
public interface IContractObserver
{
    Task OnStatusChangedAsync(Contract contract, ContractStatus previousStatus, ContractStatus newStatus);
}
