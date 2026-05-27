using GLMS.Web.Models;

namespace GLMS.Web.Services.Factories;

public class StandardContractFactory : IContractFactory
{
    public ServiceLevel SupportedLevel => ServiceLevel.Standard;

    public Contract CreateContract(int clientId, string title, DateTime startDate, DateTime endDate)
    {
        return new Contract
        {
            ClientId   = clientId,
            Title      = title,
            StartDate  = startDate,
            EndDate    = endDate,
            ServiceLevel = ServiceLevel.Standard,
            Status     = ContractStatus.Draft,
            CreatedOn  = DateTime.UtcNow
        };
    }
}
