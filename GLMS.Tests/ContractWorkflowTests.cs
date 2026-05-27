using GLMS.Web.Models;
using GLMS.Web.Services.Strategies;

namespace GLMS.Tests;

/// <summary>
/// Unit tests for ServiceRequestValidationStrategy — verifies the core business rule:
/// service requests may only be raised against Active contracts.
/// </summary>
public class ContractWorkflowTests
{
    private readonly ServiceRequestValidationStrategy _sut = new();

    private static Contract MakeContract(ContractStatus status) => new()
    {
        Id           = 1,
        Title        = "Test Contract",
        ClientId     = 1,
        StartDate    = DateTime.Today.AddDays(-30),
        EndDate      = DateTime.Today.AddDays(335),
        Status       = status,
        ServiceLevel = ServiceLevel.Standard
    };

    private static ServiceRequest MakeRequest(decimal costUsd = 500m) => new()
    {
        ContractId  = 1,
        Description = "Route optimisation service",
        CostUsd     = costUsd,
        Status      = ServiceRequestStatus.Pending
    };

    // ── Happy-path ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "Active contract allows service request")]
    public void ActiveContract_RequestIsValid()
    {
        var contract = MakeContract(ContractStatus.Active);
        var request  = MakeRequest();

        var result = _sut.Validate((contract, request));

        Assert.True(result.IsValid);
    }

    // ── Expired contract ───────────────────────────────────────────────────

    [Fact(DisplayName = "Expired contract blocks service request")]
    public void ExpiredContract_RequestIsBlocked()
    {
        var contract = MakeContract(ContractStatus.Expired);
        var request  = MakeRequest();

        var result = _sut.Validate((contract, request));

        Assert.False(result.IsValid);
        Assert.Contains("expired", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // ── On-Hold contract ───────────────────────────────────────────────────

    [Fact(DisplayName = "OnHold contract blocks service request")]
    public void OnHoldContract_RequestIsBlocked()
    {
        var contract = MakeContract(ContractStatus.OnHold);
        var request  = MakeRequest();

        var result = _sut.Validate((contract, request));

        Assert.False(result.IsValid);
        Assert.Contains("hold", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    // ── Draft contract ─────────────────────────────────────────────────────

    [Fact(DisplayName = "Draft contract blocks service request")]
    public void DraftContract_RequestIsBlocked()
    {
        var contract = MakeContract(ContractStatus.Draft);
        var request  = MakeRequest();

        var result = _sut.Validate((contract, request));

        Assert.False(result.IsValid);
        Assert.Contains("Draft", result.ErrorMessage);
    }

    // ── Edge cases ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "Zero-cost request is rejected even against Active contract")]
    public void ZeroCostRequest_IsRejected()
    {
        var contract = MakeContract(ContractStatus.Active);
        var request  = MakeRequest(costUsd: 0m);

        var result = _sut.Validate((contract, request));

        Assert.False(result.IsValid);
        Assert.Contains("zero", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Negative-cost request is rejected")]
    public void NegativeCostRequest_IsRejected()
    {
        var contract = MakeContract(ContractStatus.Active);
        var request  = MakeRequest(costUsd: -100m);

        var result = _sut.Validate((contract, request));

        Assert.False(result.IsValid);
    }

    [Theory(DisplayName = "Non-Active statuses all block service requests")]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void NonActiveStatuses_AllBlock(ContractStatus status)
    {
        var contract = MakeContract(status);
        var request  = MakeRequest();

        var result = _sut.Validate((contract, request));

        Assert.False(result.IsValid);
    }
}
