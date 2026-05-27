using GLMS.Web.Models;
using Microsoft.Extensions.Logging;

namespace GLMS.Web.Services.Observers;

/// <summary>
/// Writes a structured audit entry every time a contract status changes.
/// </summary>
public class AuditLogObserver : IContractObserver
{
    private readonly ILogger<AuditLogObserver> _logger;

    public AuditLogObserver(ILogger<AuditLogObserver> logger)
    {
        _logger = logger;
    }

    public Task OnStatusChangedAsync(Contract contract, ContractStatus previousStatus, ContractStatus newStatus)
    {
        _logger.LogInformation(
            "[AUDIT] Contract #{Id} '{Title}' transitioned from {Prev} to {New} at {Time} UTC.",
            contract.Id, contract.Title, previousStatus, newStatus, DateTime.UtcNow);

        return Task.CompletedTask;
    }
}
