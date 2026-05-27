using GLMS.Web.Models;

namespace GLMS.Web.Services.Factories;

public class EnterpriseContractFactory : IContractFactory
{
    public ServiceLevel SupportedLevel => ServiceLevel.Enterprise;

    public Contract CreateContract(int clientId, string title, DateTime startDate, DateTime endDate)
    {
        return new Contract
        {
            ClientId     = clientId,
            Title        = $"[ENTERPRISE] {title}",
            StartDate    = startDate,
            EndDate      = endDate,
            ServiceLevel = ServiceLevel.Enterprise,
            Status       = ContractStatus.Draft,
            CreatedOn    = DateTime.UtcNow
        };
    }
}
