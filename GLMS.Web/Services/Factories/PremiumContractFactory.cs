using GLMS.Web.Models;

namespace GLMS.Web.Services.Factories;

public class PremiumContractFactory : IContractFactory
{
    public ServiceLevel SupportedLevel => ServiceLevel.Premium;

    public Contract CreateContract(int clientId, string title, DateTime startDate, DateTime endDate)
    {
        return new Contract
        {
            ClientId     = clientId,
            Title        = $"[PREMIUM] {title}",
            StartDate    = startDate,
            EndDate      = endDate,
            ServiceLevel = ServiceLevel.Premium,
            Status       = ContractStatus.Draft,
            CreatedOn    = DateTime.UtcNow
        };
    }
}
