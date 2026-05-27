using GLMS.Web.Models;
using Microsoft.Extensions.Logging;

namespace GLMS.Web.Services.Observers;

/// <summary>
/// Simulates sending an email notification when a contract becomes Active or Expired.
/// In production this would call an SMTP or SendGrid service.
/// </summary>
public class EmailNotificationObserver : IContractObserver
{
    private readonly ILogger<EmailNotificationObserver> _logger;

    public EmailNotificationObserver(ILogger<EmailNotificationObserver> logger)
    {
        _logger = logger;
    }

    public Task OnStatusChangedAsync(Contract contract, ContractStatus previousStatus, ContractStatus newStatus)
    {
        if (newStatus is ContractStatus.Active or ContractStatus.Expired)
        {
            _logger.LogInformation(
                "[EMAIL NOTIFICATION] Contract #{Id} is now {Status}. Client contact: {Email}.",
                contract.Id, newStatus, contract.Client?.ContactEmail ?? "unknown");
        }

        return Task.CompletedTask;
    }
}
