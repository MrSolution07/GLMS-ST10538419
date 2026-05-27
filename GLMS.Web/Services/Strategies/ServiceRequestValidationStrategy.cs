using GLMS.Web.Models;

namespace GLMS.Web.Services.Strategies;

/// <summary>
/// Validates that a ServiceRequest may only be raised against an Active contract.
/// Expired or OnHold contracts must be rejected — core business rule from Part 1.
/// </summary>
public class ServiceRequestValidationStrategy : IValidationStrategy<(Contract contract, ServiceRequest request)>
{
    public ValidationResult Validate((Contract contract, ServiceRequest request) subject)
    {
        var (contract, request) = subject;

        if (contract.Status == ContractStatus.Expired)
            return ValidationResult.Failure(
                $"Cannot raise a service request: Contract '{contract.Title}' has expired.");

        if (contract.Status == ContractStatus.OnHold)
            return ValidationResult.Failure(
                $"Cannot raise a service request: Contract '{contract.Title}' is currently on hold.");

        if (contract.Status == ContractStatus.Draft)
            return ValidationResult.Failure(
                $"Cannot raise a service request: Contract '{contract.Title}' is still in Draft status.");

        if (request.CostUsd <= 0)
            return ValidationResult.Failure("Cost (USD) must be greater than zero.");

        return ValidationResult.Success();
    }
}
