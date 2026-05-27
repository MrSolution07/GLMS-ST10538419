namespace GLMS.Web.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    OnHold
}

public enum ServiceLevel
{
    Standard,
    Premium,
    Enterprise
}

public enum ServiceRequestStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}
